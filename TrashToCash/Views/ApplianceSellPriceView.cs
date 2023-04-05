using Kitchen;
using KitchenMods;
using MessagePack;
using TMPro;
using TrashToCash.System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace TrashToCash.Views
{
    public class ApplianceSellPriceView : UpdatableObjectView<ApplianceSellPriceView.ViewData>
    {
        public class UpdateView : IncrementalViewSystemBase<ViewData>, IModSystem
        {
            private EntityQuery Views;

            protected override void Initialise()
            {
                base.Initialise();
                Views = GetEntityQuery(new QueryHelper().All(typeof(CLinkedView), typeof(CAppliance), typeof(CDestroyApplianceAtDay)).None(typeof(CHeldAppliance), typeof(CDoesNotOccupy), typeof(CApplyDecor)));
            }

            protected override void OnUpdate()
            {
                var views = Views.ToComponentDataArray<CLinkedView>(Allocator.Temp);
                var appliances = Views.ToComponentDataArray<CAppliance>(Allocator.Temp);
                var destroyStatuses = Views.ToComponentDataArray<CDestroyApplianceAtDay>(Allocator.Temp);
                
                for (var i = 0; i < views.Length; i++)
                {
                    var view = views[i];
                    var appliance = appliances[i];
                    var destroy = destroyStatuses[i];

                    // If the bin is shown, we probably don't want to show the price :)
                    if (!destroy.HideBin)
                    {
                        SendUpdate(view, new ViewData
                        {
                            SellPrice = PriceUpdater.GetSellPrice(EntityManager, appliance.ID)
                        }, MessageType.SpecificViewUpdate);
                    }
                }
            }
        }

        // you must mark your ViewData as MessagePackObject and mark each field with a key
        // if you don't, the game will run locally but fail in multiplayer
        [MessagePackObject]
        public struct ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)] public int SellPrice;

            // this tells the game how to find this subview within a prefab
            // GetSubView<T> is a cached method that looks for the requested T in the view and its children
            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<ApplianceSellPriceView>();

            // this is used to determine if the data needs to be sent again
            public bool IsChangedFrom(ViewData check) => SellPrice != check.SellPrice;
        }

        // this receives the updated data from the ECS backend whenever a new update is sent
        // in general, this should update the state of the view to match the values in view_data
        // ideally ignoring all current state; it's possible that not all updates will be received so
        // you should avoid relying on previous state where possible
        protected override void UpdateData(ViewData view_data)
        {
            // Remove existing game object
            var old = gameObject.transform.Find("SellPrice");
            if (old != null)
            {
                Debug.Log("TrashToCash destroying old price!");
                Destroy(old.gameObject);
            }

            // Apply sell price
            Debug.Log("TrashToCash applying price of " + view_data.SellPrice);
            if (view_data.SellPrice > 0)
            {
                // Apply our custom label
                var textHolder = new GameObject("SellPrice");
                textHolder.transform.SetParent(gameObject.transform);
                textHolder.transform.SetSiblingIndex(0);
                textHolder.transform.localPosition = new Vector3(0, 1.5f, -0.75f);
                textHolder.transform.localRotation = new Quaternion(1, 0, 0, 1);

                // Add our text
                var textMesh = textHolder.AddComponent<TextMeshPro>();
                textMesh.SetText("$" + view_data.SellPrice);
                textMesh.fontSize = 3;
                textMesh.alignment = TextAlignmentOptions.Center;
            }
        }
    }

}
