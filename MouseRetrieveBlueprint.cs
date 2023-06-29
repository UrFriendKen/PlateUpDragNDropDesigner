using Kitchen;
using KitchenDragNDropDesigner.Helpers;
using KitchenDragNDropDesigner.Patches;
using Unity.Entities;

namespace KitchenDragNDropDesigner
{
    [UpdateBefore(typeof(PickUpAndDropAppliance))]
    [UpdateBefore(typeof(MouseStoreBlueprint))]
    internal class MouseRetrieveBlueprint : MouseApplianceInteractionSystem
    {
        private CItemHolder Holder;

        private CBlueprintStore Store;

        protected override InteractionType RequiredType => InteractionType.Grab;

        protected override MouseButton Button => Main.BlueprintButton;

        protected override bool IsPossible(ref InteractionData data)
        {
            if (!Require<CItemHolder>(data.Interactor, out Holder))
            {
                return false;
            }
            if (!Require<CBlueprintStore>(data.Target, out Store))
            {
                return false;
            }
            if (Holder.HeldItem != default(Entity))
            {
                return false;
            }
            if (!Store.InUse)
            {
                return false;
            }
            data.Attempt.Result = InteractionResult.Possible;
            return true;

        }

        protected override void Perform(ref InteractionData data)
        {
            Entity entity = data.Context.CreateEntity();
            data.Context.Set(entity, new CCreateAppliance
            {
                ID = Store.BlueprintID
            });
            data.Context.Set(entity, GetComponent<CPosition>(data.Interactor));
            data.Context.Set(entity, new CApplianceBlueprint
            {
                Appliance = Store.ApplianceID,
                IsCopy = Store.HasBeenCopied
            });
            if (!Preferences.Get<bool>(Pref.RequirePingForBlueprintInfo))
            {
                data.Context.Set(entity, new CShowApplianceInfo
                {
                    Appliance = Store.ApplianceID,
                    Price = Store.Price,
                    ShowPrice = true
                });
            }
            data.Context.Set(entity, new CForSale
            {
                Price = Store.Price
            });
            data.Context.Set(entity, default(CShopEntity));
            data.Context.Add<CHeldAppliance>(entity);
            data.Context.Set(entity, new CHeldBy
            {
                Holder = data.Interactor
            });
            data.Context.Set(data.Interactor, new CItemHolder
            {
                HeldItem = entity
            });
            data.Context.Set(data.Interactor, new CPerformedThisFrame());
            if (Store.HasBeenCopied)
            {
                Store.HasBeenCopied = false;
            }
            else
            {
                Store.InUse = false;
                Store.ApplianceID = 0;
                Store.Price = 0;
                Store.BlueprintID = 0;
            }
            Store.HasBeenMadeFree = false;
            SetComponent(data.Target, Store);

            data.Attempt.Result = InteractionResult.Performed;

            if (Require<CPlayer>(data.Interactor, out CPlayer cPlayer) && MouseHelpers.IsKeyboardOrFirstLocalPlayer(cPlayer))
                ManageApplianceGhostsOriginalLambdaBodyPatch.isPickedUpByMouse = true;
        }
    }
}
