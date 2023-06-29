using Kitchen;
using Unity.Entities;

namespace KitchenDragNDropDesigner
{
    [UpdateAfter(typeof(MousePickUpAndDropAppliance))]
    internal class MouseOpenLetter : MouseApplianceInteractionSystem
    {
        protected override MouseButton Button => Main.GrabButton;

        protected override bool IsPossible(ref InteractionData data)
        {
            return Has<CLetterBlueprint>(data.Target) &&
                Has<CPosition>(data.Target);
        }

        protected override void Perform(ref InteractionData data)
        {
            PostHelpers.OpenBlueprintLetter(data.Context, data.Target);
            data.Context.Destroy(data.Target);
        }
    }
}
