using Controllers;
using Kitchen;
using KitchenData;
using System;
using System.Reflection;
using Unity.Entities;

namespace KitchenDragNDropDesigner
{
    [UpdateBefore(typeof(MouseDropCrate))]
    public class MouseHideCrate : MouseItemInteractionSystem
    {
        CItemHolder PlayerHolder;
        CItemHolder TargetHolder;

        protected override bool UseImmediateContext => true;

        protected override CMouseData.Action Action => CMouseData.Action.Grab;

        protected override InteractionType RequiredType => InteractionType.Grab;

        static Type t_CCircleLineTrack = typeof(AchievementCircleLine).GetNestedType("CCircleLineTrack", BindingFlags.NonPublic);

        ComponentType ct_CCircleLineTrack = t_CCircleLineTrack != null ? ((ComponentType)t_CCircleLineTrack) : default;

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
                TargetHolder.HeldItem == default)
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
            Entity e = PlayerHolder.HeldItem;

            if (data.Context.Has<CItem>(e))
                data.Context.Remove<CItem>(e);
            if (data.Context.Has<CRequiresView>(e))
                data.Context.Remove<CRequiresView>(e);
            if (data.Context.Has<CHeldBy>(e))
                data.Context.Remove<CHeldBy>(e);
            if (data.Context.Has<CPosition>(e))
                data.Context.Remove<CPosition>(e);
            if (data.Context.Has<AchievementAntisocial.CAntisocialTracker>(e))
                data.Context.Remove<AchievementAntisocial.CAntisocialTracker>(e);

            if (ct_CCircleLineTrack != null && EntityManager.HasComponent(e, ct_CCircleLineTrack))
                EntityManager.RemoveComponent(e, ct_CCircleLineTrack);
        }
    }
}
