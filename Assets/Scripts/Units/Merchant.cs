using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

public class Merchant : RtsUnit, IHasInventory
{
    public const int InventorySize = 100;
    public const float LoadingDistance = 12f;

    private enum States { Idle, TravelingToSource, TravelingToTarget }

    private States merchantState = States.Idle;
    private readonly Inventory inventory = new Inventory(InventorySize);
    private ResourceTypes resource;
    private RtsEntity source = null;
    private RtsEntity target = null;

    public Inventory Inventory
    {
        get { return inventory; }
    }

    public override IAbility RightClickAbility
    {
        get { return null; }
    }

    public Merchant()
    {
        var keys = new[] { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.F, KeyCode.Y, KeyCode.X, KeyCode.C, KeyCode.V };
        foreach (var ability in Abilities.ToList()) { RemoveAbility(ability); } //No custom movement abilities at the moment. Nice-to-have for later
        AddAbility(new StopAbility(this));
        AddAbility(new ResumeAbility(this));
        var key = 0;
        foreach (var resource in RtsResource.Resources)
        {
            AddAbility(new MerchantRoute(resource, keys[key++], "Move"+resource.ToString(), this));
        }
    }

    private EntityControl EntityControl { get { return FindObjectOfType<EntityControl>(); } }

    protected override void Update()
    {
        base.Update();
        if (!hasAuthority) { return; }
        switch (merchantState)
        {
            case States.Idle:
                break;
            case States.TravelingToSource:
                if(source != null && (source.transform.position - transform.position).sqrMagnitude < LoadingDistance * LoadingDistance)
                {
                    //Load merchant
                    var sourceInventory = (source as IHasInventory).Inventory;
                    var amount = Math.Min(Inventory.SpaceAvailable - Inventory.Count(), sourceInventory[resource]);
                    if(amount > 0 && sourceInventory.RemoveResources(resource, amount))
                    {
                        Inventory.AddResources(resource, amount);
                    }
                    GetComponent<NavMeshAgent>().SetDestination(target.transform.position);
                    merchantState = States.TravelingToTarget;
                }
                break;
            case States.TravelingToTarget:
                if (target != null && (target.transform.position - transform.position).sqrMagnitude < LoadingDistance * LoadingDistance)
                {
                    //Unload merchant
                    var targetInventory = (target as IHasInventory).Inventory;
                    foreach (var stored in Inventory.ToList())
                    {
                        var amount = Math.Min(stored.Value, targetInventory.SpaceAvailable - targetInventory.Count());
                        if (amount > 0 && targetInventory.AddResources(stored.Key, amount))
                        {
                            Inventory.RemoveResources(stored.Key, amount);
                        }
                    }
                    GetComponent<NavMeshAgent>().SetDestination(source.transform.position);
                    merchantState = States.TravelingToSource;
                }
                break;
        }
    }

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
            if (!merchant.hasAuthority) { return; }
            merchant.EntityControl.StartTargeting(SetSource, "Select route source");
            merchant.merchantState = States.Idle;
        }

        private bool SetSource(IEnumerable<RaycastHit> hits)
        {
            var entity = Utility.GetRtsEntityFromHits(hits);
            if (!(entity is Saloon || entity is StorageHouse))
            {
                merchant.EntityControl.ShowHintText("Source has to be a storage building...\nSelect route target");
                return false;
            }
            if (!entity.hasAuthority)
            {
                merchant.EntityControl.ShowHintText("You don't own this source... Select route source");
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
                merchant.EntityControl.ShowHintText("Target has to be a storage building...\nSelect route target");
                return false;
            }
            if (!entity.hasAuthority)
            {
                merchant.EntityControl.ShowHintText("You don't own this target... Select route target");
                return false;
            }
            if (entity == source)
            {
                merchant.EntityControl.ShowHintText("Source and target can't be the same...\nSelect route target");
                return false;
            }
            foreach (var merchant in this.merchant.EntityControl.SelectedEntities.Get<Merchant>().Where(m => m.hasAuthority))
            {
                ApplyForMerchant(merchant, entity);
            }
            return true;
        }

        private void ApplyForMerchant(Merchant merchant, RtsEntity target)
        {
            merchant.resource = resource;
            merchant.source = source;
            merchant.target = target;
            merchant.merchantState = States.TravelingToSource;
            var agent = merchant.GetComponent<NavMeshAgent>();
            agent.SetDestination(merchant.source.transform.position);
            agent.Resume();
        }
    }
}
