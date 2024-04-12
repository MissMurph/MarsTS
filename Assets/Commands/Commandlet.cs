using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Teams;
using MarsTS.Networking;
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
		public virtual CommandFactory Command { get { return CommandRegistry.Get(Key); } }
		public abstract string Key { get; }
		protected List<string> commandedUnits = new List<string>();

		public virtual void OnStart (CommandQueue queue, CommandStartEvent _event) {
			commandedUnits.Add(queue.gameObject.name);
		}

		public virtual void OnActivate (CommandQueue queue, CommandActiveEvent _event) {

		}

		public virtual void OnComplete (CommandQueue queue, CommandCompleteEvent _event) {
			commandedUnits.Remove(queue.gameObject.name);
			Callback.Invoke(_event);

			if (commandedUnits.Count <= 0) Destroy(gameObject);
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
		protected virtual ISerializedCommand Serialize () { 
			return Serializers.Write(this);
		}
		protected virtual void Deserialize (SerializedCommandWrapper _data) { }

		protected virtual void SpawnAndSync () {
			ISerializedCommand data = Serialize();

			GetComponent<NetworkObject>().Spawn();
			SynchronizeClientRpc(new SerializedCommandWrapper() { commandletData = data });
		}

		[Rpc(SendTo.NotServer)]
		protected virtual void SynchronizeClientRpc (SerializedCommandWrapper _data) {
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
			//return CommandRegistry.Get<CommandFactory<T>>(Key).Construct;
		}
	}
}