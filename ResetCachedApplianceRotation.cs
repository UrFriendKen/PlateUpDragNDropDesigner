using Kitchen;
using KitchenMods;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenDragNDropDesigner
{
    [Obsolete("This component is only used for backwards compatibility, allowing saves to deserialize properly. Do not use!")]
    public struct CCachedRotation : IComponentData, IModComponent
    {
        public Quaternion Rotation;
    }
}
