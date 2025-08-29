using System;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.WorldObject
{
    public class ResourceDepositOwnership : UnitOwnership
    {
        private void Start() {
            GameInit.OnSpawnEntities += OnEntitiesSpawn;
        }

        public override void OnDestroy() {
            GameInit.OnSpawnEntities -= OnEntitiesSpawn;
        }

        private void OnEntitiesSpawn() => SetOwner(TeamCache.None);
    }
}