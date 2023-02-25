using Kitchen;
using Unity.Entities;

namespace KitchenDragNDropDesigner
{
    public struct CPerformedThisFrame : IComponentData { }

    [UpdateInGroup(typeof(LowPriorityInteractionGroup))]
    internal class CleanUpComponents : GenericSystemBase
    {
        EntityQuery Performed;
        EntityQuery Purchase;

        protected override void Initialise()
        {
            base.Initialise();
            Performed = GetEntityQuery(new QueryHelper()
                .All(typeof(CPerformedThisFrame)));

            Purchase = GetEntityQuery(new QueryHelper()
                .All(typeof(MousePurchaseAfterDuration.CPurchaseProgress))
                .None(typeof(CPerformedThisFrame)));
        }

        protected override void OnUpdate()
        {
            EntityManager.RemoveComponent<MousePurchaseAfterDuration.CPurchaseProgress>(Purchase);
            EntityManager.RemoveComponent<CPerformedThisFrame>(Performed);
        }
    }
}
