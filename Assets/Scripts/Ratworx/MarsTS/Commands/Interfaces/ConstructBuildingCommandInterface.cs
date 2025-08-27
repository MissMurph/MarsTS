using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Buildings;
using Ratworx.MarsTS.Buildings.BuildingPlacers;
using Ratworx.MarsTS.Buildings.Ghosts;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Init;
using Ratworx.MarsTS.Extensions;
using Ratworx.MarsTS.Logging;
using Ratworx.MarsTS.Pathfinding;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.UI;
using Ratworx.MarsTS.Units;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ratworx.MarsTS.Commands.Interfaces
{
    public class ConstructBuildingCommandInterface : NetworkBehaviour,
                                                     ICommandInterface,
                                                     ICommandInterfaceArgumentAccepter<ConstructionOption>
    {
        public string CommandKey => "construct";
        public string Description => "";
        public CursorSprite Cursor => null;

        public Sprite GetIcon() => throw new System.NotImplementedException();

        private Dictionary<string, BaseBuildingPlacer> _registeredBuildingPlacers =
            new Dictionary<string, BaseBuildingPlacer>();

        private BaseBuildingPlacer _currentPlacer = null;

        private void Awake() {
            BaseBuildingPlacer[] placers = GetComponentsInChildren<BaseBuildingPlacer>();

            foreach (BaseBuildingPlacer placer in placers) {
                foreach (string targetBuilding in placer.TargetBuildings) {
                    if (!_registeredBuildingPlacers.TryAdd(targetBuilding, placer)) {
                        RatLogger.Warning?.Log($"{nameof(placer.GetType)} could not be registered as building " +
                                               $"{targetBuilding} is already registered!");
                    }
                }
            }
        }

        public void StartSelection()
            => throw new NotSupportedException(
                $"{nameof(ConstructBuildingCommandInterface)} only supports {nameof(StartArgSelection)}");

        public virtual void StartArgSelection(ConstructionOption arg) {
            if (string.IsNullOrEmpty(arg.BuildingKey)) {
                RatLogger.Error?.Log($"Registry key for construction option {arg.OptionKey} is empty!");
                return;
            }

            if (!_registeredBuildingPlacers.TryGetValue(arg.BuildingKey, out BaseBuildingPlacer placer)) 
                placer = _registeredBuildingPlacers["default"];
            
            if (!arg.CanFactionAfford(Player.Player.Commander)) return;
            
            placer.StartPlacingBuilding(arg);
        }

        public virtual void CancelSelection() {
            if (_currentPlacer is not null) 
                _currentPlacer.CancelPlacingBuilding();
        }

        public string GetArgDescription(ConstructionOption arg) => arg.Description;

        public Sprite GetArgIcon(ConstructionOption arg) {
            if (!Registry.Registry.TryGetPrefab(arg.BuildingKey, out GameObject prefab))
                RatLogger.Error?.Log($"Couldn't find registered prefab {arg.BuildingKey} for icon!");

            // TODO: Create an image cache so we don't have to keep getting components
            return prefab.GetComponent<ISelectable>().Icon;
        }
    }
}