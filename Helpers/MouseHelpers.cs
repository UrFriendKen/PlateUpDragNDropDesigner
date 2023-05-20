using Controllers;
using Kitchen;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using UniverseLib;
using static UnityEngine.EventSystems.EventTrigger;

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
            using NativeArray<Entity> entities = Main.Players.ToEntityArray(Allocator.Temp);
            using NativeArray<CAttemptingInteraction> playerAttempts = Main.Players.ToComponentDataArray<CAttemptingInteraction>(Allocator.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                if (attempt.Type == playerAttempts[i].Type &&
                    attempt.Target == playerAttempts[i].Target &&
                    (attempt.Location - playerAttempts[i].Location).sqrMagnitude < 0.01f)
                {
                    entity = entities[i];
                    return true;
                }
            }
            entity = default;
            return false;
        }

        /// <summary>
        /// Check if player is local and is using a keyboard. Failing that, whether the player is the first local player.
        /// </summary>
        /// <returns>true if player matches description; otherwise false</returns>
        public static bool IsKeyboardOrFirstLocalPlayer(CPlayer cPlayer)
        {
            int? firstLocalPlayerIndex = null;
            NativeArray<CPlayer> players = Main.Players.ToComponentDataArray<CPlayer>(Allocator.Temp);
            foreach (var player in players)
            {
                if (player.InputSource == InputSourceIdentifier.Identifier.Value)
                {
                    if (!firstLocalPlayerIndex.HasValue || player.Index < firstLocalPlayerIndex)
                    {
                        firstLocalPlayerIndex = player.Index;
                    }
                    continue;
                }

                if (player.InputSource == cPlayer.InputSource)
                {
                    break;
                }
            }
            players.Dispose();
            if (!firstLocalPlayerIndex.HasValue)
            {
                //Main.LogInfo("Is Remote Player");
                return false;
            }

            //InputDevice keyboard = null;
            //int i = 0;
            //foreach (InputDevice device in InputSystem.devices)
            //{
            //    if (device is Keyboard)
            //    {
            //        keyboard = device;
            //        break;
            //    }
            //}
            //Main.LogInfo(Players.Main.Get(cPlayer.ID).Identifier.Value);
            //InputDevice playerDevice = InputSystem.GetDeviceById(Players.Main.Get(cPlayer.ID).Identifier.Value);
            //Main.LogInfo($"Device ID = {playerDevice.deviceId}");
            //if (playerDevice is Keyboard)// keyboard)
            //{
            //    Main.LogInfo("Is Keyboard Player");
            //    return true;
            //}

            //Main.LogInfo($"{(cPlayer.Index == firstLocalPlayerIndex ? "Is First Player" : "Not First Player")}");
            return cPlayer.Index == firstLocalPlayerIndex;
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
    }
}
