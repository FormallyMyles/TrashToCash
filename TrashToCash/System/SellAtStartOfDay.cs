using Kitchen;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace TrashToCash.System
{
    [UpdateBefore(typeof(DestroyAppliancesAtDay))]
    public class SellAtStartOfDay : StartOfDaySystem, IModSystem
    {
        private EntityQuery _appliances;

        protected override void Initialise()
        {
            base.Initialise();

            // We only allow appliances which can be held to be sold
            _appliances = GetEntityQuery(new QueryHelper().All(typeof(CDestroyApplianceAtDay), typeof(CItemHolder)));
        }

        protected override void OnUpdate()
        {
            var totalSold = 0;
            var appliances = _appliances.ToEntityArray(Allocator.Temp);
            try
            {
                foreach (var appliance in appliances)
                {
                    // Get the value to sell for
                    var applianceData = EntityManager.GetComponentData<CAppliance>(appliance);
                    Debug.Log("Sold " + applianceData.ID + " for " + TrashToCash.GetSellPrice(applianceData.ID));

                    // Add to the total
                    totalSold += TrashToCash.GetSellPrice(applianceData.ID);
                    
                    // Destroy the entity
                    EntityManager.DestroyEntity(appliance);
                }
            }
            finally
            {
                appliances.Dispose();
            }

            // Don't add money if we didn't make any :(
            if (totalSold <= 0) return;

            // Congrats you made money!
            SetSingleton<SMoney>(GetSingleton<SMoney>() + totalSold);
            MoneyTracker.AddEvent(new EntityContext(this.EntityManager), TrashToCash.TrashID, totalSold);
        }
    }
}