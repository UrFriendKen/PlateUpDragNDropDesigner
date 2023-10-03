using Kitchen;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace KitchenDragNDropDesigner
{
    public struct CCachedRotation : IComponentData, IModComponent
    {
        public Quaternion Rotation;
    }

    public class ResetCachedApplianceRotation : GameSystemBase, IModSystem
    {
        EntityQuery Appliances;

        protected override void Initialise()
        {
            base.Initialise();
            Appliances = GetEntityQuery(new QueryHelper()
                .All(typeof(CAppliance), typeof(CPosition), typeof(CCachedRotation))
                .None(typeof(CHeldAppliance)));
        }

        protected override void OnUpdate()
        {
            if (Main.PrefManager.Get<bool>(Main.MAINTAIN_ORIENTATION_ID))
            {
                using NativeArray<Entity> entities = Appliances.ToEntityArray(Allocator.Temp);
                using NativeArray<CPosition> positions = Appliances.ToComponentDataArray<CPosition>(Allocator.Temp);
                using NativeArray<CCachedRotation> orientations = Appliances.ToComponentDataArray<CCachedRotation>(Allocator.Temp);

                for (int i = 0; i < entities.Length; i++)
                {
                    Entity entity = entities[i];
                    CPosition position = positions[i];
                    CCachedRotation cache = orientations[i];
                    position.Rotation = cache.Rotation;
                    Set(entity, position);
                }
            }
            EntityManager.RemoveComponent<CCachedRotation>(Appliances);
        }
    }
}
