using Kitchen;
using System;
using Unity.Entities;

namespace KitchenDragNDropDesigner
{
    internal class MouseRotateChairs : MouseApplianceInteractionSystem
    {
        private CApplianceGhostChair Ghost;

        private DynamicBuffer<CGhostChairTableCandidates> Candidates;

        protected override InteractionType RequiredType => InteractionType.Grab;

        protected override MouseButton Button => Main.GrabButton;

        protected override bool IsPossible(ref InteractionData data)
        {
            return Require<CApplianceGhostChair>(data.Attempt.Target, out Ghost) &&
                Require<CPosition>(data.Attempt.Target, out Position) &&
                RequireBuffer(data.Attempt.Target, out Candidates) &&
                !(Require<CItemHolder>(data.Interactor, out CItemHolder comp) && comp.HeldItem != default(Entity)) &&
                Candidates.Length > 1;
        }

        protected override void Perform(ref InteractionData data)
        {
            for (int i = 0; i < Candidates.Length; i++)
            {
                if (Candidates[i].Table == Ghost.Table)
                {
                    CGhostChairTableCandidates cGhostChairTableCandidates = Candidates[(i + 1 + Candidates.Length) % Candidates.Length];
                    Position.Rotation = cGhostChairTableCandidates.Rotation;
                    Ghost.Table = cGhostChairTableCandidates.Table;
                    Set(data.Attempt.Target, Ghost);
                    Set(data.Attempt.Target, Position);
                    break;
                }
            }
        }
    }
}
