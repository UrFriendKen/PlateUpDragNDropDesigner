using HarmonyLib;
using Kitchen;
using System.Collections.Generic;
using System.Reflection;

namespace KitchenDragNDropDesigner.Patches
{
    [HarmonyPatch]
    static class IsPossibleDisable_Patches
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            return new MethodBase[]
            {
                typeof(PickUpAndDropAppliance).GetMethod("IsPossible", BindingFlags.NonPublic | BindingFlags.Instance)
            };
        }

        static bool Prefix(ref InteractionData data, ref bool __result)
        {
            if (data.Context.Require(data.Interactor, out CMouseData mouseData) &&
                mouseData.Active)
            {
                __result = false;
                return false;
            }
            Main.LogError("Fall through");
            return true;
        }
    }
}
