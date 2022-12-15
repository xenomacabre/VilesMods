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
    /// The <c>HediffProperties</c> used to provide instructions for
    /// <c>HediffComp_MultiplyOtherHediffDuration</c>. The <c>compClass</c>
    /// must be manually set to either
    /// <c>HediffComp_MultiplyOtherHediffDuration</c> or
    /// <c>HediffComp_MultiplyOtherHediffSeverity</c>.
    /// </summary>
    class HediffCompProperties_MultiplyOtherHediff :
        HediffCompProperties
    {
        /// <summary>
        /// A list of <c>HediffDef</c>s. If a sibling <c>Hediff</c> is added
        /// with one of these <c>Def</c>s, then it will have its
        /// <c>HediffComp_Disappears.ticksToDisappear</c> multiplied by
        /// <c>multiplier</c>.
        /// </summary>
        public List<HediffDef> affectedHediffs = new List<HediffDef>();
        /// <summary>
        /// The value that affected <c>Hediff</c>s will have their durations
        /// multiplied by.
        /// </summary>
        public float multiplier = 1.0f;
    }
}
