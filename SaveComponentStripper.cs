using Kitchen;
using KitchenMods;
using Unity.Entities;
using static KitchenDragNDropDesigner.MousePurchaseAfterDuration;
using static KitchenDragNDropDesigner.MouseRemotePseudoInteraction;

namespace KitchenDragNDropDesigner
{
    public class SaveComponentStripper : GenericSystemBase, IModSystem
    {
        EntityQuery MouseDatas;
        EntityQuery CachedRotations;
        EntityQuery PickedUpByMouse;
        EntityQuery PurchaseProgress;
        EntityQuery PerformedThisFrame;
        EntityQuery LinkedInteractionProxy;

        protected override void Initialise()
        {
            base.Initialise();
            MouseDatas = GetEntityQuery(typeof(CMouseData));
            CachedRotations = GetEntityQuery(typeof(CCachedRotation));
            PickedUpByMouse = GetEntityQuery(typeof(CPickedUpByMouse));
            PurchaseProgress = GetEntityQuery(typeof(CPurchaseProgress));
            PerformedThisFrame = GetEntityQuery(typeof(CPerformedThisFrame));
            LinkedInteractionProxy = GetEntityQuery(typeof(CLinkedInteractionProxy));
        }

        protected override void OnUpdate()
        {
        }

        public override void BeforeSaving(SaveSystemType system_type)
        {
            base.BeforeSaving(system_type);

            if (system_type != SaveSystemType.FullWorld)
                return;

            EntityManager.RemoveComponent<CMouseData>(MouseDatas);
            EntityManager.RemoveComponent<CCachedRotation>(CachedRotations);
            EntityManager.RemoveComponent<CPickedUpByMouse>(PickedUpByMouse);
            EntityManager.RemoveComponent<CPurchaseProgress>(PurchaseProgress);
            EntityManager.RemoveComponent<CPerformedThisFrame>(PerformedThisFrame);
            EntityManager.RemoveComponent<CLinkedInteractionProxy>(LinkedInteractionProxy);
        }
    }
}
