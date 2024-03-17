using Controllers;
using Kitchen;
using KitchenData;

namespace KitchenDragNDropDesigner
{
    public class MouseDropCrate : MouseItemInteractionSystem
    {
        CItemHolder PlayerHolder;
        CItemHolder TargetHolder;

        protected override CMouseData.Action Action => CMouseData.Action.Grab;

        protected override InteractionType RequiredType => InteractionType.Grab;

        protected override void Initialise()
        {
            base.Initialise();
            RequireSingletonForUpdate<SFranchiseMarker>();
        }

        protected override bool IsPossibleCondition(ref InteractionData data)
        {
            if (!Require(data.Interactor, out CPlayer player) ||
                player.InputSource != InputSourceIdentifier.Identifier ||
                !Main.PrefManager.Get<bool>(Main.HOST_MANIPULATE_HQ_CRATES_ID))
                return false;

            if (!Require(data.Interactor, out PlayerHolder) ||
                PlayerHolder.HeldItem == default ||
                !Has<CCrate>(PlayerHolder.HeldItem) ||
                !Require(PlayerHolder.HeldItem, out CItem item) ||
                !GameData.Main.TryGet<Item>(item.ID, out var itemGDO))
                return false;

            if (!Require(data.Target, out TargetHolder) ||
                TargetHolder.HeldItem != default)
                return false;

            if (Require(data.Target, out CItemHolderFilter itemHolderFilter))
            {
                if (itemHolderFilter.NoDirectInsertion || !itemHolderFilter.AllowCategory(itemGDO.ItemCategory))
                    return false;
            }
            else if (itemGDO.ItemCategory != ItemCategory.Generic)
                return false;

            if (Require(data.Target, out CItemHolderOnlySpecificItem itemHolderOnlySpecficItem) &&
                item.ID != itemHolderOnlySpecficItem.ItemID)
                return false;

            return true;
        }

        protected override void Perform(ref InteractionData data)
        {
            data.Context.UpdateHolder(PlayerHolder.HeldItem, data.Target);
        }
    }
}
