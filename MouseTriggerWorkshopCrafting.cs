using Kitchen;
using KitchenData;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenDragNDropDesigner
{
    [UpdateBefore(typeof(TriggerWorkshopCrafting))]
    public class MouseTriggerWorkshopCrafting : MouseItemInteractionSystem
    {
        private SWorkshopOutput Output;

        private Entity OutputPedestal;

        private EntityQuery Inputs;

        private EntityQuery Machines;

        private NativeArray<Entity> MachineArray;

        private int[] InputItems = new int[3];

        protected override InteractionType RequiredType => InteractionType.Act;

        protected override CMouseData.Action Action => CMouseData.Action.Act;

        protected override void Initialise()
        {
            base.Initialise();

            Inputs = GetEntityQuery(typeof(CItemHolder), typeof(CWorkshopInput));
            Machines = GetEntityQuery(typeof(CWorkshopMachine));

            RequireSingletonForUpdate<SFranchiseMarker>();
        }
        protected override bool BeforeRun()
        {
            base.BeforeRun();
            MachineArray = Machines.ToEntityArray(Allocator.TempJob);
            return true;
        }
        protected override void AfterRun()
        {
            base.AfterRun();
            MachineArray.Dispose();
        }

        protected override bool IsPossibleCondition(ref InteractionData data)
        {
            if (!Has<CWorkshopActivateButton>(data.Target))
            {
                return false;
            }
            if (!Require(out Output) || !Output.IsReady)
            {
                return false;
            }
            OutputPedestal = GetEntity<SWorkshopOutput>();
            if (!Require(OutputPedestal, out CItemHolder comp) || comp.HeldItem != default)
            {
                return false;
            }
            return true;
        }

        protected override void Perform(ref InteractionData data)
        {
            Entity entity = data.Context.CreateEntity();
            if (!base.Data.TryGet(Output.OutputAppliance, out Appliance applianceGDO))
            {
                return;
            }
            int iD = (applianceGDO.CrateItem == null) ? AssetReference.ApplianceCrate : applianceGDO.CrateItem.ID;
            data.Context.Set(entity, new CCreateItem
            {
                ID = iD,
                Holder = OutputPedestal
            });
            data.Context.Set(entity, new CUpgrade
            {
                ID = Output.OutputAppliance
            });
            using NativeArray<CItemHolder> nativeArray = Inputs.ToComponentDataArray<CItemHolder>(Allocator.Temp);
            using NativeArray<CWorkshopInput> nativeArray2 = Inputs.ToComponentDataArray<CWorkshopInput>(Allocator.Temp);
            for (int i = 0; i < nativeArray.Length; i++)
            {
                CItemHolder cItemHolder = nativeArray[i];
                int index = nativeArray2[i].Index;
                if (Require(cItemHolder.HeldItem, out CCrateAppliance comp))
                {
                    InputItems[index] = comp.Appliance;
                }
                else
                {
                    InputItems[index] = 0;
                }
                if (Has<CHeldBy>(cItemHolder.HeldItem))
                {
                    data.Context.Destroy(cItemHolder.HeldItem);
                }
            }
            Set(new SWorkshopOutput
            {
                IsReady = false,
                OutputAppliance = 0,
                Nonce = Random.Range(int.MinValue, int.MaxValue)
            });
            foreach (Entity item in MachineArray)
            {
                base.EntityManager.SetComponentData(item, new CWorkshopMachine
                {
                    Item1 = InputItems[0],
                    Item2 = InputItems[1],
                    Item3 = InputItems[2],
                    Nonce = Random.Range(int.MinValue, int.MaxValue)
                });
            }
            Entity entity2 = data.Context.CreateEntity();
            data.Context.Set(entity2, new CRequestSave
            {
                SaveType = SaveType.Auto
            });
        }
    }
}
