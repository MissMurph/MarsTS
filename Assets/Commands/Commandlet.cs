using MarsTS.Events;
using MarsTS.Teams;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace MarsTS.Commands {

    public abstract class Commandlet : NetworkBehaviour {

		public abstract Type TargetType { get; }
		public string Name { get; protected set; }
		public Faction Commander { get; protected set; }
		public UnityEvent<CommandCompleteEvent> Callback = new UnityEvent<CommandCompleteEvent>();
		public virtual CommandFactory Command { get { return CommandRegistry.Get(Name); } }

		public virtual void OnStart (CommandQueue queue, CommandStartEvent _event) {

		}

		public virtual void OnActivate (CommandQueue queue, CommandActiveEvent _event) {

		}

		public virtual void OnComplete (CommandQueue queue, CommandCompleteEvent _event) {
			Callback.Invoke(_event);
		}

		public virtual bool CanInterrupt () {
			return true;
		}

		public Commandlet<T> Get<T> () {
			if (typeof(T).Equals(TargetType)) return this as Commandlet<T>;
			else throw new ArgumentException("Commandlet target type " + TargetType + " does not match given type " + typeof(T) + ", cannot return Commandlet!");
		}

		public abstract Commandlet Clone ();

		//Making virtual while testing
		protected virtual ISerializedCommand Serialize () { throw new NotImplementedException(); }
		protected virtual void Deserialize (SerializedWrapper _data) { }

		protected virtual void SpawnAndSync () {
			ISerializedCommand data = Serialize();

			GetComponent<NetworkObject>().Spawn();
			SynchronizeClientRpc(new SerializedWrapper() { commandletData = data });
		}

		[Rpc(SendTo.NotServer)]
		protected virtual void SynchronizeClientRpc (SerializedWrapper _data) {
			Deserialize(_data);
		}
	}

	public abstract class Commandlet<T> : Commandlet {

		public T Target { get { return target; } }

		[SerializeField]
		protected T target;

		public override Type TargetType { get { return typeof(T); } }

		public virtual void Init (string _name, T _target, Faction _commander) {
			Name = _name;
			target = _target;
			Commander = _commander;

			SpawnAndSync();
		}

		public override Commandlet Clone () {
			//return new Commandlet<T>(Name, Target, Commander);
			throw new NotImplementedException();
		}
	}

	public struct SerializedWrapper : INetworkSerializable {

		public ISerializedCommand commandletData;

		public void NetworkSerialize<T> (BufferSerializer<T> serializer) where T : IReaderWriter {
			commandletData.NetworkSerialize(serializer);
		}
	}

	public interface ISerializedCommand : INetworkSerializable {

	}
}