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
    /// A hediff comp that allows <c>Hediff</c>s to increase or decrease the
    /// value of items crafted by the <c>Pawn</c> that the <c>Hediff</c> is
    /// applied to, at random.
    /// </summary>
    class HediffComp_CraftedQualityOffset : HediffComp
    {
        /// <summary>
        /// Pre-cast reference to the relevant
        /// <c>CompProperties_CraftedQualityOffset</c>.
        /// </summary>
        public HediffCompProperties_CraftedQualityOffset Props =>
           (HediffCompProperties_CraftedQualityOffset)props;

        /// <summary>
        /// The current number of quality levels that the quality of the
        /// completed item should be increased or decreased by, calculated from
        /// the parent <c>Hediff</c>'s severity.
        /// </summary>
        public int QualityOffset
        {
            get
            {
                float highestStage = 0;
                int offset = 0;
                foreach (HediffCompProperties_CraftedQualityOffset.Stage s in
                    Props.stages)
                {
                    if (s.minSeverity >= highestStage &&
                        parent.Severity >= s.minSeverity)
                    {
                        highestStage = s.minSeverity;
                        offset = s.offset;
                    }
                }
                return offset;
            }
        }

        /// <summary>
        /// The chance, represented as a decimal percent, that the offset will
        /// be applied to the crafted item.
        /// </summary>
        public float PercentChance => Props.percentChance;

        /// <summary>
        /// Used to display additional information on the parent
        /// <c>Hediff</c>'s in-game tooltip.
        /// </summary>
        public override string CompTipStringExtra =>
            //If the chance of offset being applied is less than 100%...
            "\n" + (PercentChance < 1f ?
                //English: "Quality Offset Chance:"
                "CF_CraftedQualityOffset_QualityOffsetChance".Translate() +
                //Display offset chance as percent.
                " " + ((int)(PercentChance * 100)).ToString() + "%\n" :
                //If chance is 100%, add nothing here.
                (TaggedString)string.Empty) +
            //English: "Current Quality Offset:"
            "CF_CraftedQualityOffset_QualityOffset".Translate() + " " +
                //Print the current quality offset, with a "+" at the front if
                //it is a positive value.
                (QualityOffset >= 0 ? "+" + QualityOffset.ToString() :
                    QualityOffset.ToString());
    }
}
