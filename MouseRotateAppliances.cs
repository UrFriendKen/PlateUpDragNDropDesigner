using Kitchen;
using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace KitchenDragNDropDesigner
{
    internal class MouseRotateAppliances : MouseApplianceInteractionSystem
    {
        protected override CMouseData.Action Action => CMouseData.Action.Act;

        protected override InteractionType RequiredType => InteractionType.Act;

        protected override bool UseImmediateContext => true;

        protected override bool IsPossibleCondition(ref InteractionData data)
        {
            return Require<CPosition>(data.Attempt.Target, out Position) &&
                !Has<CMustHaveWall>(data.Attempt.Target) &&
                !Has<CFixedRotation>(data.Attempt.Target) &&
                Has<CAppliance>(data.Attempt.Target);
        }

        protected override void Perform(ref InteractionData data)
        {
            bool isMouseInteraction = IsMouseButtonPressed || IsSharedActionBinding;

            CAttemptingInteraction attempt = data.Attempt;
            bool flagChangesDone = PerformRotate(ref attempt, in Position, data.ShouldAct, isMouseInteraction);
            if (data.ShouldAct)
                MadeChanges |= flagChangesDone;
        }

        private bool PerformRotate(
          ref CAttemptingInteraction interact,
          in CPosition pos,
          bool should_act,
          bool isMouseInteraction)
        {
            bool flag = false;
            if (!CanReach((Vector3)pos, interact.Location) && !isMouseInteraction)
                return flag;

            Entity entity2 = interact.Target;

            if (!Require<CPosition>(entity2, out CPosition comp) || !comp.Position.IsSameTile(interact.Location))
                return false;

            if (HasComponent<CAppliance>(entity2) && !HasComponent<CHeldAppliance>(entity2) && !HasComponent<CImmovable>(entity2))
            {
                if (should_act)
                {
                    quaternion rotation = math.mul(quaternion.RotateY((float)Math.PI / 2f), Position.Rotation);
                    SetComponent(entity2, new CPosition
                    {
                        Position = Position.Position,
                        Rotation = rotation
                    });
                }
                flag = true;
                interact.Result = should_act ? InteractionResult.Performed : InteractionResult.Possible;
            }
            return flag;
        }

    }
}
