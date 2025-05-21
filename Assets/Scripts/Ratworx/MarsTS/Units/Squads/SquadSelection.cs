using Ratworx.MarsTS.Units.Infantry;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Squads
{
    public class SquadSelection : MonoBehaviour
    {
        private UnitSelection _unitSelection;
        private SquadManager _squadManager;
        private bool _selected;
        private bool _hovering;

        private void Awake() {
            _unitSelection = GetComponent<UnitSelection>();
            _squadManager = GetComponent<SquadManager>();
        }

        private void Start() {
            _squadManager.OnSquadMembershipChanged += OnSquadMembershipChanged;
            _unitSelection.OnUnitSelectionChange += OnSquadSelectionChanged;
            _unitSelection.OnUnitHoverChange += OnSquadHoverChanged;
        }

        private void OnSquadMembershipChanged(SquadMemberEntry member, bool isMember) {
            if (isMember) {
                member.Selection.Select(_selected);
                member.Selection.Hover(_hovering);
            }
            else {
                member.Selection.Select(false);
                member.Selection.Hover(false);
            }
        }

        private void OnSquadHoverChanged(bool status) {
            foreach (SquadMemberEntry member in _squadManager.MemberEntries) {
                member.Selection.Hover(status);
            }
        }

        private void OnSquadSelectionChanged(bool status) {
            foreach (SquadMemberEntry member in _squadManager.MemberEntries) {
                member.Selection.Select(status);
            }
        }
    }
}