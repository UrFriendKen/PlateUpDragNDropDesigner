using Kitchen;

namespace KitchenDragNDropDesigner
{
    public abstract class MouseApplianceInteractionSystem : MouseInteractionSystem
    {
        protected override InteractionMode RequiredMode => InteractionMode.Appliances;
    }
}
