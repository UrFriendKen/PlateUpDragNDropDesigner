using Kitchen;
using Unity.Entities;

namespace KitchenDragNDropDesigner
{
    [UpdateBefore(typeof(MousePingedApplianceInfo))]
    internal class MouseInteractRotatePushDuringNight : MouseApplianceInteractionSystem
    {
        private CConveyPushRotatable Rotatable;
        protected override CMouseData.Action Action => CMouseData.Action.Miscellaneous;
        protected override InteractionType RequiredType => InteractionType.Notify;

        protected override bool IsPossibleCondition(ref InteractionData data)
        {
            if (!Require(data.Target, out Rotatable))
            {
                return false;
            }

            if (Rotatable.Target == Orientation.Null)
            {
                return false;
            }
            return true;
        }

        protected override void Perform(ref InteractionData data)
        {
            Rotatable.Target = Rotatable.Target.RotateCW();
            if (Rotatable.Target == Orientation.Down)
            {
                Rotatable.Target = Orientation.Left;
            }
            Set(data.Target, Rotatable);
        }
    }
}
