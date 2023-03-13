using Kitchen;
using KitchenData;
using Unity.Entities;

namespace KitchenDragNDropDesigner
{
    [UpdateAfter(typeof(MousePickUpAndDropAppliance))]
    [UpdateAfter(typeof(MouseOpenLetter))]
    [UpdateAfter(typeof(MouseOpenIngredientParcel))]
    [UpdateAfter(typeof(MouseOpenApplianceParcels))]
    internal class MouseStartPracticeMode : MouseApplianceInteractionSystem
    {

        private bool ShouldPrompt;

        private EntityQuery CarriedAppliances;

        private EntityQuery Popups;

        protected override InteractionType RequiredType => InteractionType.Act;

        protected override MouseButton Button => Main.ActButtonPreference.Get();

        private EntityQuery StartDayWarnings;

        protected override void Initialise()
        {
            base.Initialise();
            StartDayWarnings = GetEntityQuery(ComponentType.ReadOnly<SStartDayWarnings>());
            CarriedAppliances = GetEntityQuery(typeof(CHeldAppliance));
            Popups = GetEntityQuery(typeof(StartPracticePopup.CRequest));
        }

        protected override bool BeforeRun()
        {
            ShouldPrompt = false;
            return Popups.IsEmpty;
        }

        protected override void AfterRun()
        {
            base.AfterRun();
            if (ShouldPrompt)
            {
                Set<CRequestPracticeMode>();
            }
        }

        protected override bool IsPossible(ref InteractionData data)
        {
            if (!HasComponent<CTriggerPracticeMode>(data.Target))
            {
                return false;
            }
            return true;
        }

        protected override void Perform(ref InteractionData data)
        {
            ShouldPrompt = true;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (Has<CRequestPracticeMode>())
            {
                PopupType type = ((StartDayWarnings.GetSingleton<SStartDayWarnings>().PostUnopened.IsBlocking() || !CarriedAppliances.IsEmpty) ? PopupType.PracticeBlockedByParcelOrHolding : PopupType.EnterPracticeMode);
                base.PopupUtilities.RequestManagedPopup(type);
            }
            Clear<CRequestPracticeMode>();
        }
    }
}
