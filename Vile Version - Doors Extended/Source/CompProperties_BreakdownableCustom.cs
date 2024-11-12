using RimWorld;

namespace DoorsExpanded
{
    public class CompProperties_BreakdownableCustom : CompProperties_Breakdownable
    {
        public float breakdownMTBUnit = 1f;

        public CompProperties_BreakdownableCustom()
        {
            // We can't have a hypothetical CompBreakdownableCustom subclass that overrides CompBreakdownable.CheckForBreakdown,
            // since that method is non-virtual and thus non-overrideable. Instead, the CompBreakdownableCheckForBreakdownTranspiler
            // harmony patch effectively does this overriding.
            compClass = typeof(CompBreakdownable);
        }
    }
}
