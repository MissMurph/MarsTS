using Ratworx.MarsTS.Extensions;
using Ratworx.MarsTS.Pathfinding;
using Ratworx.MarsTS.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ratworx.MarsTS.Buildings.BuildingPlacers
{
    public class PumpjackBuildingPlacer : BaseBuildingPlacer
    {
        private PumpjackSnapping _snapper = null;

        [SerializeField]
        private GameObject _snapPrefab;
        
        public override void StartPlacingBuilding(ConstructionOption option) {
            base.StartPlacingBuilding(option);
			
            _snapper = Instantiate(_snapPrefab).GetComponent<PumpjackSnapping>();
        }

        protected override void Update () {
            if (GhostTransform is null || _snapper is null) 
                return;
			
            Ray ray = Player.Player.ViewPort.ScreenPointToRay(Player.Player.MousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask))
                _snapper.gameObject.transform.position = hit.point;

            GhostTransform.position = _snapper.TrySnap(out Vector3 snapPos) ? snapPos : _snapper.transform.position;
        }
		
        protected override void OnSelect (InputAction.CallbackContext context) {
            if (!context.canceled) 
                return;
			
            if (!CurrentlyPlacingOption.CanFactionAfford(Player.Player.Commander) || !SelectionGhostComp.Legal) 
                return;
			
            base.OnSelect(context);
			
            Destroy(_snapper.gameObject);
            _snapper = null;
        }

        protected override void OnOrder (InputAction.CallbackContext context) {
            if (context.canceled) {
                CancelPlacingBuilding();
            }
        }

        public override void CancelPlacingBuilding () {
            if (GhostTransform is null)
                return;
            
            Destroy(GhostTransform.gameObject);
            Destroy(_snapper.gameObject);
            _snapper = null;
				
            Player.Player.Input.Release("Select");
            Player.Player.Input.Release("Order");
        }
    }
}