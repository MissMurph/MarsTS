using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Commands.Cache;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Teams;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Commands {

    public abstract class Commandlet : NetworkBehaviour {

		public abstract Type TargetType { get; }
		public string Name { get; protected set; }
		public Faction Commander { get; protected set; }
		public UnityEvent<CommandCompleteEvent> Callback = new UnityEvent<CommandCompleteEvent>();
		public virtual CommandFactory Command => CommandPrimer.Get(Name);
		public abstract string SerializerKey { get; }
		public List<string> commandedUnits = new List<string>();
		public int Id { get; protected set; } = 0;
		public bool IsStale => CommandletsCache.IsStale(Id);

		protected void InternalInit(string name, Faction commander)
		{
			Name = name;
			Commander = commander;
		}

		public virtual void StartCommand (EventAgent eventAgent, ICommandable unit) {
			commandedUnits.Add(unit.GameObject.name);
			eventAgent.Local(new CommandStartEvent(eventAgent, this, unit));
		}

		public virtual void ActivateCommand (CommandQueue queue, CommandActiveEvent _event) {

		}

		public virtual void CompleteCommand (EventAgent eventAgent, ICommandable unit, bool isCancelled = false) 
		{
			commandedUnits.Remove(unit.GameObject.name);
			Callback.Invoke(new CommandCompleteEvent(eventAgent, this, isCancelled, unit));
		}

		public virtual bool CanInterrupt () {
			return true;
		}

		public Commandlet<T> Get<T> ()
		{
			if (typeof(T).Equals(TargetType)) return this as Commandlet<T>;
			throw new ArgumentException("Commandlet target type " + TargetType + " does not match given type " + typeof(T) + ", cannot return Commandlet!");
		}

		public abstract Commandlet Clone ();

		//Making virtual while testing
		protected virtual ISerializedCommand Serialize () => CommandSerializers.Write(this);

		protected virtual void SpawnAndSync () {
			ISerializedCommand data = Serialize();

			GetComponent<NetworkObject>().Spawn();
			SynchronizeClientRpc(new SerializedCommandWrapper() { commandletData = data });
		}

		[Rpc(SendTo.NotServer)]
		protected virtual void SynchronizeClientRpc (SerializedCommandWrapper _data) {
			Deserialize(_data);
		}

		protected virtual void Deserialize(SerializedCommandWrapper _data)
		{
			Name = _data.Name;
			Commander = TeamCache.Faction(_data.Faction);
			Id = _data.Id;

			CommandletsCache.Register(this);
		}

		protected bool TryGetQueue(ICommandable unit, out CommandQueue queue) 
			=> EntityCache.TryGet($"{unit.GameObject.name}:commandQueue", out queue);
    }

	public abstract class Commandlet<T> : Commandlet {

		public T Target => _target;
		public override Type TargetType => typeof(T);

		[FormerlySerializedAs("target")]
		[SerializeField]
		protected T _target;

		public virtual void Init (string name, T target, Faction commander) {
			InternalInit(name, commander);
			
			_target = target;

			Id = CommandletsCache.Register(this);

			if (Id <= -1)
			{
				Debug.LogError($"Unable to register command {Name} with cache! Deleting");
				Destroy(gameObject);
				return;
			}

			SpawnAndSync();
		}

		public abstract override Commandlet Clone();
	}
}