using Kitchen;
using Kitchen.Layouts;
using Unity.Collections;
using Unity.Entities;

namespace KitchenDragNDropDesigner
{
    [UpdateInGroup(typeof(HighPriorityInteractionGroup))]
    [UpdateBefore(typeof(ApplyDecorationInteraction))]
    internal class MouseApplyDecorationInteraction : MouseApplianceInteractionSystem
    {
        private EntityQuery DecorChanges;

        private CItemHolder ItemHolder;

        private CApplyDecor ApplyDecor;

        private CLayoutRoomTile Tile;

        private NativeArray<CChangeDecorEvent> ChangeDecorEvents;

        private NativeArray<Entity> ChangeDecorEntities;

        protected override CMouseData.Action Action => CMouseData.Action.Act;

        protected override void Initialise()
        {
            base.Initialise();
            DecorChanges = GetEntityQuery(typeof(CChangeDecorEvent));
        }

        protected override bool BeforeRun()
        {
            ChangeDecorEvents = DecorChanges.ToComponentDataArray<CChangeDecorEvent>(Allocator.Temp);
            ChangeDecorEntities = DecorChanges.ToEntityArray(Allocator.Temp);
            return base.BeforeRun();
        }

        protected override void AfterRun()
        {
            ChangeDecorEvents.Dispose();
            ChangeDecorEntities.Dispose();
            base.AfterRun();
        }

        protected override bool IsPossibleCondition(ref InteractionData data)
        {
            if (!Require<CItemHolder>(data.Interactor, out ItemHolder) || ItemHolder.HeldItem == default(Entity))
            {
                return false;
            }
            if (!Require<CApplyDecor>(ItemHolder.HeldItem, out ApplyDecor))
            {
                return false;
            }
            Tile = GetTile(data.Attempt.Location);
            if (!LayoutHelpers.IsInside(Tile.Type))
            {
                return false;
            }
            return true;
        }

        protected override void Perform(ref InteractionData data)
        {
            CApplyDecor applyDecor = ApplyDecor;
            int num = 0;
            for (int i = 0; i < ChangeDecorEvents.Length; i++)
            {
                CChangeDecorEvent cChangeDecorEvent = ChangeDecorEvents[i];
                Entity entity = ChangeDecorEntities[i];
                if (cChangeDecorEvent.RoomID == Tile.RoomID && cChangeDecorEvent.Type == applyDecor.Type)
                {
                    num = cChangeDecorEvent.DecorID;
                    data.Context.Destroy(entity);
                }
            }
            Entity entity2 = data.Context.CreateEntity();
            data.Context.Set(entity2, new CChangeDecorEvent
            {
                RoomID = Tile.RoomID,
                DecorID = applyDecor.ID,
                Type = applyDecor.Type
            });
            if (num == 0)
            {
                data.Context.Destroy(ItemHolder.HeldItem);
                data.Context.Set(data.Interactor, default(CItemHolder));
                return;
            }
            data.Context.Set(ItemHolder.HeldItem, new CApplyDecor
            {
                Type = applyDecor.Type,
                ID = num
            });
            data.Context.Set(ItemHolder.HeldItem, new CDrawApplianceUsing
            {
                DrawApplianceID = num
            });
            data.Context.Remove<CLinkedView>(ItemHolder.HeldItem);
        }
    }
}
