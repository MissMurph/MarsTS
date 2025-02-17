using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Prefabs;
using MarsTS.Teams;
using System;
using System.Collections;
using System.Collections.Generic;
using MarsTS.Logging;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {

    public class ProduceCommandlet : Commandlet<GameObject>, IProducable {

		[field:SerializeField]
		public int ProductionRequired { get; private set; }
		public int ProductionProgress {
			get => _productionProgress.Value;
			set => _productionProgress.Value = value;
		}

		public override string SerializerKey => commandKey;

		[SerializeField]
		protected NetworkVariable<int> _productionProgress = new(writePerm: NetworkVariableWritePermission.Server);

		public event Action<int, int> OnWork;

		public Dictionary<string, int> Cost { get; private set; }
		public GameObject Product => Target;
		public override CommandFactory Command => CommandPrimer.Get(Name + "/" + Product.name);

		private string commandKey;

		public string ProductRegistryKey { get; private set; }

		public void InitProduce (string _name, string _commandKey, string productRegistryKey, GameObject _target, Faction _commander, int timeRequired, CostEntry[] cost) {
			ProductionRequired = timeRequired;
			ProductionProgress = 0;
			ProductRegistryKey = productRegistryKey;

			commandKey = _commandKey;

			Cost = new Dictionary<string, int>();

			foreach (CostEntry entry in cost) {
				Cost[entry.key] = entry.amount;
			}

			//Calling the rest of the Init will also spawn & sync the commandlet, make sure all data is created
			//BEFORE the sync
			Init(_commandKey, _target, _commander);
		}

		public override void OnNetworkSpawn () {
			base.OnNetworkSpawn();

			_productionProgress.OnValueChanged += OnProgressValueChanged;
		}

		private void OnProgressValueChanged (int previous, int current) {
			OnWork?.Invoke(previous, current);
		}

		public Commandlet Get () {
			return this;
		}

		public override void CompleteCommand (EventAgent eventAgent, ICommandable unit, bool isCancelled = false) {
			if (isCancelled) {
				foreach (KeyValuePair<string, int> entry in Cost) {
					Commander.GetResource(entry.Key).Deposit(entry.Value);
				}
			}

			base.CompleteCommand(eventAgent, unit, isCancelled);
		}

		public override Commandlet Clone()
		{
			throw new NotImplementedException();
		}

		protected override void Deserialize (SerializedCommandWrapper _data) {
			base.Deserialize(_data);
			
			var deserialized = (SerializedProduceCommandlet)_data.commandletData;

			ProductionRequired = deserialized.ProductionRequired;
			if (!Registry.TryGetPrefab(deserialized.PrefabKey, out GameObject prefab))
				RatLogger.Warning?.Log($"Couldn't find registry {deserialized.PrefabKey} for {GetType()}");
			_target = prefab;
		}
	}
}