﻿using Kitchen;
using KitchenData;
using KitchenDragNDropDesigner.Helpers;
using KitchenDragNDropDesigner.Patches;
using Unity.Entities;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace KitchenDragNDropDesigner
{
    [UpdateBefore(typeof(PickUpAndDropAppliance))]
    internal class MousePickUpAndDropAppliance : MouseApplianceInteractionSystem
    {
        private CItemHolder Holder;

        protected override InteractionType RequiredType => InteractionType.Grab;

        protected override bool UseImmediateContext => true;

        protected override MouseButton Button => Main.GrabButton;

        protected override bool IsPossible(ref InteractionData data) => Require<CPosition>(data.Interactor, out Position) && Require<CItemHolder>(data.Interactor, out Holder) && Perform(ref data, false);

        protected override void Perform(ref InteractionData data)
        {
            Perform(ref data, data.ShouldAct);
        }

        protected bool Perform(ref InteractionData data, bool should_act)
        {
            bool isMouseInteraction = IsMouseButtonPressed;

            CAttemptingInteraction attempt = data.Attempt;
            bool flagChangesDone = Holder.HeldItem == default ? PerformPickUp(data.Context, data.Interactor, ref attempt, in Position, should_act, OccupancyLayer.Default, isMouseInteraction) : PerformDrop(data.Context, data.Interactor, ref attempt, Holder, Position, should_act, isMouseInteraction);
            if (should_act)
                MadeChanges |= flagChangesDone;
            return flagChangesDone;
        }

        private bool PerformDrop(
          EntityContext ctx,
          Entity player,
          ref CAttemptingInteraction interact,
          CItemHolder player_holder,
          CPosition pos,
          bool should_act,
          bool isMouseInteraction)
        {
            EntityManager entityManager = EntityManager;

            Bounds includeOutside = Bounds;
            includeOutside.Encapsulate(GetFrontDoor() + new Vector3(0f, 0f, -3f));
            includeOutside.Expand(0.5f);

            if ((!CanReach((Vector3)pos, interact.Location) && !isMouseInteraction) || GetFrontDoor().IsSameTile(interact.Location) || GetFrontDoor(true).IsSameTile(interact.Location) || !includeOutside.Contains(interact.Location))
                return false;
            CLayoutRoomTile tile = GetTile(interact.Location);
            Vector3 position = interact.Location.Rounded();
            Quaternion quaternion = Quaternion.identity;
            Entity heldItem = player_holder.HeldItem;
            CAppliance component;
            if (!entityManager.RequireComponent<CAppliance>(heldItem, out component))
                return false;
            OccupancyLayer layer = component.Layer;
            bool flag2 = false;
            foreach (Orientation preferredRotation in OrientationHelpers.PreferredRotations)
            {
                Vector3 offset = preferredRotation.ToOffset();
                if (GetRoom(position + offset) != tile.RoomID)
                {
                    flag2 = !tile.CanReach(preferredRotation);
                    quaternion = (Quaternion)preferredRotation.ToRotation();
                    break;
                }
            }
            bool flag3 = tile.RoomID == 0;
            if (flag3 && HasComponent<CUnsellableAppliance>(heldItem))
                return false;
            if (!flag3 && HasComponent<CMustHaveWall>(heldItem) && !flag2)
                return false;
            bool flag4 = false;
            Entity occupant1 = GetOccupant(position, layer);
            if (occupant1 != new Entity() && !HasComponent<CAllowPlacingOver>(occupant1))
            {
                if (!PerformPickUp(ctx, player, ref interact, in pos, false, layer, isMouseInteraction))
                    return false;
                flag4 = true;
            }
            if (layer == OccupancyLayer.Floor && !flag4)
            {
                Entity occupant2 = GetOccupant(position);
                if (occupant2 != new Entity() && !HasComponent<CAllowPlacingOver>(occupant2) && !PerformPickUp(ctx, player, ref interact, in pos, false, OccupancyLayer.Default, isMouseInteraction))
                    return false;
                flag4 = true;
            }
            bool flag5 = true;
            if (should_act)
            {
                CPosition data = CPosition.Rounded(position);
                data.Rotation = (Quaternion)quaternion;
                ctx.Remove<CHeldAppliance>(heldItem);
                ctx.Remove<CHeldBy>(heldItem);
                ctx.Add<CRemoveView>(heldItem);
                ctx.Set<CRequiresView>(heldItem, new CRequiresView()
                {
                    Type = ViewType.Appliance
                });
                ctx.Set<CPosition>(heldItem, data);
                if (flag4)
                    PerformPickUp(ctx, player, ref interact, in pos, true, layer, isMouseInteraction);
                else
                    ctx.Set<CItemHolder>(player, new CItemHolder());
                SetOccupant(position, heldItem);

                if (Require<CPlayer>(player, out CPlayer cPlayer) && MouseHelpers.IsKeyboardOrFirstLocalPlayer(cPlayer))
                    ManageApplianceGhostsOriginalLambdaBodyPatch.isPickedUpByMouse = flag4 && isMouseInteraction;
            }
            return flag5;
        }

        private bool PerformPickUp(
          EntityContext ctx,
          Entity player,
          ref CAttemptingInteraction interact,
          in CPosition pos,
          bool should_act,
          OccupancyLayer layer,
          bool isMouseInteraction)
        {
            EntityManager entityManager = EntityManager;
            bool performed = false;
            Entity occupant = default;
            if (!CanReach((Vector3)pos, interact.Location) && !isMouseInteraction)
                return performed;
            occupant = ((layer != 0) ? GetOccupant(interact.Location, layer) : GetPrimaryOccupant(interact.Location));
            
            if (occupant == default)
            {
                occupant = interact.Target;
                if (!Require(occupant, out CPosition occupantPosition) || !occupantPosition.Position.IsSameTile(interact.Location))
                    return false;
            }

            if (entityManager.RequireComponent(occupant, out CAppliance appliance) && !HasComponent<CHeldAppliance>(occupant) && !HasComponent<CImmovable>(occupant))
            {
                if (should_act)
                {
                    ctx.Add<CHeldAppliance>(occupant);
                    ctx.Add<CHeldBy>(occupant);
                    ctx.Add<CRemoveView>(occupant);
                    ctx.Set(occupant, new CRequiresView()
                    {
                        Type = ViewType.HeldAppliance
                    });
                    ctx.Set(occupant, new CHeldBy()
                    {
                        Holder = player
                    });
                    ctx.Set(occupant, CPosition.Hidden);
                    ctx.Set(player, new CItemHolder()
                    {
                        HeldItem = occupant
                    });
                    SetOccupant(interact.Location, new Entity(), appliance.Layer);

                    if (Require(player, out CPlayer cPlayer) && MouseHelpers.IsKeyboardOrFirstLocalPlayer(cPlayer))
                        ManageApplianceGhostsOriginalLambdaBodyPatch.isPickedUpByMouse = isMouseInteraction;
                }
                performed = true;
                interact.Result = should_act ? InteractionResult.Performed : InteractionResult.Possible;
            }
            return performed;
        }

        protected override void OnCreateForCompiler() => base.OnCreateForCompiler();
    }
}
