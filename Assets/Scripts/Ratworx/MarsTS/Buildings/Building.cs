using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Events.Selectable.Internal;
using Ratworx.MarsTS.Research;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.UI.Unit_Pane;
using Ratworx.MarsTS.Units;
using Ratworx.MarsTS.Vision;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Buildings
{
    public abstract class Building : NetworkBehaviour
    {
        [Header("Construction")]
        [SerializeField] private GameObject selectionGhost;
        [SerializeField] private GameObject constructionGhost;

        public GameObject SelectionGhost => selectionGhost;
        public GameObject ConstructionGhost => constructionGhost;
    }
}