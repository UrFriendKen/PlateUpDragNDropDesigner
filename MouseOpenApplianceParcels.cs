﻿using Kitchen;
using Unity.Entities;

namespace KitchenDragNDropDesigner
{
    [UpdateAfter(typeof(MousePickUpAndDropAppliance))]
    internal class MouseOpenApplianceParcels : MouseApplianceInteractionSystem
    {
        private CLetterAppliance Letter;
        protected override CMouseData.Action Action => CMouseData.Action.Grab;

        protected override bool IsPossibleCondition(ref InteractionData data)
        {
            return Require<CLetterAppliance>(data.Target, out Letter) && Require<CPosition>(data.Target, out Position);
        }

        protected override void Perform(ref InteractionData data)
        {
            data.Context.Destroy(data.Target);
            Entity entity = data.Context.CreateEntity();
            data.Context.Set(entity, new CCreateAppliance
            {
                ID = Letter.ApplianceID
            });
            data.Context.Set(entity, new CPosition(Position));
        }
    }
}
