using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Prefabs;
using MarsTS.Teams;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {

    public class ProduceCommandlet : Commandlet<GameObject>, IProducable {

		[field:SerializeField]
		public int ProductionRequired { get; private set; }
		public int ProductionProgress {
			get => _productionProgress.Value;
			set {
				int oldProgress = _productionProgress.Value;
				_productionProgress.Value = value;
				//OnWork.Invoke(oldProgress, _productionProgress.Value);
			}
		}

		[SerializeField]
		protected NetworkVariable<int> _productionProgress = new(writePerm: NetworkVariableWritePermission.Server);

		public event Action<int, int> OnWork;

		public Dictionary<string, int> Cost { get; private set; }
		public GameObject Product { get { return Target; } }
		public override CommandFactory Command { get { return CommandRegistry.Get(Name + "/" + Product.name); } }

		public override string Key { get { return Name; } }

		private string commandKey;

		public void Init (string _name, string _commandKey, GameObject _target, Faction _commander, int timeRequired, CostEntry[] cost) {
			ProductionRequired = timeRequired;
			ProductionProgress = 0;

			commandKey = _commandKey;

			Cost = new Dictionary<string, int>();

			foreach (CostEntry entry in cost) {
				Cost[entry.key] = entry.amount;
			}

			//Calling the rest of the Init will also spawn & sync the commandlet, make sure all data is created
			//BEFORE the sync
			Init(_name, _target, _commander);
		}

		public override void OnNetworkSpawn () {
			base.OnNetworkSpawn();

			_productionProgress.OnValueChanged += OnProgressValueChanged;
		}

		private void OnProgressValueChanged (int previous, int current) {
			OnWork.Invoke(previous, current);
		}

		public Commandlet Get () {
			return this;
		}

		public override void OnComplete (CommandQueue queue, CommandCompleteEvent _event) {
			if (_event.CommandCancelled) {
				foreach (KeyValuePair<string, int> entry in Cost) {
					Player.Commander.Resource(entry.Key).Deposit(entry.Value);
				}
			}

			base.OnComplete(queue, _event);
		}

		/*protected override ISerializedCommand Serialize () {

			string prefabKey = "unit:" + Product.name;

			return new SerializedProduceCommandlet() {
				_productionRequired = ProductionRequired,
				_prefabKey = prefabKey
			};
		}*/

		protected override ISerializedCommand Serialize () {
			return Serializers.Write(this);
		}

		protected override void Deserialize (SerializedCommandWrapper _data) {
			SerializedProduceCommandlet deserialized = (SerializedProduceCommandlet)_data.commandletData;

			Name = _data.Key;
			Commander = TeamCache.Faction(_data.Faction);
			ProductionRequired = deserialized._productionRequired;
			target = Registry.Prefab(deserialized._prefabKey);
		}
	}
}