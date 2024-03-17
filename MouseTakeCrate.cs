using Controllers;
using Kitchen;

namespace KitchenDragNDropDesigner
{
    public class MouseTakeCrate : MouseItemInteractionSystem
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

            if (!Require(data.Target, out TargetHolder) ||
                !Has<CCrate>(TargetHolder.HeldItem))
                return false;
            if (!Require(data.Interactor, out PlayerHolder) ||
                PlayerHolder.HeldItem != default)
                return false;
            return true;
        }

        protected override void Perform(ref InteractionData data)
        {
            data.Context.UpdateHolder(TargetHolder.HeldItem, data.Interactor);
        }
    }
}
