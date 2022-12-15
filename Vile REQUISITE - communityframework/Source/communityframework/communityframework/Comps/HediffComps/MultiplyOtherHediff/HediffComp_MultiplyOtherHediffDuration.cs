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
    /// When a sibling <c>Hediff</c> is applied, this comp with multiply the
    /// sibling's <c>HediffComp_Disappears.ticksToDisappear</c> by a set
    /// amount. Can be used to artificially increase or decrease the lengths of
    /// certain <c>Hediff</c>s.
    /// </summary>
    class HediffComp_MultiplyOtherHediffDuration :
        HediffComp, IHediffComp_OnHediffAdded
    {
        /// <summary>
        /// Pre-cast reference to this comp's corresponding properties,
        /// <c>HediffCompProperties_MultiplyOtherHediff</c>.
        /// </summary>
        public HediffCompProperties_MultiplyOtherHediff Props =>
            (HediffCompProperties_MultiplyOtherHediff)props;

        /// <summary>
        /// Run when another <c>Hediff</c> is added to the parent's
        /// <c>Pawn</c>. Multiplies the other <c>Hediff</c>'s duration by a set
        /// amount, if the other <c>Hediff</c>'s <c>Def</c> is in
        /// <c>Props.affectedHediffs</c>.
        /// </summary>
        /// <param name="hediff">The <c>Hediff</c> being added.</param>
        public void OnHediffAdded(ref Hediff hediff)
        {
            if (hediff == parent)
                return;
            if (Props.affectedHediffs.Contains(hediff.def))
                if (hediff is HediffWithComps withComps)
                {
                    HediffComp_Disappears comp =
                        withComps.TryGetComp<HediffComp_Disappears>();
                    if (comp != null)
                        comp.ticksToDisappear =
                            (int)Math.Ceiling(
                                comp.ticksToDisappear * Props.multiplier);
                }
        }
    }
}
