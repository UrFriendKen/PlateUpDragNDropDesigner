using Kitchen;
using KitchenDragNDropDesigner.Helpers;
using KitchenDragNDropDesigner.Patches;
using Unity.Entities;

namespace KitchenDragNDropDesigner
{
    internal class MouseStoreBlueprint : MouseApplianceInteractionSystem
    {
        private CItemHolder Holder;

        private CApplianceBlueprint Blueprint;

        private CForSale Sale;

        private CAppliance Appliance;

        private CBlueprintStore Store;

        protected override MouseButton Button => Main.BlueprintButtonPreference.Get();

        protected override InteractionType RequiredType => InteractionType.Grab;

        protected override bool IsPossible(ref InteractionData data)
        {
            if (!Require<CItemHolder>(data.Interactor, out Holder))
            {
                return false;
            }
            if (!Require<CApplianceBlueprint>((Entity)Holder, out Blueprint))
            {
                return false;
            }
            if (!Require<CForSale>((Entity)Holder, out Sale))
            {
                return false;
            }
            if (!Require<CAppliance>((Entity)Holder, out Appliance))
            {
                return false;
            }
            if (!Require<CBlueprintStore>(data.Target, out Store))
            {
                return false;
            }
            if (Has<CPerformedThisFrame>(data.Interactor))
            {
                return false;
            }
            data.Attempt.Result = InteractionResult.Possible;
            return true;
        }

        protected override void Perform(ref InteractionData data)
        {
            if (!Store.InUse && !Blueprint.IsCopy)
            {
                data.Context.Destroy(Holder.HeldItem);
                data.Context.Set(data.Interactor, default(CItemHolder));
                Store.InUse = true;
                Store.Price = Sale.Price;
                Store.ApplianceID = Blueprint.Appliance;
                Store.BlueprintID = Appliance.ID;
                Store.HasBeenCopied = false;
                Store.HasBeenMadeFree = false;
                SetComponent(data.Target, Store);
                data.Attempt.Result = InteractionResult.Performed;

                if (Require<CPlayer>(data.Interactor, out CPlayer cPlayer) && MouseHelpers.IsKeyboardOrFirstLocalPlayer(cPlayer))
                    ManageApplianceGhostsOriginalLambdaBodyPatch.isPickedUpByMouse = false;
            }
            else if (!Store.HasBeenCopied && Store.Price == Sale.Price && Store.ApplianceID == Blueprint.Appliance)
            {
                data.Context.Destroy(Holder.HeldItem);
                data.Context.Set(data.Interactor, default(CItemHolder));
                Store.HasBeenCopied = true;
                SetComponent(data.Target, Store);
                data.Attempt.Result = InteractionResult.Performed;

                if (Require<CPlayer>(data.Interactor, out CPlayer cPlayer) && MouseHelpers.IsKeyboardOrFirstLocalPlayer(cPlayer))
                    ManageApplianceGhostsOriginalLambdaBodyPatch.isPickedUpByMouse = false;
            }
        }
    }
}
