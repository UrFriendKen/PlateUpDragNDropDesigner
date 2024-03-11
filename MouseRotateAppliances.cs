using Kitchen;
using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace KitchenDragNDropDesigner
{
    [UpdateBefore(typeof(RotateAppliances))]
    internal class MouseRotateAppliances : MouseApplianceInteractionSystem
    {
        protected override CMouseData.Action Action => CMouseData.Action.Act;

        protected override InteractionType RequiredType => InteractionType.Act;

        protected override bool IsPossibleCondition(ref InteractionData data)
        {
            return Require<CPosition>(data.Attempt.Target, out Position) &&
                !Has<CMustHaveWall>(data.Attempt.Target) &&
                !Has<CFixedRotation>(data.Attempt.Target) &&
                Has<CAppliance>(data.Attempt.Target);
        }

        protected override void Perform(ref InteractionData data)
        {
            quaternion rotation = math.mul(quaternion.RotateY((float)Math.PI / 2f), Position.Rotation);
            SetComponent(data.Target, new CPosition
            {
                Position = Position.Position,
                Rotation = rotation
            });
        }
    }
}
