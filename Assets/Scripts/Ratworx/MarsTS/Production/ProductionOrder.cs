using System;
using UnityEngine;

namespace Ratworx.MarsTS.Production
{
    [Serializable]
    public class ProductionOrder : MonoBehaviour
    {
        public int ProductionRequired;
        /// <summary>RegistryKey to the product prefab</summary>
        public string ProductKey;
        /// <summary>Pre-defined types are: <br/><c>production</c><br/><c>research</c><br/><c>upgrade</c></summary>
        public string ProductionType;
        public ResourceCost[] Cost;
    }
}