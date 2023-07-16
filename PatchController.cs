using Kitchen;
using KitchenMods;
using System;
using System.Collections.Generic;
using System.Deployment.Internal;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

namespace KitchenDragNDropDesigner
{
    public class PatchController : GenericSystemBase, IModSystem
    {
        private static PatchController _instance;

        internal static EntityQuery Players;

        protected override void Initialise()
        {
            base.Initialise();
            _instance = this;
            Players = GetEntityQuery(typeof(CPlayer));
        }

        protected override void OnUpdate()
        {
        
        }

        internal static bool RequireStatic<T>(Entity e, out T comp) where T : struct, IComponentData
        {
            comp = default;
            return _instance?.Require<T>(e, out comp) ?? false;
        }

        internal static EntityQuery GetEntityQueryStatic(params ComponentType[] componentTypes)
        {
            return _instance?.GetEntityQuery(componentTypes) ?? default;
        }

        internal static EntityQuery GetEntityQueryStatic(params EntityQueryDesc[] queryDesc)
        {
            return _instance?.GetEntityQuery(queryDesc) ?? default;
        }

        internal static bool CanReachIfNotPickedUpByMouse(bool isPickedUpByMouse, Vector3 from, Vector3 to, bool do_not_swap = false)
        {
            return isPickedUpByMouse || (_instance?.CanReach(from, to, do_not_swap) ?? true);
        }
    }
}
