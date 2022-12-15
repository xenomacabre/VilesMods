using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace CF
{
    /// <summary>
    /// Sets the parent hediff's severity based on worn apparel.
    /// </summary>
    /// <remarks>
    /// If wornSeverity is unset, uses the hediff's initial severity.
    /// If apparelDefs is unset, checks if any worn apparel have a matching
    /// <see cref="CompCauseHediff_Apparel"/>.
    /// </remarks>
    class HediffComp_SeverityFromApparel : HediffComp
    {
        /// <summary>
        /// Pre-cast reference to this comp's
        /// <seealso cref="HediffCompProps_SeverityFromApparel"/>.
        /// </summary>
        public HediffCompProps_SeverityFromApparel Props =>
            (HediffCompProps_SeverityFromApparel)props;

        /// <summary>
        /// The severity to apply to the parent <c>Hediff</c> if any of the
        /// required apparel items are worn. Equal to
        /// <c>Props.wornSeverity</c>, or the parent <c>Hediff</c>'s
        /// <c>initialSeverity</c> if the former is <c>null</c>.
        /// </summary>
        public float WornSeverity =>
            Props.wornSeverity ?? parent.def.initialSeverity;

        /// <summary>
        /// Run each time the comp is ticked. Checks the affected <c>Pawn</c>'s
        /// apparel and updates the parent <c>Hediff</c>'s severity whenever
        /// <c>Props.tickInterval</c> ticks have passed.
        /// </summary>
        /// <param name="severityAdjustment">
        /// Unused in this override.
        /// </param>
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            // Only execute the remaining code if we're currently on a "cheap"
            // tick.
            if (!IsCheapIntervalTick)
                return;

            // Check each item worn by the affected Pawn, and determine if it
            // is one of the required apparel items.
            foreach (Apparel apparel in parent.pawn.apparel.WornApparel)
            {
                if (IsMatching(apparel))
                {
                    // If the item we're looking at is a match, set the parent
                    // Hediff's severity to WornSeverity and stop.
                    parent.Severity = WornSeverity;
                    return;
                }
            }
            // If no matches are found, set the parent Hediff's severity to
            // Props.unwornSeverity.
            // By default, sets it to 0, which removes the hediff.
            parent.Severity = Props.unwornSeverity;
        }

        /// <summary>
        /// Used internally to determine if a given apparel item should set the
        /// parent <c>Hediff</c>'s severity to <c>WornSeverity</c>.
        /// </summary>
        /// <param name="apparel">
        /// The <seealso cref="Apparel"/> item to check.
        /// </param>
        /// <returns>
        /// <c>true</c> if the apparel item's <c>Def</c> is in
        /// <c>Props.apparelDefs</c>, or if <c>Props.apperalDefs</c> is empty
        /// and the apparel item causes the parent <c>Hediff</c> when worn.
        /// </returns>
        public bool IsMatching(Apparel apparel)
        {
            if (Props.apparelDefs != null)
                return Props.apparelDefs.Contains(apparel.def);

            CompProperties_CauseHediff_Apparel props = 
                (CompProperties_CauseHediff_Apparel)apparel.
                TryGetComp<CompCauseHediff_Apparel>()?.props;
            return props is null ? false : props.hediff == parent.def;
        }

        #region cheap tick interval stuff
        /// <summary>
        /// Cached tick offset, calculated from the affected pawn's
        /// <c>HashOffset</c>.
        /// </summary>
        /// <remarks>
        /// Based on <seealso cref="CompWithCheapHashInterval"/>
        /// </remarks>
        private int hashOffset = 0;

        /// <summary>
        /// Used internally to determine if the current game tick is a "cheap"
        /// tick.
        /// </summary>
        /// <remarks>
        /// Based on <seealso cref="CompWithCheapHashInterval"/>
        /// </remarks>
        public bool IsCheapIntervalTick => 
            (Find.TickManager.TicksGame + hashOffset) % Props.tickInterval
            == 0;

        // todo: check whether this is called when loading a save
        /// <summary>
        /// Stores the affected pawn's <c>HashOffset</c> whenever this comp is
        /// initialized.
        /// </summary>
        /// <remarks>
        /// Based on <seealso cref="CompWithCheapHashInterval"/>
        /// </remarks>
        public override void CompPostMake() 
        {
            hashOffset = parent.pawn.thingIDNumber.HashOffset();
        }
        #endregion cheap tick interval stuff
    }
    /// <summary>
    /// <c>CompProperties</c> for <see cref="HediffComp_SeverityFromApparel"/>.
    /// </summary>
    class HediffCompProps_SeverityFromApparel : HediffCompProperties
    {
#pragma warning disable CS0649
        /// <summary>
        /// Severity applied when an item from <c>apparelDefs</c> is actively
        /// equipped. If <c>null</c>, then the parent <c>Hediff</c>'s 
        /// <c>initialSeverity</c> is used.
        /// </summary>
        public float? wornSeverity;
        /// <summary>
        /// Severity applied when no items from <c>apparelDefs</c> are actively
        /// equipped.
        /// </summary>
        public float unwornSeverity = 0f;
        /// <summary>
        /// A list of <c>ThingDef</c>s, ideally apparel items. The parent
        /// <c>Hediff</c>'s severity will be set to <c>wornSeverity</c> if the
        /// affected <c>Pawn</c> is wearing any of these.
        /// </summary>
        public List<ThingDef> apparelDefs = null;
        /// <summary>
        /// The affected <c>Pawn</c>'s apparel will be checked at an interval
        /// of this many ticks.
        /// </summary>
        public int tickInterval = 250;
#pragma warning restore CS0649

        /// <summary>
        /// No-arg constructor, automatically sets the <c>compClass</c> to
        /// <seealso cref="HediffComp_SeverityFromApparel"/>
        /// </summary>
        public HediffCompProps_SeverityFromApparel()
        {
            compClass = typeof(HediffComp_SeverityFromApparel);
        }
    }
}
