using MarsTS.Events;
using MarsTS.Players;
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

		public void Init (string _name, GameObject _target, Faction _commander, int timeRequired, CostEntry[] cost) {
			Init(_name, _target, _commander);

			ProductionRequired = timeRequired;
			ProductionProgress = 0;

			Cost = new Dictionary<string, int>();

			foreach (CostEntry entry in cost) {
				Cost[entry.key] = entry.amount;
			}
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

		protected override ISerializedCommand Serialize () {
			return new SerializedProduceCommandlet() {
				_productionRequired = ProductionRequired
			};
		}

		protected override void Deserialize (SerializedWrapper _data) {
			SerializedProduceCommandlet deserialized = (SerializedProduceCommandlet)_data.commandletData;

			ProductionRequired = deserialized._productionRequired;
		}
	}

	public struct SerializedProduceCommandlet : ISerializedCommand {

		internal int _productionRequired;

		public void NetworkSerialize<T> (BufferSerializer<T> serializer) where T : IReaderWriter {
			serializer.SerializeValue(ref _productionRequired);
		}
	}
}