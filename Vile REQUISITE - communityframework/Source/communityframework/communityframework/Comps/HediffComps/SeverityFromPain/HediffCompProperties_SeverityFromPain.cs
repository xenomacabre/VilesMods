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
    /// The base <c>HediffProperties</c> used to provide instructions for
    /// <c>HediffComp_PainFromSeverity</c>
    /// </summary>
    class HediffCompProperties_SeverityFromPain : HediffCompProperties
    {
        /// <summary>
        /// If this is set to <c>true</c>, then the <c>Pawn</c>'s current pain
        /// will be divided by their maximum pain shock threshold. If
        /// <c>false</c>, it will always be the exact value.
        /// </summary>
        public bool usePainThreshold = true;

        /// <summary>
        /// Boilerplate constructor, sets the comp's class to
        /// <c>HediffComp_SeverityFromPain</c>.
        /// </summary>
        public HediffCompProperties_SeverityFromPain() =>
            compClass = typeof(HediffComp_SeverityFromPain);
    }
}
