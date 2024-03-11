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
    }
}
