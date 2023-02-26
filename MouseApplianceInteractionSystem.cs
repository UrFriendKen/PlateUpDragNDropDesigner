using Controllers;
using Kitchen;
using KitchenData;
using KitchenDragNDropDesigner.Helpers;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KitchenDragNDropDesigner
{

    internal abstract class MouseApplianceInteractionSystem : ApplianceInteractionSystem
    {
        protected bool MadeChanges = default;
        protected CPosition Position;

        protected enum MouseButton
        {
            Left,
            Middle,
            Right
        }

        protected bool IsMouseButtonPressed
        {
            get
            {
                switch (Button)
                {
                    case MouseButton.Left:
                        return Mouse.current.leftButton.IsPressed();
                    case MouseButton.Middle:
                        return Mouse.current.middleButton.IsPressed();
                    case MouseButton.Right:
                        return Mouse.current.rightButton.IsPressed();
                    default:
                        return false;
                }
            }
        }

        protected bool IsMouseButtonWasDown { get; private set; }

        protected virtual bool AllowHold => false;

        protected virtual MouseButton Button => MouseButton.Left;

        protected override bool BeforeRun()
        {
            base.BeforeRun();
            if (!Has<SLayout>())
                return false;
            MadeChanges = false;
            return true;
        }

        protected override void AfterRun()
        {
            base.AfterRun();
            if (!MadeChanges)
                return;
            EntityManager.CreateEntity((ComponentType)typeof(CRequireStartDayWarningRecalculation));
            if (!HasSingleton<SPerformTableUpdate>())
                EntityManager.CreateEntity((ComponentType)typeof(SPerformTableUpdate));
        }

        protected override bool ShouldAct(ref InteractionData interaction_data)
        {
            if (Require<CPlayer>(interaction_data.Interactor, out CPlayer player) && MouseHelpers.IsKeyboardOrFirstLocalPlayer(player))
                UpdateInteractionData(ref interaction_data);
            return base.ShouldAct(ref interaction_data);
        }

        protected virtual void UpdateInteractionData(ref InteractionData interaction_data)
        {
            if (IsMouseButtonPressed && (AllowHold || !IsMouseButtonWasDown))
            {
                interaction_data.Attempt.Type = RequiredType;
                UpdateMouseTarget(ref interaction_data, OccupancyLayer.Default);
                IsMouseButtonWasDown = true;
            }
            else if (!IsMouseButtonPressed)
            {
                IsMouseButtonWasDown = false;
            }
        }

        protected Entity UpdateMouseTarget(ref InteractionData data, OccupancyLayer layer)
        {
            data.Attempt.Location = MouseHelpers.MousePlanePos();
            data.Attempt.Target = layer != OccupancyLayer.Default ? GetOccupant(data.Attempt.Location, layer) : GetPrimaryOccupant(data.Attempt.Location);
            return data.Attempt.Target;
        }
    }
}
