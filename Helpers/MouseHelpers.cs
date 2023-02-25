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
    }
}
