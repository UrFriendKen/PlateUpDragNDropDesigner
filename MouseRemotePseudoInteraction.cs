using Controllers;
using Kitchen;
using KitchenData;
using KitchenMods;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;

namespace KitchenDragNDropDesigner
{
    internal class MouseRemotePseudoInteraction : NightSystem, IModSystem
    {
        private struct CLinkedInteractionProxy : IComponentData, IModComponent
        {
            public Entity Proxy;
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct CInteractionProxyMarker : IComponentData, IModComponent { }

        EntityQuery Players;

        MethodInfo HasSRerollTrigger;

        protected override void Initialise()
        {
            base.Initialise();
            Players = GetEntityQuery(typeof(CPlayer), typeof(CMouseData));

            Type sRerollTriggerType = typeof(CreateRerollTrigger).GetNestedType("SRerollTrigger", BindingFlags.NonPublic);
            MethodInfo hasMethod = typeof(GenericSystemBase).GetMethod("Has", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(Entity) }, null);
            HasSRerollTrigger = hasMethod.MakeGenericMethod(sRerollTriggerType);
        }

        protected override void OnUpdate()
        {
            using NativeArray<Entity> entities = Players.ToEntityArray(Allocator.Temp);
            using NativeArray<CMouseData> mouseDatas = Players.ToComponentDataArray<CMouseData>(Allocator.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                Entity entity = entities[i];
                CMouseData mouseData = mouseDatas[i];
                CPosition position = mouseData.Position;

                Entity occupant = GetOccupant(mouseData.Position, OccupancyLayer.Default);
                bool hasLinkedInteractionProxy = Require(entity, out CLinkedInteractionProxy cLinkedInteractionProxy);

                ButtonState actButtonState = mouseData.GetButtonState(CMouseData.Action.Act);
                if (actButtonState != ButtonState.Held ||
                    (!Has<CApplianceChair>(occupant) &&
                    !(bool)HasSRerollTrigger.Invoke(this, new object[] { occupant })))
                {
                    if (hasLinkedInteractionProxy)
                    {
                        if (cLinkedInteractionProxy.Proxy != default)
                            EntityManager.DestroyEntity(cLinkedInteractionProxy.Proxy);
                        EntityManager.RemoveComponent<CLinkedInteractionProxy>(entity);
                    }
                    continue;
                }

                Entity interactionProxy;
                if (!hasLinkedInteractionProxy)
                {
                    interactionProxy = EntityManager.CreateEntity(typeof(CInteractionProxyMarker));
                    Set(interactionProxy, new CIsInteractor
                    {
                        InteractionOffset = 0f,
                        InteractionRadius = 0.7f,
                        Mode = InteractionMode.Appliances
                    });
                    Set<CDoNotPersist>(interactionProxy);

                    Set(entity, new CLinkedInteractionProxy()
                    {
                        Proxy = interactionProxy
                    });
                }
                else
                {
                    interactionProxy = cLinkedInteractionProxy.Proxy;
                }
                Set(interactionProxy, position);

                Set(interactionProxy, new CAttemptingInteraction()
                {
                    Target = occupant,
                    Type = InteractionType.Act,
                    Result = InteractionResult.Performed,
                    IsHeld = false,
                    Location = position,
                    Mode = InteractionMode.Appliances,
                });

                Set(interactionProxy, new CInputData
                {
                    State = new InputState
                    {
                        InteractAction = ButtonState.Held
                    }
                });
            }
        }
    }
}
