using HarmonyLib;
using Kitchen;
using System;
using System.Reflection;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace KitchenDragNDropDesigner.Patches
{
    [HarmonyPatch]
    internal static class ManageApplianceGhostsOriginalLambdaBodyPatch
    {
        public static MethodBase TargetMethod()
        {
            Type type = AccessTools.FirstInner(typeof(ManageApplianceGhosts), t => t.Name.Contains($"c__DisplayClass_OnUpdate_LambdaJob1"));
            return AccessTools.FirstMethod(type, method => method.Name.Contains("OriginalLambdaBody"));
        }

        [HarmonyPrefix]
        static void Prefix(ref (bool IsChanged, Vector3 InteractLocation, Vector3 PlayerPos) __state, Entity player, ref CAttemptingInteraction interact, ref CPosition pos)
        {
            __state = (false, Vector3.zero, Vector3.zero);
            bool isPickedUpByMouse = PatchController.RequireStatic(player, out CMouseData mouseData) &&
                mouseData.Active &&
                PatchController.RequireStatic(player, out CItemHolder holder) &&
                holder.HeldItem != default &&
                PatchController.HasStatic<CPickedUpByMouse>(holder.HeldItem);
            
            if (isPickedUpByMouse)
            {
                __state = (true, interact.Location, pos.Position);
                interact.Location = mouseData.Position;
                pos.Position = mouseData.Position;
            }
            else if (PatchController.RequireStatic(player, out CInputData input) &&
                PatchController.RequireStatic(player, out CIsInteractor interactor))
            {
                float3 playerFacing = math.normalizesafe(new float3(input.State.Movement.x, 0f, input.State.Movement.y), math.mul(pos.Rotation, new float3(0f, 0f, 1f)));
                float3 interactionPosition = new float3(pos.Position) + playerFacing * interactor.InteractionOffset;
                if (!PatchController.StaticCanReach(pos, interactionPosition))
                {
                    interactionPosition = new float3(pos.Position) + playerFacing * interactor.InteractionOffset * 0.25f;
                }
                interact.Location = interactionPosition;
            }
        }

        [HarmonyPostfix]
        static void Postfix(ref (bool IsChanged, Vector3 InteractLocation, Vector3 PlayerPos) __state, ref CAttemptingInteraction interact, ref CPosition pos)
        {
            if (__state.IsChanged)
            {
                interact.Location = __state.InteractLocation;
                pos.Position = __state.PlayerPos;
            }
        }

        //[HarmonyTranspiler]
        //static IEnumerable<CodeInstruction> OriginalLambdaBody_Transpiler(IEnumerable<CodeInstruction> instructions)
        //{
        //    Main.LogInfo($"{TARGET_TYPE.Name} Transpiler");
        //    if (!(DESCRIPTION == null || DESCRIPTION == string.Empty))
        //        Main.LogInfo(DESCRIPTION);
        //    List<CodeInstruction> list = instructions.ToList();

        //    int matches = 0;
        //    int windowSize = OPCODES_TO_MATCH.Count;
        //    for (int i = 0; i < list.Count - windowSize; i++)
        //    {
        //        for (int j = 0; j < windowSize; j++)
        //        {
        //            if (OPCODES_TO_MATCH[j] == null)
        //            {
        //                Main.LogError("OPCODES_TO_MATCH cannot contain null!");
        //                return instructions;
        //            }

        //            string logLine = $"{j}:\t{OPCODES_TO_MATCH[j]}";

        //            int index = i + j;
        //            OpCode opCode = list[index].opcode;
        //            if (j < OPCODES_TO_MATCH.Count && opCode != OPCODES_TO_MATCH[j])
        //            {
        //                if (j > 0)
        //                {
        //                    logLine += $" != {opCode}";
        //                    Main.LogInfo($"{logLine}\tFAIL");
        //                }
        //                break;
        //            }
        //            logLine += $" == {opCode}";

        //            if (j == 0)
        //                Debug.Log("-------------------------");

        //            if (j < OPERANDS_TO_MATCH.Count && OPERANDS_TO_MATCH[j] != null)
        //            {
        //                logLine += $"\t{OPERANDS_TO_MATCH[j]}";
        //                object operand = list[index].operand;
        //                if (OPERANDS_TO_MATCH[j] != operand)
        //                {
        //                    logLine += $" != {operand}";
        //                    Main.LogInfo($"{logLine}\tFAIL");
        //                    break;
        //                }
        //                logLine += $" == {operand}";
        //            }
        //            Main.LogInfo($"{logLine}\tPASS");

        //            if (j == OPCODES_TO_MATCH.Count - 1)
        //            {
        //                Main.LogInfo($"Found match {++matches}");
        //                if (matches > EXPECTED_MATCH_COUNT)
        //                {
        //                    Main.LogError("Number of matches found exceeded EXPECTED_MATCH_COUNT! Returning original IL.");
        //                    return instructions;
        //                }

        //                // Perform replacements
        //                for (int k = 0; k < MODIFIED_OPCODES.Count; k++)
        //                {
        //                    int replacementIndex = i + k;
        //                    if (MODIFIED_OPCODES[k] == null || list[replacementIndex].opcode == MODIFIED_OPCODES[k])
        //                    {
        //                        continue;
        //                    }
        //                    OpCode beforeChange = list[replacementIndex].opcode;
        //                    list[replacementIndex].opcode = MODIFIED_OPCODES[k];
        //                    Main.LogInfo($"Line {replacementIndex}: Replaced Opcode ({beforeChange} ==> {MODIFIED_OPCODES[k]})");
        //                }

        //                for (int k = 0; k < MODIFIED_OPERANDS.Count; k++)
        //                {
        //                    if (MODIFIED_OPERANDS[k] != null)
        //                    {
        //                        int replacementIndex = i + k;
        //                        object beforeChange = list[replacementIndex].operand;
        //                        list[replacementIndex].operand = MODIFIED_OPERANDS[k];
        //                        Main.LogInfo($"Line {replacementIndex}: Replaced operand ({beforeChange ?? "null"} ==> {MODIFIED_OPERANDS[k] ?? "null"})");
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    Main.LogWarning($"{(matches > 0 ? (matches == EXPECTED_MATCH_COUNT ? "Transpiler Patch succeeded with no errors" : $"Completed with {matches}/{EXPECTED_MATCH_COUNT} found.") : "Failed to find match")}");
        //    return list.AsEnumerable();
        //}
    }
}
