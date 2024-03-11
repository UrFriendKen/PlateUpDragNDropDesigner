using Controllers;
using Kitchen;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenDragNDropDesigner
{
    public class AddMouseData : GameSystemBase, IModSystem
    {
        EntityQuery Players;

        protected override void Initialise()
        {
            base.Initialise();
            Players = GetEntityQuery(new QueryHelper()
                .All(typeof(CPlayer))
                .None(typeof(CMouseData)));
        }

        protected override void OnUpdate()
        {
            using NativeArray<Entity> entities = Players.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                Set(entities[i], default(CMouseData));
            }
        }
    }
}
