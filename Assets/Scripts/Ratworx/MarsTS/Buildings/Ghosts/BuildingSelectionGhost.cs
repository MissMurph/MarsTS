using System.Collections.Generic;
using UnityEngine;

namespace Ratworx.MarsTS.Buildings.Ghosts
{
    public class BuildingSelectionGhost : MonoBehaviour
    {
        protected List<Collider> Collisions = new List<Collider>();

        [SerializeField] protected Material legalMat;

        [SerializeField] protected Material illegalMat;

        protected Renderer[] AllRenderers;

        [SerializeField] protected LayerMask selectionMask;

        public virtual bool Legal => Collisions.Count == 0;

        public virtual void InitializeGhost(Building buildingBeingConstructed)
        {
            Instantiate(buildingBeingConstructed.transform.Find("Model"), transform);
            GameObject legalityCollider = Instantiate(buildingBeingConstructed.transform.Find("Collider"), transform).gameObject;

            legalityCollider.layer = LayerMask.NameToLayer("BuildingGhost");
            legalityCollider.GetComponent<Collider>().isTrigger = true;
            
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