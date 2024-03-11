using Controllers;
using Kitchen;
using KitchenData;
using Unity.Entities;

namespace KitchenDragNDropDesigner
{
    public abstract class MouseApplianceInteractionSystem : ApplianceInteractionSystem
    {
        protected bool MadeChanges = default;
        protected CPosition Position;

        public enum MouseButton
        {
            None,
            Left,
            Right,
            Middle,
            Forward,
            Back
        }

        protected bool WasMouseButtonDown { get; private set; }

        protected virtual bool AllowHold => false;

        protected virtual CMouseData.Action Action { get; } = CMouseData.Action.Act;

        CMouseData _mouseData;
        protected bool IsMouseActive => _mouseData.Active;
        protected bool IsMouseButtonPressed => _mouseData.GetButtonState(Action) == ButtonState.Pressed || IsMouseButtonHeld;
        protected bool IsMouseButtonHeld => _mouseData.GetButtonState(Action) == ButtonState.Held;
        protected bool IsMouseButtonActive => IsMouseButtonPressed && (AllowHold || !IsMouseButtonHeld);
        protected bool IsSharedActionBinding => _mouseData.ShouldOverridePosition(Action);

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

        protected abstract bool IsPossibleCondition(ref InteractionData data);

        protected sealed override bool IsPossible(ref InteractionData data)
        {
            //if (!Require(data.Interactor, out CMouseData mouseData) ||
            //    !mouseData.Active)
            //    return false;

            return IsPossibleCondition(ref data);
        }

        protected override bool ShouldAct(ref InteractionData interaction_data)
        {
            _mouseData = default;
            if (Has<CPlayer>(interaction_data.Interactor) &&
                Require(interaction_data.Interactor, out CMouseData mouseData) &&
                mouseData.Active)
            {
                _mouseData = mouseData;
            }
            UpdateInteractionData(ref interaction_data);
            return base.ShouldAct(ref interaction_data);
        }

        protected virtual void UpdateInteractionData(ref InteractionData interaction_data)
        {
            if (!IsMouseActive)
                return;

            if (IsMouseButtonActive)
            {
                interaction_data.Attempt.Type = RequiredType;
                UpdateMouseTarget(ref interaction_data, OccupancyLayer.Default);
            }

            if (RequiredType == InteractionType.Look || IsMouseButtonActive || (interaction_data.Attempt.Type != InteractionType.Look && IsSharedActionBinding))
            {
                interaction_data.Attempt.Location = _mouseData.Position;
                UpdateMouseTarget(ref interaction_data, OccupancyLayer.Default);
            }
        }

        Entity UpdateMouseTarget(ref InteractionData data, OccupancyLayer layer)
        {
            Entity newTarget = layer != OccupancyLayer.Default ? GetOccupant(data.Attempt.Location, layer) : GetPrimaryOccupant(data.Attempt.Location);
            if (newTarget != default && Has<CIsInteractive>(newTarget))
            {
                data.Attempt.Target = newTarget;
            }
            return data.Attempt.Target;
        }
    }
}
