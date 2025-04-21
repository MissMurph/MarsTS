using System.Collections.Generic;
using Ratworx.MarsTS.Buildings;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Commands.Factories;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Events.Harvesting;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.UI.Unit_Pane;
using Ratworx.MarsTS.Units.Sensors;
using Ratworx.MarsTS.Units.Turrets;
using Ratworx.MarsTS.WorldObject;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Vehicles
{
    public class Harvester : AbstractUnit
    {
        protected int Stored => _storageComp.Value;

        protected int Capacity => _storageComp.Capacity;

        protected ResourceStorage _storageComp;

        protected DepositSensor _depositableDetector;

        //This is how many units per second
        [SerializeField] protected float _depositRate;

        protected int _depositAmount;
        protected float _cooldown;
        protected float _currentCooldown;
        
        protected override void Awake()
        {
            base.Awake();

            _storageComp = GetComponent<ResourceStorage>();
            _depositableDetector = GetComponentInChildren<DepositSensor>();

            _cooldown = 1f / _depositRate;
            _depositAmount = Mathf.RoundToInt(_depositRate * _cooldown);
            _currentCooldown = _cooldown;

            foreach (HarvesterTurret turret in GetComponentsInChildren<HarvesterTurret>())
            {
                registeredTurrets.TryAdd(turret.name, turret);
            }
        }

        protected virtual void DepositResources()
        {
            _storageComp.Value -= DepositTarget.Deposit("resource_unit", _depositAmount);
            Bus.PostGlobal(new HarvesterDepositEvent(Bus, this, HarvesterDepositEvent.Side.Harvester, Stored, Capacity,
                DepositTarget));
            _currentCooldown += _cooldown;
        }

        protected override void OnUnitInfoDisplayed(UnitInfoEvent _event)
        {
            base.OnUnitInfoDisplayed(_event);

            if (ReferenceEquals(_event.Unit, this))
            {
                UnitResourceStorageInfo info = _event.Info.Module<UnitResourceStorageInfo>("storage");
                info.SetStorage(_storageComp);
            }
        }
    }
}