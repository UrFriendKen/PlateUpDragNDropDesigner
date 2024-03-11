using Kitchen;
using KitchenMods;
using Unity.Entities;

namespace KitchenDragNDropDesigner
{
    public class SaveComponentStripper : GenericSystemBase, IModSystem
    {
        EntityQuery MouseDatas;

        protected override void Initialise()
        {
            base.Initialise();
            MouseDatas = GetEntityQuery(typeof(CMouseData));
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
        }
    }
}
