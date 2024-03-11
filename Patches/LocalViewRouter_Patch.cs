using HarmonyLib;
using Kitchen;
using KitchenDragNDropDesigner.Views;
using UnityEngine;

namespace KitchenDragNDropDesigner.Patches
{
    [HarmonyPatch]
    static class LocalViewRouter_Patch
    {
        [HarmonyPatch(typeof(LocalViewRouter), "GetPrefab")]
        [HarmonyPostfix]
        static void GetPrefab_Postfix(ViewType view_type, ref GameObject __result)
        {
            if (view_type == ViewType.Player &&
                __result != null &&
                __result.GetComponent<MouseInputView>() == null)
            {
                __result.AddComponent<MouseInputView>();
            }
        }
    }
}
