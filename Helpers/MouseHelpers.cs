using Controllers;
using Kitchen;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KitchenDragNDropDesigner.Helpers
{
    public static class MouseHelpers
    {
        /// <summary>
        /// Mouse position in world at planeYOffset (default: 0.3f)
        /// </summary>
        /// <returns>Position on plane; otherwise 0,0,0</returns>
        public static Vector3 MousePlanePos(float planeYOffset = 0.3f)
        {
            Plane plane = new Plane(Vector3.down, planeYOffset);
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (plane.Raycast(ray, out float distance))
            {
                Vector3 intersection = ray.GetPoint(distance);
                return new Vector3(intersection.x, intersection.y - planeYOffset, intersection.z);
            }
            return Vector3.zero;
        }



        /// <summary>
        /// Get entity of player from its CAttemptingLocation component
        /// </summary>
        /// <returns>true if player entity is found; otherwise false</returns>
        public static bool TryGetPlayerFromInteractionAttempt(CAttemptingInteraction attempt, out Entity entity)
        {
            using NativeArray<Entity> entities = PatchController.Players.ToEntityArray(Allocator.Temp);
            using NativeArray<CAttemptingInteraction> playerAttempts = PatchController.Players.ToComponentDataArray<CAttemptingInteraction>(Allocator.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                if (attempt.Type == playerAttempts[i].Type &&
                    attempt.Target == playerAttempts[i].Target &&
                    (attempt.Location - playerAttempts[i].Location).sqrMagnitude < 0.0001f)
                {
                    entity = entities[i];
                    return true;
                }
            }
            entity = default;
            return false;
        }

        internal static bool TryGetBoundMouseButton(string prefKey, out MouseApplianceInteractionSystem.MouseButton button)
        {
            return Enum.TryParse(Main.PrefManager.Get<string>(prefKey), true, out button);
        }

        internal static bool IsMouseButtonPressed(string prefKey)
        {
            if (!TryGetBoundMouseButton(prefKey, out MouseApplianceInteractionSystem.MouseButton button))
            {
                Main.LogError($"Failed to parse {prefKey} to {typeof(MouseApplianceInteractionSystem.MouseButton)}.");
                return false;
            }
            return IsMouseButtonPressed(button);
        }

        internal static bool IsMouseButtonPressed(MouseApplianceInteractionSystem.MouseButton button)
        {
            switch (button)
            {
                case MouseApplianceInteractionSystem.MouseButton.Left:
                    return Mouse.current.leftButton.IsPressed();
                case MouseApplianceInteractionSystem.MouseButton.Middle:
                    return Mouse.current.middleButton.IsPressed();
                case MouseApplianceInteractionSystem.MouseButton.Right:
                    return Mouse.current.rightButton.IsPressed();
                case MouseApplianceInteractionSystem.MouseButton.Forward:
                    return Mouse.current.forwardButton.IsPressed();
                case MouseApplianceInteractionSystem.MouseButton.Back:
                    return Mouse.current.backButton.IsPressed();
                default:
                    return false;
            }
        }

        static Dictionary<string, string> _prefKeyToAction = new Dictionary<string, string>()
        {
            { Main.GRAB_BUTTON_PREF_ID, Controls.Interact1 },
            { Main.ACT_BUTTON_PREF_ID, Controls.Interact2 },
            { Main.PING_BUTTON_PREF_ID, Controls.Interact4 }
        };

        internal static bool IsBindingConflict(int playerID, string prefKey)
        {
            if (!_prefKeyToAction.TryGetValue(prefKey, out string action) || !TryGetBoundMouseButton(prefKey, out MouseApplianceInteractionSystem.MouseButton button))
                return false;
            return IsBindingConflict(playerID, button, action);
        }

        internal static bool IsBindingConflict(int playerID, MouseApplianceInteractionSystem.MouseButton button, string action)
        {
            string bindingName = InputSourceIdentifier.DefaultInputSource.GetBindingName(playerID, action);
            switch (button)
            {
                case MouseApplianceInteractionSystem.MouseButton.Left:
                    return bindingName == "leftButton";
                case MouseApplianceInteractionSystem.MouseButton.Middle:
                    return bindingName == "middleButton";
                case MouseApplianceInteractionSystem.MouseButton.Right:
                    return bindingName == "rightButton";
                case MouseApplianceInteractionSystem.MouseButton.Forward:
                    return bindingName == "forwardButton";
                case MouseApplianceInteractionSystem.MouseButton.Back:
                    return bindingName == "backButton";
                default:
                    return false;
            }
        }
    }
}
