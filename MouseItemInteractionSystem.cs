using Kitchen;

namespace KitchenDragNDropDesigner
{
    public abstract class MouseItemInteractionSystem : MouseInteractionSystem
    {
        protected override InteractionMode RequiredMode => InteractionMode.Items;
    }
}
