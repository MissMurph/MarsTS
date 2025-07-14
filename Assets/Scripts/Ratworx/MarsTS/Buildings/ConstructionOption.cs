using System;
using Ratworx.MarsTS.Production;

namespace Ratworx.MarsTS.Buildings
{
    [Serializable]
    public class ConstructionOption
    {
        public string OptionKey;
        public int ConstructionRequired;
        /// <summary>RegistryKey to the product prefab</summary>
        public string BuildingKey;
        public ResourceCost[] Cost;
        public string Description;
    }
}