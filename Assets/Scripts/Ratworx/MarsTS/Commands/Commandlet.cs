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

		public string Name { get; protected set; }
		public Faction Commander { get; protected set; }
		public UnityEvent<CommandCompleteEvent> OnCommandComplete = new UnityEvent<CommandCompleteEvent>();
		public virtual CommandFactory Command => CommandPrimer.Get(Name);
		public abstract string SerializerKey { get; }
		public List<string> commandedUnits = new List<string>();
		public int Id { get; protected set; } = 0;

		protected void InternalInit(string name, Faction commander)
		{
			Name = name;
			Commander = commander;
		}

		public virtual void StartCommand (ICommandable unit) {
			commandedUnits.Add(unit.GameObject.name);
		}

		public virtual void CompleteCommand (ICommandable unit, bool isCancelled = false) 
		{
			commandedUnits.Remove(unit.GameObject.name);
			OnCommandComplete.Invoke(new CommandCompleteEvent(this, isCancelled, unit));
		}
		
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

		protected virtual void Deserialize(SerializedCommandWrapper data)
		{
			Name = data.Name;
			Commander = TeamCache.Faction(data.Faction);
			Id = data.Id;

			CommandletsCache.Register(this);
		}

		protected bool TryGetQueue(ICommandable unit, out CommandQueue queue) 
			=> EntityCache.TryGetEntityComponent($"{unit.GameObject.name}:commandQueue", out queue);
    }

	public abstract class Commandlet<T> : Commandlet {

		public T Target => _target;

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
	}
}