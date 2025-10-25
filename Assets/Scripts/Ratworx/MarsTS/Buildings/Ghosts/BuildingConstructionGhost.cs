using System;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Commands.Commandlets;
using Ratworx.MarsTS.Commands.Interfaces;
using Ratworx.MarsTS.Commands.Receivers;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Init;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using Ratworx.MarsTS.Vision;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Buildings.Ghosts
{
    public class BuildingConstructionGhost : NetworkBehaviour, ICommandReceiver<BooleanCommandlet>
    {
        private ResourceCost[] _constructionCost;

        private GameObject _buildingBeingConstructed;

        private Entity _entity;
        private EventAgent _bus;
        private Transform _model;

        private GameObject[] _visionObjects = Array.Empty<GameObject>();
        private HealthAttribute _healthAttribute;
        private ConstructionProgressAttribute _constructionAttribute;
        private ConstructionGhostSelection _ghostSelection;
        private UnitOwnership _ownership;
        private UnitVision _ghostVision;

        private void Awake() {
            _bus = GetComponent<EventAgent>();
            _entity = GetComponent<Entity>();
            _healthAttribute = GetComponent<HealthAttribute>();
            _constructionAttribute = GetComponent<ConstructionProgressAttribute>();
            _ownership = GetComponent<UnitOwnership>();
            _ghostSelection = GetComponent<ConstructionGhostSelection>();
            _ghostVision = GetComponent<UnitVision>();
        }

        public virtual void InitializeGhost(string buildingKey, params ResourceCost[] constructionCost) {
            if (!NetworkManager.Singleton.IsServer) return;

            UpdateProperties(buildingKey, constructionCost);
            InstantiateChildObjects();

            InitializeGhostClientRpc(buildingKey);

            _bus.PostLocal(new UnitInitEvent(_entity));
        }

        [Rpc(SendTo.NotServer)]
        private void InitializeGhostClientRpc(string buildingKey) {
            UpdatePropertiesClient(buildingKey);
            InstantiateChildObjects();
        }

        protected void UpdateProperties(string registryKey, params ResourceCost[] constructionCost) {
            Registry.Registry.TryGetPrefab($"{registryKey}", out GameObject buildingBeingConstructed);

            var targetHealth = buildingBeingConstructed.GetComponent<HealthAttribute>();
            
            _healthAttribute.SetMaxHealth(targetHealth.MaxHealth);

            _buildingBeingConstructed = buildingBeingConstructed;
            _constructionCost = constructionCost;
            
            _ghostSelection.SetBuildingOverride(registryKey);
            
            _constructionAttribute.OnAttributeChange += OnConstructionProgressChanged;
        }

        private void UpdatePropertiesClient(string buildingKey) {
            Registry.Registry.TryGetPrefab($"{buildingKey}", out GameObject buildingBeingConstructed);

            _buildingBeingConstructed = buildingBeingConstructed;
            
            _ghostSelection.SetBuildingOverride(buildingKey);
            _constructionAttribute.OnAttributeChange += OnConstructionProgressChanged;
        }

        private void InstantiateChildObjects() {
            _model = Instantiate(_buildingBeingConstructed.transform.Find("Model"), transform);

            var selectionCircle = Instantiate(_buildingBeingConstructed.transform.Find("SelectionCircle"), transform);
            var mapSquare = Instantiate(_buildingBeingConstructed.transform.Find("MapSquare"), transform);
            var barOrientation = transform.Find("UnitBarsOrientation");
            Instantiate(_buildingBeingConstructed.transform.Find("Collider"), transform);
            var selectionCollider =
                Instantiate(_buildingBeingConstructed.transform.Find("SelectionCollider"), transform);

            if (_constructionAttribute.Value > 0) {
                float constructedProportion = (float)_constructionAttribute.Value / _healthAttribute.MaxHealth;
                _model.localScale = Vector3.one * constructedProportion;
            }
            else {
                _model.localScale = Vector3.one * 0.01f;
            }
            
            _visionObjects = new[]
            {
                _model.gameObject,
                selectionCircle.gameObject,
                mapSquare.gameObject,
                barOrientation.gameObject,
                //hitCollider.gameObject,
                selectionCollider.gameObject
            };

            foreach (GameObject visionObject in _visionObjects) {
                visionObject.SetActive(false);
                _ghostVision.SetObjectHideable(visionObject);
            }
        }

        private void OnConstructionProgressChanged(int oldValue, int newValue) {
            float constructedProportion = (float)_constructionAttribute.Value / _healthAttribute.MaxHealth;
            _model.localScale = Vector3.one * constructedProportion;

            if (!NetworkManager.Singleton.IsServer) return;

            if (_constructionAttribute.Value >= _healthAttribute.MaxHealth) CompleteConstruction();
        }

        public void ReceiveCommand(BooleanCommandlet command) {
            CancelConstruction();
        }

        private void CancelConstruction() {
            _bus.PostGlobal(new UnitDeathEvent(_entity));

            foreach (ResourceCost materialCost in _constructionCost) {
                _ownership.Owner.GetResource(materialCost.key).Deposit(materialCost.amount);
            }

            Destroy(gameObject, 0.1f);
        }

        private void CompleteConstruction() {
            SendCompletionClientEventRpc();

            GameObject newBuilding = Instantiate(_buildingBeingConstructed, transform.position, transform.rotation);
            NetworkObject buildingNetworking = newBuilding.GetComponent<NetworkObject>();
            UnitOwnership buildingOwnership = newBuilding.GetComponent<UnitOwnership>();

            buildingNetworking.Spawn();
            buildingOwnership.SetOwner(_ownership.Owner);

            _bus.PostGlobal(new UnitDeathEvent(_entity));
            Destroy(gameObject, 0.1f);
        }

        [Rpc(SendTo.NotServer)]
        private void SendCompletionClientEventRpc() {
            _bus.PostGlobal(new UnitDeathEvent(_entity));
        }

        public event Action OnCommandStateUpdated;
        public string CommandKey => "cancelConstruction";
        public bool CanCommand => true;
        public int EvaluationPriority => 0;
        public bool IsActive => false;
        public bool CanInterrupt => false;
        public float Cooldown => 0f;
        public bool InterruptQueue => true;

        public void ReceiveCommand(Commandlet command) {
            throw new NotImplementedException();
        }

        public (bool valid, ICommandInterface command) EvaluateCommand(Entity entity) => (false, null);
        public void StartSelection(string argument = null) {
            throw new NotImplementedException();
        }

        public Sprite GetIcon(string argument = null) => throw new NotImplementedException();

        public string GetDescription(string argument = null) => throw new NotImplementedException();
        public string GetName(string argument = null) => throw new NotImplementedException();
    }
}