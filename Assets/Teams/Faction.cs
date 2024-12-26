using System;
using System.Collections.Generic;
using System.Linq;
using MarsTS.Buildings;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Research;
using MarsTS.Units;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Teams
{
    public class Faction : NetworkBehaviour, IEquatable<Faction>
    {
        private Dictionary<string, Roster> _ownedUnits;

        public Team Allegiance => TeamCache.Team(this);

        public int VisionMask => Allegiance.VisionMask;

        public int Id => _id;

        [SerializeField] private int _id = -1;

        public bool IsHuman => true;

        public List<PlayerResource> Resources
        {
            get
            {
                var output = new List<PlayerResource>();

                foreach (PlayerResource registered in _resources.Values)
                {
                    output.Add(registered);
                }

                return output;
            }
        }

        private Dictionary<string, PlayerResource> _resources;

        private Dictionary<string, Technology> _research;

        private void Awake()
        {
            _ownedUnits = new Dictionary<string, Roster>();
            _resources = new Dictionary<string, PlayerResource>();
            _research = new Dictionary<string, Technology>();

            foreach (PlayerResource toRegister in GetComponents<PlayerResource>())
            {
                _resources[toRegister.Key] = toRegister;
            }

            foreach (Technology startingTech in GetComponentsInChildren<Technology>())
            {
                _research[startingTech.key] = startingTech;
            }
        }

        private void Start()
        {
            if (!NetworkManager.Singleton.IsServer) return;
            
            EventBus.AddListener<UnitOwnerChangeEvent>(OnUnitOwnershipChange);
        }

        public void SetId(int factionId)
        {
            _id = factionId;
            name = $"{typeof(Faction)}:{Id}";
            SynchronizeClientRpc(Id);
        }

        [Rpc(SendTo.NotServer)]
        private void SynchronizeClientRpc(int id)
        {
            _id = id;
            name = $"{typeof(Faction)}:{Id}";
            TeamCache.RegisterFaction(this);
        }

        public PlayerResource GetResource(string key)
        {
            if (_resources.TryGetValue(key, out PlayerResource bank)) return bank;

            Debug.LogWarning("Player Resource with key: " + key + " not found on Player " + name);
            return null;
        }

        public Relationship GetRelationship(Faction other)
        {
            if (other == this) return Relationship.Owned;
            if (TeamCache.Team(other).Id == 0) return Relationship.Neutral;
            if (TeamCache.Team(other).Id == Allegiance.Id) return Relationship.Friendly;
            return Relationship.Hostile;
        }
        
        private void OnUnitOwnershipChange (UnitOwnerChangeEvent _event)
        {
            string key = _event.Unit.RegistryKey;
            
            if (_event.Unit.Owner == this)
            {
                Roster roster = GetRoster(key);

                if (!roster.TryAdd(_event.Unit)) 
                    Debug.Log($"Couldn't add Unit {_event.Unit.GameObject.name} to {gameObject.name} Roster!");
            }
            else if (_ownedUnits.TryGetValue(key, out Roster roster) && roster.Contains(_event.Unit.Id))
            {
                roster.Remove(_event.Unit.Id);

                if (roster.Count == 0) _ownedUnits.Remove(key);
            }
        }

        public List<IDepositable> GetOwnedDepositables()
        {
            var output = new List<IDepositable>();
            
            foreach (Roster roster in _ownedUnits.Values)
            {
                if (!typeof(IDepositable).IsAssignableFrom(roster.Type)) continue;

                output.AddRange(roster.Cast<IDepositable>());
            }

            return output;
        }

        public bool IsResearched(string key) => _research.ContainsKey(key);

        public void SubmitResearch(Technology product)
        {
            _research[product.key] = product;
        }
        
        private Roster GetRoster (string key) {
            Roster map = _ownedUnits.GetValueOrDefault(key, new Roster());
            _ownedUnits.TryAdd(key, map);
            return map;
        }

        public bool Equals(Faction other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && _id == other._id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Faction)obj);
        }

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), _id);
    }
}