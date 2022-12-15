using System.Collections.Generic;
using Verse;

namespace CF
{
    /// <summary>
    /// The base <c>HediffProperties</c> used to provide instructions for
    /// <c>HediffComp_HealPermanentWoundsConfigurable</c>
    /// </summary>
    class HediffCompProperties_HealPermanentWoundsConfigurable :
        HediffCompProperties
    {
        /// <summary>
        /// A <c>List</c> of <c>Hediff</c>s that cannot be cured. Should be
        /// left blank if a whitelist is in use.
        /// </summary>
        public List<HediffDef> blacklist = null;
        /// <summary>
        /// A <c>List</c> of <c>Hediff</c>s. If this list is used, then only
        /// the <c>Hediff</c>s it contains will be used. Should be left blank
        /// if a whitelist is in use. It is generally recommended to blacklist
        /// <c>Hediff</c>s that can be caused by side-effects so that they
        /// aren't automatically cured.
        /// </summary>
        public List<HediffDef> whitelist = null;
        /// <summary>
        /// A <c>List</c> of <c>RegenSideEffect</c>s that will be applied to
        /// the <c>Pawn</c> whenever a <c>Hediff</c> is removed.
        /// </summary>
        public List<CommunityHealthUtility.RegenSideEffect> regenHediffs = null;
        /// <summary>
        /// If <c>true</c>, parts that are fully destroyed will still have
        /// their <c>Hediff</c>s cured.
        /// </summary>
        public bool canHealDestroyed = false;
        /// <summary>
        /// Determines whether all <c>Hediff</c>s of the <c>Hediff_Injury</c>
        /// class should automatically be treated as whitelisted or
        /// blacklisted, if at all.
        /// </summary>
        public CommunityHealthUtility.InjuryRegenListMode injuryRegenListMode =
            CommunityHealthUtility.InjuryRegenListMode.None;

        /// <summary>
        /// Shortest and longest times between regeneration attempts. Values
        /// are multiplied by <c>RegenIntervalTicks</c>. Default is based on
        /// the values for Luciferium.
        /// </summary>
        public IntRange regenInterval = new IntRange(15, 30);
        /// <summary>
        /// Number of ticks to multiply regenInterval by. Usually a distinct
        /// period of time, represented in ticks. For reference, one day is
        /// 60000 ticks long.
        /// </summary>
        public int regenIntervalTicks = 60000;

        /// <summary>
        /// The number of times that the regeneration method will be invoked
        /// while the parent <c>Hediff</c> is applied. If the value us -1,
        /// regeneration will be repeated indefinately.
        /// </summary>
        public int usesBeforeExhaustion = -1;
        /// <summary>
        /// If <c>true</c>, the parent <c>Hediff</c> will be removed when
        /// <c>usesBeforeExhaustion</c> reaches 0.
        /// </summary>
        public bool removeParentOnExhaustion = true;

        /// <summary>
        /// Boilerplate constructor, sets <c>compClass</c> to 
        /// <c>HediffComp_HealPermanentWoundsConfigurable</c>
        /// </summary>
        public HediffCompProperties_HealPermanentWoundsConfigurable() =>
            this.compClass =
            typeof(HediffComp_HealPermanentWoundsConfigurable);
    }
}
