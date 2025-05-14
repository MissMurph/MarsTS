using System;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units.Infantry;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Squads
{
    public class SquadOwnership : MonoBehaviour
    {
        private UnitOwnership _unitOwnership;
        private SquadManager _squadManager;

        private void Awake() {
            _unitOwnership = GetComponent<UnitOwnership>();
            _squadManager = GetComponent<SquadManager>();
        }

        private void Start() {
            _unitOwnership.OnUnitOwnershipChanged += OnSquadOwnershipChanged;
            _squadManager.OnSquadMembershipChanged += OnSquadMembershipChanged;
        }

        private void OnSquadMembershipChanged(SquadMemberEntry member, bool isMember) {
            if (isMember) 
                member.Ownership.SetOwner(_unitOwnership.Owner);
        }

        private void OnSquadOwnershipChanged(Faction owner) {
            foreach (SquadMemberEntry member in _squadManager.MemberEntries) {
                member.Ownership.SetOwner(owner);
            }
        }
    }
}