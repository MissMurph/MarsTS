using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Buildings
{
    public class BuildingGhosts : MonoBehaviour
    {
        [SerializeField] private GameObject _selectionGhost;
        [SerializeField] private GameObject _constructionGhost;

        public GameObject SelectionGhost => _selectionGhost;
        public GameObject ConstructionGhost => _constructionGhost;
    }
}