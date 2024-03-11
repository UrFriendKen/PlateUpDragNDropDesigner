using Kitchen;
using Unity.Entities;

namespace KitchenDragNDropDesigner
{
    [UpdateInGroup(typeof(InteractionGroup), OrderLast = true)]
    internal class MouseLookAtAppliance : MouseApplianceInteractionSystem
    {
        protected override InteractionType RequiredType => InteractionType.Look;

        protected override bool IsPossibleCondition(ref InteractionData data)
        {
            if (!Has<CAppliance>(data.Target))
            {
                return false;
            }
            return true;
        }

        protected override void Perform(ref InteractionData data)
        {
            Set<CBeingLookedAt>(data.Target);
        }
    }
}
