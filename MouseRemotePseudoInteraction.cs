using Controllers;
using Kitchen;
using KitchenData;
using KitchenDragNDropDesigner.Helpers;
using System;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;

namespace KitchenDragNDropDesigner
{
    internal class MouseRemotePseudoInteraction : RestaurantSystem
    {
        private struct SInteractionProxyMarker : IComponentData { }

        EntityQuery Players;

        bool wasPressed = false;

        MethodInfo HasSRerollTrigger;

        protected override void Initialise()
        {
            base.Initialise();
            Players = GetEntityQuery(typeof(CPlayer));

            Type sRerollTriggerType = typeof(CreateRerollTrigger).GetNestedType("SRerollTrigger", BindingFlags.NonPublic);
            MethodInfo hasMethod = typeof(GenericSystemBase).GetMethod("Has", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(Entity) }, null);
            HasSRerollTrigger = hasMethod.MakeGenericMethod(sRerollTriggerType);
        }

        protected override void OnUpdate()
        {
            bool performed = false;
            
            CPosition position = MouseHelpers.MousePlanePos();
            position.ForceSnap = false;
            Entity entity = GetOccupant(position, OccupancyLayer.Default);

            if (Has<SIsNightTime>() && (Has<CApplianceChair>(entity) || (bool)HasSRerollTrigger.Invoke(this, new object[] { entity })) && 
                MouseHelpers.IsMouseButtonPressed(Main.ActButtonPreference.Get()))
            {
                wasPressed = true;
                NativeArray<Entity> players = Players.ToEntityArray(Allocator.Temp);
                foreach(Entity player in players)
                {
                    if (Require<CPlayer>(player, out CPlayer cPlayer) && cPlayer.InputSource == InputSourceIdentifier.Identifier.Value)
                    {
                        Entity interactionProxy;
                        if (!TryGetSingletonEntity<SInteractionProxyMarker>(out interactionProxy))
                        {
                            interactionProxy = EntityManager.CreateEntity(typeof(SInteractionProxyMarker));
                            Set(interactionProxy, new CIsInteractor
                            {
                                InteractionOffset = 0f,
                                InteractionRadius = 0.7f,
                                Mode = InteractionMode.Appliances
                            });
                            Set<CDoNotPersist>(interactionProxy);
                        }
                        Set(interactionProxy, position);


                        Set(interactionProxy, new CAttemptingInteraction()
                        {
                            Target = entity,
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
                        performed = true;
                    }
                }
                players.Dispose();
            }


            if (!performed && wasPressed && TryGetSingletonEntity<SInteractionProxyMarker>(out Entity toDestroy))
            {
                wasPressed = false;
                EntityManager.DestroyEntity(toDestroy);
            }
        }
    }
}
