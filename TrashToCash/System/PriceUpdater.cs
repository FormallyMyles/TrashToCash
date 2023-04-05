using System;
using System.Linq;
using Kitchen;
using KitchenData;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;

namespace TrashToCash.System
{
    public class PriceUpdater : NightSystem, IModSystem
    {
        private EntityQuery _appliances;

        protected override void Initialise()
        {
            base.Initialise();

            // We only allow appliances which can be held to be sold
            _appliances = GetEntityQuery(new QueryHelper().All(typeof(CApplianceBlueprint), typeof(CForSale), typeof(CLinkedView)));
        }

        protected override void OnUpdate()
        {
            var appliances = _appliances.ToEntityArray(Allocator.Temp);
            try
            {
                foreach (var appliance in appliances)
                {
                    // Get the value to sell for
                    var applianceData = EntityManager.GetComponentData<CApplianceBlueprint>(appliance);
                    var forSale = EntityManager.GetComponentData<CForSale>(appliance);
                    TrackPrice(EntityManager, applianceData.Appliance, forSale.Price);
                }
            }
            finally
            {
                appliances.Dispose();
            }
        }

        public static int GetSellPrice(EntityManager entityManager, int applianceID)
        {
            return (int) Math.Floor(GetKnownPrice(entityManager, applianceID) / 2D);
        }

        public static int GetKnownPrice(EntityManager entityManager, int applianceID)
        {
            var buyPrice = GameData.Main.Get<Appliance>(applianceID).PurchaseCost;
            
            // We look for CApplianceInfo with CImmovable
            // These are our price trackers
            // Why? Because this mod should easily be removable, if we use entities without views, they will never get one, easy!
            var query = entityManager.CreateEntityQuery(new QueryHelper().All(typeof(CApplianceInfo), typeof(CImmovable)).None(typeof(CLinkedView), typeof(CRequiresView)).Build());
            var appliances = query.ToComponentDataArray<CApplianceInfo>(Allocator.Temp);
            try
            {
                foreach (var appliance in appliances.Where(appliance => appliance.ID == applianceID))
                {
                    buyPrice = Math.Min(buyPrice, appliance.Price);
                }
            }
            finally
            {
                appliances.Dispose();
            }

            return buyPrice;
        }

        public static void TrackPrice(EntityManager entityManager, int applianceID, int amount)
        {
            // Find if we already have the price tracked :)
            var query = entityManager.CreateEntityQuery(new QueryHelper().All(typeof(CApplianceInfo), typeof(CImmovable)).None(typeof(CLinkedView), typeof(CRequiresView)).Build());
            var entities = query.ToEntityArray(Allocator.Temp);
            try
            {
                foreach (var entity in entities)
                {
                    var appliance = entityManager.GetComponentData<CApplianceInfo>(entity);
                    if (appliance.ID != applianceID) continue;
                    if (amount < appliance.Price)
                    {
                        entityManager.SetComponentData(entity, new CApplianceInfo()
                        {
                            ID = applianceID,
                            Price = amount
                        });
                        return;
                    }
                    else
                    {
                        return; // No updated needed
                    }
                }
            }
            finally
            {
                entities.Dispose();
            }

            // Otherwise add
            var newEntity = entityManager.CreateEntity(typeof(CApplianceInfo), typeof(CImmovable));
            entityManager.AddComponentData(newEntity, new CApplianceInfo()
            {
                ID = applianceID,
                Price = amount
            });
        }
    }
}