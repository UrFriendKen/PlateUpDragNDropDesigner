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
    }
}
