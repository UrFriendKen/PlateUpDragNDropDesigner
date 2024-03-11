using Controllers;
using Unity.Entities;
using UnityEngine;

namespace KitchenDragNDropDesigner
{
    public struct CMouseData : IComponentData
    {
        public enum Action
        {
            None,
            Grab,
            Act,
            Ping,
            StoreRetrieveBlueprint,
            Miscellaneous
        }

        public bool Active;
        public Vector3 Position;
        public ButtonState Grab;
        public ButtonState Act;
        public ButtonState Ping;
        public ButtonState StoreRetrieveBlueprint;
        public ButtonState Miscellaneous;

        public bool GrabPositionOverride;
        public bool ActPositionOverride;
        public bool PingPositionOverride;
        public bool StoreRetrieveBlueprintPositionOverride;
        public bool MiscellaneousPositionOverride;

        public ButtonState GetButtonState(Action action)
        {
            switch (action)
            {
                case Action.Grab:
                    return Grab;
                case Action.Act:
                    return Act;
                case Action.Ping:
                    return Ping;
                case Action.StoreRetrieveBlueprint:
                    return StoreRetrieveBlueprint;
                case Action.Miscellaneous:
                    return Miscellaneous;
                default:
                    return ButtonState.Up;
            }
        }

        public bool ShouldOverridePosition(Action action)
        {
            switch (action)
            {
                case Action.Grab:
                    return GrabPositionOverride;
                case Action.Act:
                    return ActPositionOverride;
                case Action.Ping:
                    return PingPositionOverride;
                case Action.StoreRetrieveBlueprint:
                    return StoreRetrieveBlueprintPositionOverride;
                case Action.Miscellaneous:
                    return MiscellaneousPositionOverride;
                default:
                    return false;
            }
        }
    }
}
