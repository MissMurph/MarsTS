using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Buildings
{
    public class BuildingSelectionGhost : MonoBehaviour
    {
        protected List<Collider> collisions = new List<Collider>();

        [SerializeField] protected Material legalMat;

        [SerializeField] protected Material illegalMat;

        private Renderer[] _allRenderers;

        public virtual bool Legal => collisions.Count == 0;

        private void Awake()
        {
            //_allRenderers = GetComponentsInChildren<Renderer>();
        }

        public void InitializeGhost(Building buildingBeingConstructed)
        {
            Instantiate(buildingBeingConstructed.transform.Find("Model"), transform);
            
            _allRenderers = GetComponentsInChildren<Renderer>();

            ChangeAllRenderers(legalMat);
        }

        private void Update()
        {
            ChangeAllRenderers(Legal ? legalMat : illegalMat);
        }

        private void ChangeAllRenderers(Material material)
        {
            foreach (Renderer render in _allRenderers)
            {
                render.material = material;
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!collisions.Contains(other)) collisions.Add(other);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (collisions.Contains(other)) collisions.Remove(other);
        }
    }
}