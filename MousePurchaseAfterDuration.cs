using Kitchen;
using KitchenData;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenDragNDropDesigner
{
    [UpdateBefore(typeof(MouseRotateAppliances))]
    internal class MousePurchaseAfterDuration : MouseApplianceInteractionSystem
    {
        public struct CPurchaseProgress : IComponentData
        {
            public float Progress;

            public float Total;
        }

        protected CAppliance ApplianceLetter;

        protected CTakesDuration Duration;

        protected CApplianceBlueprint Blueprint;

        protected CForSale Sale;

        protected SMoney Money;

        protected override CMouseData.Action Action => CMouseData.Action.Act;

        protected override bool AllowHold => true;

        EntityQuery RebuyChances;
        EntityQuery RefreshLetterChances;


        protected override void Initialise()
        {
            RebuyChances = GetEntityQuery(typeof(CBlueprintRebuyableChance));
            RefreshLetterChances = GetEntityQuery(typeof(CBlueprintRefreshChance));
            base.Initialise();
        }

        protected override bool IsPossibleCondition(ref InteractionData data)
        {
            if (Has<CBeingActedOn>(data.Target))
            {
                return false;
            }
            if (!Require<CApplianceBlueprint>(data.Target, out Blueprint))
            {
                return false;
            }
            if (!Require<CAppliance>(data.Target, out ApplianceLetter))
            {
                return false;
            }
            if (!Require<CForSale>(data.Target, out Sale))
            {
                return false;
            }
            if (!Require<CTakesDuration>(data.Target, out Duration))
            {
                return false;
            }
            if (!Require<CPosition>(data.Target, out Position))
            {
                return false;
            }
            Money = GetOrDefault<SMoney>();
            if (Sale.Price > Money)
            {
                return false;
            }
            return true;
        }

        protected override void Perform(ref InteractionData data)
        {
            if (!Require(data.Target, out CPurchaseProgress progress))
            {
                progress = new CPurchaseProgress
                {
                    Progress = 1f,
                    Total = 1f
                };
            }
            else
            {
                progress.Progress -= Time.DeltaTime;
            }
            Set(data.Target, progress);

            if (progress.Progress <= 0f)
            {
                Purchase(ref data);
                return;
            }
            else
            {
                Duration.Active = true;
                Duration.Remaining = progress.Progress;
                Set(data.Target, Duration);
                Set<CPerformedThisFrame>(data.Target);
            }
            data.Attempt.Result = InteractionResult.Performed;
        }

        private void Purchase(ref InteractionData data)
        {
            Money.Amount -= Sale.Price;
            SetSingleton(Money);


            NativeArray<CBlueprintRebuyableChance> nativeArray = RebuyChances.ToComponentDataArray<CBlueprintRebuyableChance>(Allocator.Temp);
            NativeArray<CBlueprintRefreshChance> nativeArray2 = RefreshLetterChances.ToComponentDataArray<CBlueprintRefreshChance>(Allocator.Temp);

            EntityManager.RemoveComponent<CPurchaseProgress>(data.Target);
            EntityManager.RemoveComponent<CPerformedThisFrame>(data.Target);
            EntityManager.RemoveComponent<CTakesDuration>(data.Target);
            EntityManager.RemoveComponent<CDisplayDuration>(data.Target);
            EntityManager.RemoveComponent<CHasIndicator>(data.Target);
            EntityManager.RemoveComponent<CPurchaseAfterDuration>(data.Target);
            EntityManager.RemoveComponent<CForSale>(data.Target);
            EntityManager.RemoveComponent<CShowApplianceInfo>(data.Target);
            EntityManager.RemoveComponent<CApplianceBlueprint>(data.Target);
            EntityManager.RemoveComponent<CShopEntity>(data.Target);
            EntityManager.AddComponent<CRemoveView>(data.Target);
            Set(data.Target, new CRequiresView
            {
                Type = ViewType.Appliance
            });
            Set(data.Target, new CCreateAppliance
            {
                ID = Blueprint.Appliance
            });


            float rebuyable_chance = 0f;
            foreach (CBlueprintRebuyableChance item in nativeArray)
            {
                rebuyable_chance += (1f - rebuyable_chance) * item.Chance;
            }

            float refresh_chance = 0f;
            foreach (CBlueprintRefreshChance item2 in nativeArray2)
            {
                refresh_chance += (1f - refresh_chance) * item2.Chance;
            }

            if (!Blueprint.IsCopy && Random.value < rebuyable_chance)
            {
                Entity e2 = EntityManager.CreateEntity();
                Set(e2, new CCreateAppliance
                {
                    ID = ApplianceLetter.ID
                });
                Set(e2, new CPosition(Position));
                Set(e2, new CApplianceBlueprint
                {
                    Appliance = Blueprint.Appliance,
                });
                if (Has<CShowApplianceInfo>(data.Target))
					{
                    Set(e2, new CShowApplianceInfo
                    {
                        Appliance = Blueprint.Appliance,
                        Price = Sale.Price,
                        ShowPrice = true
                    });
                }
                Set(e2, new CForSale
                {
                    Price = Sale.Price
                });
                Set(e2, default(CShopEntity));
            }

            if (!Blueprint.IsCopy && Random.value < refresh_chance)
            {
                Entity e3 = EntityManager.CreateEntity();
                Set(e3, new CNewShop
                {
                    Tags = (ShoppingTags.Technology | ShoppingTags.FrontOfHouse | ShoppingTags.Plumbing | ShoppingTags.Cooking | ShoppingTags.Automation | ShoppingTags.Misc | ShoppingTags.Office),
                    FixedLocation = true,
                    Location = Position.Position.Rounded()
                });
            }

            nativeArray.Dispose();
            nativeArray2.Dispose();
        }
    }
}
