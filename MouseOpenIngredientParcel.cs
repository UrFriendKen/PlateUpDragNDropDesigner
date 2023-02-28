using Kitchen;
using KitchenData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace KitchenDragNDropDesigner
{
    [UpdateAfter(typeof(MousePickUpAndDropAppliance))]
    internal class MouseOpenIngredientParcel : MouseApplianceInteractionSystem
    {
        private CLetterIngredient Letter;
        protected override bool AllowActOrGrab => true;

        protected override MouseButton Button => Main.GrabButtonPreference.Get();

        protected override bool IsPossible(ref InteractionData data)
        {
            if (!Require<CLetterIngredient>(data.Target, out Letter))
            {
                return false;
            }
            if (!Require<CPosition>(data.Target, out Position))
            {
                return false;
            }
            return true;
        }

        protected override void Perform(ref InteractionData data)
        {
            data.Context.Destroy(data.Target);
            int iD = base.Data.ReferableObjects.DefaultProvider.ID;
            if (base.Data.TryGet<Item>(Letter.IngredientID, out var output, warn_if_fail: true))
            {
                Appliance dedicatedProvider = output.DedicatedProvider;
                iD = ((dedicatedProvider == null) ? base.Data.ReferableObjects.DefaultProvider.ID : dedicatedProvider.ID);
            }
            Entity entity = data.Context.CreateEntity();
            data.Context.Set(entity, new CCreateAppliance
            {
                ID = iD
            });
            data.Context.Set(entity, CItemProvider.InfiniteItemProvider(Letter.IngredientID));
            data.Context.Set(entity, new CPosition(Position));
        }
    }
}
