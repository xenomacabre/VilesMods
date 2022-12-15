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
    /// <c>HediffComp_CraftedQualityOffset</c>.
    /// </summary>
    class HediffCompProperties_CraftedQualityOffset : HediffCompProperties
    {
        /// <summary>
        /// Represents an individual stage of the quality offset effect, with
        /// the minimum quality required for it to occur.
        /// </summary>        
        public class Stage
        {
            /// <summary>
            /// The minimum severity for this stage to occur.
            /// </summary>
            public float minSeverity = 0f;
            /// <summary>
            /// The quality offset applied at this stage.
            /// </summary>
            public int offset = 1;
        }

        /// <summary>
        /// The list of <c>Stage</c>s, each stage having its own offset and
        /// minimum severity.
        /// </summary>
        public List<Stage> stages = new List<Stage>();
        /// <summary>
        /// The chance, represented as a decimal percent, that the offset will
        /// be applied to the crafted item.
        /// </summary>
        public float percentChance = 0.1f;

        /// <summary>
        /// Constructor that automatically attaches the required CompClass to
        /// these properties.
        /// </summary>
        public HediffCompProperties_CraftedQualityOffset() =>
            compClass = typeof(HediffComp_CraftedQualityOffset);
    }
}
