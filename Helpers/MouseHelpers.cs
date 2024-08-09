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
        static Camera _mainCamera;

        static Camera MainCamera 
        {
            get
            {
                if (_mainCamera == null)
                {
                    _mainCamera = Camera.main;
                }
                return _mainCamera;
            }
        }
        
        /// <summary>
        /// Mouse position in world at planeYOffset (default: 0.3f)
        /// </summary>
        /// <returns>Position on plane; otherwise 0,0,0</returns>
        public static Vector3 MousePlanePos(float planeYOffset = 0.3f)
        {
            Plane plane = new Plane(Vector3.down, planeYOffset);
            Ray ray = MainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
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
