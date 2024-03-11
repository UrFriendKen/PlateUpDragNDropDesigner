using Kitchen;
using KitchenMods;
using System.Runtime.InteropServices;
using Unity.Entities;

namespace KitchenDragNDropDesigner
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct CPickedUpByMouse : IComponentData, IModComponent { }

    public class CleanPickedUpByMouse : GameSystemBase, IModSystem
    {
        EntityQuery Appliances;

        protected override void Initialise()
        {
            base.Initialise();
            Appliances = GetEntityQuery(new QueryHelper()
                .All(typeof(CPickedUpByMouse))
                .None(typeof(CHeldAppliance)));
            RequireForUpdate(Appliances);
        }

        protected override void OnUpdate()
        {
            EntityManager.RemoveComponent<CPickedUpByMouse>(Appliances);
        }
    }
}
