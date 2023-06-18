using Kitchen;
using KitchenData;
using Unity.Entities;

namespace KitchenDragNDropDesigner
{
    [UpdateInGroup(typeof(InteractionGroup), OrderLast = true)]
    internal class MouseLookAtAppliance : MouseApplianceInteractionSystem
    {
        protected override InteractionType RequiredType => InteractionType.Look;
        protected override InteractionMode RequiredMode => InteractionMode.Appliances;

        protected override bool IsPossible(ref InteractionData data)
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

        protected override void UpdateInteractionData(ref InteractionData interaction_data)
        {
            UpdateMouseTarget(ref interaction_data, OccupancyLayer.Default);
        }
    }
}
