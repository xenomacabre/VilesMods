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
    /// A <c>HediffComp</c> that adjusts its parent's severity by the skill
    /// levels of the pawn it is applied to. Most likely incompatible with
    /// any other severity-adjusting <c>HediffComp</c>s.
    /// </summary>
    class HediffComp_SeverityFromSkill : HediffComp
    {
        /// <summary>
        /// Pre-cast reference to the comp's properties.
        /// </summary>
        public HediffCompProperties_SeverityFromSkill Props =>
           (HediffCompProperties_SeverityFromSkill)props;

        /// <summary>
        /// The average of <c>parent.pawn</c>'s relevant skills, as defined in
        /// <c>Props.skills</c>.
        /// </summary>
        public float SkillAverage
        {
            get
            {
                int totalSkills = 0;
                foreach (SkillDef s in Props.skills)
                    totalSkills += parent.pawn.skills.GetSkill(s).Level;
                return totalSkills / Props.skills.Count();
            }
        }

        /// <summary>
        /// Returns a string containing a multi-line, bulleted list of skills
        /// that affect severity.
        /// </summary>
        private string SkillListFormattedInt
        {
            get
            {
                string final = string.Empty;
                foreach (SkillDef s in Props.skills)
                {
                    final = final + "-" + s.LabelCap + "\n";
                }
                return final;
            }
        }

        /// <summary>
        /// Used to display additional information on the parent
        /// <c>Hediff</c>'s in-game tooltip.
        /// </summary>
        public override string CompTipStringExtra =>
            //English: "Required Skills:"
            "\n" + "CF_SeverityFromSkill_SkillRequire".Translate() + "\n" +
            //Display a bulleted list of skills that affect severity.
            SkillListFormattedInt + "\n" +
            //If there is a minimum skill requirement...
            (Props.minSkill > 0 ?
                //English: "Minimum level required:"
                "CF_SeverityFromSkill_SkillMin".Translate() + " " +
                    //Display the minimum average of required skills.
                    Props.minSkill.ToString() + "\n" :
                //If no minimum requirement, add nothing here.
                (TaggedString)string.Empty) +
            //English: "Average of relevant skills:"
            "CF_SeverityFromSkill_SkillAverage".Translate() + " " +
            //Display the current skill average.
            SkillAverage.ToString();

        /// <summary>
        /// This method is called every tick, and directly sets the parent
        /// <c>Hediff</c>'s current severity to the appropriate value based
        /// on the pawn's current set of skills.
        /// </summary>
        /// <param name="severityAdjustment">Normally, this would be used to
        /// increase or decrease the parent <c>Hediff</c>'s severity through
        /// smaller increments. Here, it does nothing.</param>
        public override void CompPostTick(ref float severityAdjustment)
        {
            if (SkillAverage < Props.minSkill)
                parent.Severity = 0f;
            else
                parent.Severity = Props.targetSeverity * SkillAverage / 20f;
        }
    }
}
