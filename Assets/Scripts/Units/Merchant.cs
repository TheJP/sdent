using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

public class Merchant : RtsUnit, IHasInventory
{
    public const int InventorySize = 10;

    private enum States { Idle, TravelingToSource, TravelingToTarget }

    private States merchantState = States.Idle;
    private readonly Inventory inventory = new Inventory(InventorySize);
    private ResourceTypes resource;
    private RtsEntity source;
    private RtsEntity target;

    public Inventory Inventory
    {
        get { return inventory; }
    }

    public Merchant()
    {
        var keys = new[] { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.F, KeyCode.Y, KeyCode.X, KeyCode.C, KeyCode.V };
        foreach (var ability in Abilities.ToList()) { RemoveAbility(ability); } //No custom movement abilities at the moment. Nice-to-have for later
        var key = 0;
        foreach (var resource in RtsResource.Resources)
        {
            AddAbility(new MerchantRoute(resource, keys[key++], "BuildBuilding", this));
        }
    }

    private EntityControl EntityControl { get { return FindObjectOfType<EntityControl>(); } }

    private class MerchantRoute : AbilityBase
    {
        private readonly ResourceTypes resource;
        private readonly Merchant merchant;
        private RtsEntity source;

        public MerchantRoute(ResourceTypes resource, KeyCode code, string iconName, Merchant merchant)
            : base(string.Format("Transport {0}", resource), string.Format("Transport {0} from the selected source to the selected target", resource), code, iconName, isSettingTarget: true)
        {
            this.resource = resource;
            this.merchant = merchant;
        }

        public override void Execute()
        {
            merchant.EntityControl.StartTargeting(SetSource, "Select route source");
            merchant.merchantState = States.Idle;
        }

        private bool SetSource(IEnumerable<RaycastHit> hits)
        {
            var entity = Utility.GetRtsEntityFromHits(hits);
            if (!(entity is Saloon || entity is StorageHouse))
            {
                merchant.EntityControl.ShowHintText("Source has to be a storage building... Select route target");
                return false;
            }
            source = entity;
            merchant.EntityControl.StartTargeting(SetTarget, "Select route target");
            return false; //Not finished with this ability (but swapped targeting lambda)
        }

        private bool SetTarget(IEnumerable<RaycastHit> hits)
        {
            var entity = Utility.GetRtsEntityFromHits(hits);
            if (!(entity is Saloon || entity is StorageHouse))
            {
                merchant.EntityControl.ShowHintText("Target has to be a storage building... Select route target");
                return false;
            }
            if (entity == source)
            {
                merchant.EntityControl.ShowHintText("Source and target can't be the same... Select route target");
                return false;
            }
            merchant.resource = resource;
            merchant.source = source;
            merchant.target = entity;
            return true;
        }
    }
}
