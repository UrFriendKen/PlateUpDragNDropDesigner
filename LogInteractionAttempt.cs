using Kitchen;
using KitchenMods;
using System;
using Unity.Collections;
using Unity.Entities;

namespace KitchenDragNDropDesigner
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public class LogInteractionAttempt : GameSystemBase, IModSystem
    {
        EntityQuery Attempts;

        protected override void Initialise()
        {
            base.Initialise();
            Attempts = GetEntityQuery(typeof(CAttemptingInteraction));
        }

        protected override void OnUpdate()
        {
            using NativeArray<CAttemptingInteraction> attempts = Attempts.ToComponentDataArray<CAttemptingInteraction>(Allocator.Temp);
            for (int i = 0; i < attempts.Length; i++)
            {
                if (attempts[i].PerformedBy != 0)
                    Main.LogInfo(SystemReference.GetName(attempts[i].PerformedBy));
            }
        }
    }
}
