using Controllers;
using HarmonyLib;
using Kitchen;
using KitchenDragNDropDesigner.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Unity.Entities;

namespace KitchenDragNDropDesigner.Patches
{
    [HarmonyPatch]
    public static class ManageApplianceGhostsOriginalLambdaBodyPatch
    {
        static MethodBase TargetMethod()
        {
            Type type = AccessTools.FirstInner(typeof(ManageApplianceGhosts), t => t.Name.Contains("c__DisplayClass_OnUpdate_LambdaJob1"));
            return AccessTools.FirstMethod(type, method => method.Name.Contains("OriginalLambdaBody"));
        }

        static void Prefix(ref CAttemptingInteraction interact)
        {
            if (MousePickUpAndDropAppliance.isPickedUpByMouse &&
                MouseHelpers.TryGetPlayerFromInteractionAttempt(interact, out Entity player) &&
                Main.instance.EntityManager.RequireComponent(player, out CPlayer cPlayer) &&
                MouseHelpers.IsKeyboardOrFirstLocalPlayer(cPlayer))
            {
                interact.Location = MouseHelpers.MousePlanePos();
            }
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> _instructions)
        {
            var codes = new List<CodeInstruction>(_instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldloc_0 &&
                    codes[i + 1].opcode == OpCodes.Ldarg_0 &&
                    codes[i + 2].opcode == OpCodes.Ldfld &&
                    codes[i + 3].opcode == OpCodes.Ldarg_3 &&
                    codes[i + 4].opcode == OpCodes.Ldobj &&
                    codes[i + 5].opcode == OpCodes.Call &&
                    codes[i + 6].opcode == OpCodes.Ldarg_2 &&
                    codes[i + 7].opcode == OpCodes.Ldfld &&
                    codes[i + 8].opcode == OpCodes.Ldc_I4_0 &&
                    codes[i + 9].opcode == OpCodes.Call &&
                    codes[i + 10].opcode == OpCodes.And &&
                    codes[i + 11].opcode == OpCodes.Stloc_0)
                {
                    codes[i + 0].opcode = OpCodes.Ldc_I4_1;
                    codes[i + 1].opcode = OpCodes.Stloc_0;
                    for (int j = 2; j < 12; j++)
                    {
                        codes[i + j].opcode = OpCodes.Nop;
                    }
                }
            }

            return codes.AsEnumerable();
        }
    }
}
