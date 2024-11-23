using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Buildings
{
    public class BuildingSelectionGhost : MonoBehaviour
    {
        protected List<Collider> Collisions = new List<Collider>();

        [SerializeField] protected Material legalMat;

        [SerializeField] protected Material illegalMat;

        protected Renderer[] AllRenderers;

        public virtual bool Legal => Collisions.Count == 0;

        public virtual void InitializeGhost(Building buildingBeingConstructed)
        {
            Instantiate(buildingBeingConstructed.transform.Find("Model"), transform);
            
            AllRenderers = GetComponentsInChildren<Renderer>();

            ChangeAllRenderers(legalMat);
        }

        private void Update()
        {
            ChangeAllRenderers(Legal ? legalMat : illegalMat);
        }

        protected void ChangeAllRenderers(Material material)
        {
            foreach (Renderer render in AllRenderers)
            {
                render.material = material;
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!Collisions.Contains(other)) Collisions.Add(other);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (Collisions.Contains(other)) Collisions.Remove(other);
        }
    }
}