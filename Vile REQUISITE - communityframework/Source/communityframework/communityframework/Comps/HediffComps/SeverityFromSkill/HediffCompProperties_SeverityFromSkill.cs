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
    /// The properties that determine the behavior of
    /// <c>HediffComp_SeverityFromSkill</c>.
    /// </summary>
    class HediffCompProperties_SeverityFromSkill : HediffCompProperties
    {
        /// <summary>
        /// A list of skills that will affect the severity of the parent
        /// <c>Hediff</c>.
        /// </summary>
        public List<SkillDef> skills = new List<SkillDef>();
        /// <summary>
        /// The intended maximum severity, reached when the pawn acquires level
        /// 20 in all relevant skills.
        /// </summary>
        public float targetSeverity = 1.0f;
        /// <summary>
        /// If the average of relevant skills is below this value, then the
        /// parent <c>Hediff</c>'s severity will always be 0.
        /// </summary>
        public int minSkill = 0;

        /// <summary>
        /// Constructor that automatically attaches the required CompClass to
        /// these properties.
        /// </summary>
        public HediffCompProperties_SeverityFromSkill() =>
            compClass = typeof(HediffComp_SeverityFromSkill);
    }
}
