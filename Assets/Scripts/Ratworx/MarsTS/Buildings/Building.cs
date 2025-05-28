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

        /*protected virtual void Upgrade(Commandlet order)
        {
            Bus.AddListener<CommandCompleteEvent>(UpgradeComplete);
        }

        protected virtual void UpgradeComplete(CommandCompleteEvent _event)
        {
            Bus.RemoveListener<CommandCompleteEvent>(UpgradeComplete);

            IProducable order = _event.Command as IProducable;
            GameObject product = Instantiate(order.Product, transform, false);

            for (int i = 0; i < boundCommands.Length; i++)
                if (boundCommands[i] == _event.Command.Command.Name)
                {
                    boundCommands[i] = "";
                    Bus.PostGlobal(new CommandsUpdatedEvent(Bus, this, Commands()));
                    break;
                }

            Bus.PostGlobal(new ProductionCompleteEvent(Bus, product, this, production, order));
        }*/

        /*protected virtual void Research(Commandlet order)
        {
            Bus.AddListener<CommandCompleteEvent>(ResearchComplete);
        }

        protected virtual void ResearchComplete(CommandCompleteEvent _event)
        {
            Bus.RemoveListener<CommandCompleteEvent>(ResearchComplete);

            IProducable order = _event.Command as IProducable;
            Technology product = Instantiate(order.Product).GetComponent<Technology>();
            
            Owner.SubmitResearch(product);

            for (int i = 0; i < boundCommands.Length; i++)
                if (boundCommands[i] == _event.Command.Command.Name)
                {
                    boundCommands[i] = "";
                    Bus.PostGlobal(new CommandsUpdatedEvent(Bus, this, Commands()));
                    break;
                }

            Bus.PostGlobal(new ResearchCompleteEvent(Bus, product, this, production, order));
            Bus.PostGlobal(new ProductionCompleteEvent(Bus, product.gameObject, this, production, order));
        }*/

        // Move this to adding commands on the Queue
        /*protected virtual void OnGlobalResearchComplete(ResearchCompleteEvent _event)
        {
            for (int i = 0; i < boundCommands.Length; i++)
                if (_event.CurrentProduction.Get().Command.Name == boundCommands[i])
                    boundCommands[i] = "";
        }*/

        /*protected virtual void OnUnitInfoDisplayed(UnitInfoEvent @event)
        {
            if (!ReferenceEquals(@event.Unit, this)) return;
            
            HealthInfo info = @event.Info.Module<HealthInfo>("health");
            info.CurrentUnit = this;

            @event.Info.Module<ProductionInfo>("productionQueue").SetQueue(this, production.Current as IProducable,
                production.QueuedProduction);
        }*/
    }
}