using Kitchen;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace KitchenDragNDropDesigner
{
    [UpdateBefore(typeof(MousePingedApplianceInfo))]
    internal class MouseInteractRotatePushDuringNight : MouseApplianceInteractionSystem
    {
        private CConveyPushRotatable Rotatable;
        protected override MouseButton Button => Main.MiscellaneousButtonPreference.Get();
        protected override InteractionType RequiredType => InteractionType.Notify;

        protected override bool IsPossible(ref InteractionData data)
        {
            if (!Require(data.Target, out Rotatable))
            {
                return false;
            }

            if (Rotatable.Target == Orientation.Null)
            {
                return false;
            }
            return true;
        }

        protected override void Perform(ref InteractionData data)
        {
            Rotatable.Target = Rotatable.Target.RotateCW();
            if (Rotatable.Target == Orientation.Down)
            {
                Rotatable.Target = Orientation.Left;
            }
            Set(data.Target, Rotatable);
        }
    }
}
