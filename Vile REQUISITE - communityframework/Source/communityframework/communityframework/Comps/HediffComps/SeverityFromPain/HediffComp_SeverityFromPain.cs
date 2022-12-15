using System;
using Verse;
using RimWorld;

namespace CF
{
    /// <summary>
    /// A <c>HediffComp</c> that directly sets the parent
    /// <c>Hediff.Severity</c> to match the <c>Pawn</c>'s current pain level.
    /// </summary>
    class HediffComp_SeverityFromPain : HediffComp
    {
        /// <summary>
        /// Pre-cast reference to this comp's corresponding properties,
        /// <c>HediffCompProperties_SeverityFromPain</c>.
        /// </summary>
        HediffCompProperties_SeverityFromPain Props =>
            (HediffCompProperties_SeverityFromPain)props;

        /// <summary>
        /// An internal function used to calculate the intended severity
        /// setting, accounting for <c>Props.usePainThreshold</c>.
        /// </summary>
        /// <returns>The intended severity setting.</returns>
        private float GetPainLevelInt()
        {
            float num = CommunityHealthUtility.
                CalculatePawnIntendedPain(Pawn, false);
            if (Props.usePainThreshold)
                num /= Pawn.GetStatValue(StatDefOf.PainShockThreshold);
            return num;
        }

        /// <summary>
        /// Called per-tick, updates the parent's severity level to the
        /// intended amount.
        /// </summary>
        /// <param name="severityAdjustment">
        /// Used by <c>HediffComp</c>s to adjust the parent's severity
        /// incrementally, unused in this case.
        /// </param>
        public override void CompPostTick(ref float severityAdjustment)
        {
            parent.Severity = GetPainLevelInt();
        }

        /// <summary>
        /// Used to display additional information in the parent's in-game
        /// tooltip. In this case, it displays the pawn's current pain level.
        /// </summary>
        public override string CompTipStringExtra =>
            //English: "Current pain amount:"
            "CF_SeverityFromPain_CurrentPain".Translate() + "\n" +
            //Display pain level as a percent.
            Math.Round(GetPainLevelInt() * 100).ToString() + "%";
    }
}
