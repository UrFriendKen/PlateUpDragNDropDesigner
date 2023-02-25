using Kitchen;
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
    }
}
