using System;
using Verse;

namespace CF
{
    /// <summary>
    /// The <c>HediffComp</c> responsible for managing regeneration, based on
    /// the member values defined in
    /// <c>HediffCompProperties_HealPermanentWoundsConfigurable</c>
    /// </summary>
    class HediffComp_HealPermanentWoundsConfigurable : HediffComp
    {
        /// <summary>
        /// Ticks before the next regeneration attempt is executed
        /// </summary>
        private int ticksToHeal;
        /// <summary>
        /// The number of additional times that TryHealPermanentWound will be
        /// invoked. -1 means infinite uses.
        /// </summary>
        private int remainingUses = -1;

        /// <summary>
        /// Getter used to directly access this comp's
        /// <c>HediffCompProperties_HealPermanentWoundsConfigurable</c> so that
        /// a cast isn't required every time it is referenced.
        /// </summary>
        HediffCompProperties_HealPermanentWoundsConfigurable Props =>
            (HediffCompProperties_HealPermanentWoundsConfigurable)props;

        /// <summary>
        /// Internal helper method, resets the cooldown interval
        /// </summary>
        private void ResetTicksToHeal()
        {
            Random random = new Random();
            ticksToHeal =
                random.Next(Props.regenInterval.min,
                    Props.regenInterval.max + 1) *
                Props.regenIntervalTicks;
        }

        /// <summary>
        /// Method run after the internal timer runs out, simply invokes
        /// <c>CommunityHealthUtility.TryHealRandomPermanentWoundFor</c>
        /// using values defined by
        /// <c>HediffCompProperties_HealPermanentWoundsConfigurable</c>.
        /// </summary>
        private void TryHealPermanentWound()
        {
            CommunityHealthUtility.TryHealRandomPermanentWoundFor(
                Pawn,
                parent,
                Props.blacklist,
                Props.whitelist,
                Props.regenHediffs,
                Props.canHealDestroyed,
                Props.injuryRegenListMode
                );
        }

        /// <summary>
        /// Run when this <c>HediffComp</c> (and by extention its parent
        /// <c>Hediff</c>) is created. Sets up the internal timer for the first
        /// time and sets the intended number of uses.
        /// </summary>
        public override void CompPostMake()
        {
            base.CompPostMake();
            ResetTicksToHeal();
            remainingUses = Props.usesBeforeExhaustion;
        }

        /// <summary>
        /// Runs every tick, decreases the internal timer by 1. If the timer
        /// reaches 0, <c>TryHealPermanentWound</c> is invoked, the timer is
        /// reset, and the remaining uses is decreased by 1 if necessary. If
        /// the number of reamining uses is 0 and
        /// <c>removeParentOnExhaustion</c> is <c>true</c>, then the parent
        /// <c>Hediff</c> will be removed.
        /// </summary>
        /// <param name="severityAdjustment">
        /// Parameter from the <c>base</c> method, currently unused.
        /// </param>
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (remainingUses == 0)
                return;
            --ticksToHeal;
            if (ticksToHeal > 0)
                return;
            TryHealPermanentWound();
            ResetTicksToHeal();
            if (remainingUses != -1)
            {
                //Prevents exhaustible comps from ever dropping below 0,
                //because that might allow players to obtain an infinite
                //regeneration bug.
                remainingUses = Math.Max(0, remainingUses - 1);
            }
        }

        /// <summary>
        /// <c>override</c> of the saving/loading method, used to store the
        /// current number of ticks remaining in the timer, and the current
        /// number of uses remaining.
        /// </summary>
        public override void CompExposeData()
        {
            Scribe_Values.Look<int>(ref ticksToHeal, "ticksToHeal");
            Scribe_Values.Look<int>(ref remainingUses, "remainingUses");
        }

        /// <summary>
        /// <c>override</c> of the method used to display additional debug
        /// information in the health pane.
        /// </summary>
        /// <returns>
        /// A <c>string</c> containing the values of member variables for
        /// debugging purposes.
        /// </returns>
        public override string CompDebugString()
        {
            return "ticksToHeal: " + ticksToHeal +
                "\nremainingUses: " + remainingUses;
        }

        /// <summary>
        /// <c>override</c> of method used to determine if a
        /// <c>HediffComp</c>'s parent should be removed when available. Parent
        /// will be removed if remaining uses are exhausted and
        /// <c>removeParentOnExhaustion</c> is <c>true</c>, or if the base
        /// method returns <c>true</c>.
        /// </summary>
        public override bool CompShouldRemove => base.CompShouldRemove ||
            (remainingUses == 0 && Props.removeParentOnExhaustion);

        /// <summary>
        /// <c>override</c> of method called when an identical <c>Hediff</c>
        /// is applied to the same part. Re-applying the regeneration
        /// <c>Hediff</c> will add to the remaining number of uses, unless
        /// the current number of uses in infinite.
        /// </summary>
        /// <param name="other"></param>
        public override void CompPostMerged(Hediff other)
        {
            base.CompPostMerged(other);
            HediffComp_HealPermanentWoundsConfigurable otherComp =
                other.TryGetComp<HediffComp_HealPermanentWoundsConfigurable>();
            if (otherComp == null)
                return;
            if (remainingUses >= 0)
                remainingUses += otherComp.remainingUses;
        }
    }
}
