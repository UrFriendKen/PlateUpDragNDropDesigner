using Kitchen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace KitchenDragNDropDesigner
{
    [UpdateAfter(typeof(MousePickUpAndDropAppliance))]
    internal class MouseOpenLetter : MouseApplianceInteractionSystem
    {
        private CLetterBlueprint Letter;

        protected override bool IsPossible(ref InteractionData data)
        {
            return Require<CLetterBlueprint>(data.Target, out Letter) &&
                Require<CPosition>(data.Target, out Position);
        }

        protected override void Perform(ref InteractionData data)
        {
            PostHelpers.OpenBlueprintLetter(data.Context, data.Target);
            data.Context.Destroy(data.Target);
        }
    }
}
