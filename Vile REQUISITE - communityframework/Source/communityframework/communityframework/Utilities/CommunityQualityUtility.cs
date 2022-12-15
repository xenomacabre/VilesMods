using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Verse;
using RimWorld;

namespace CF
{
    /// <summary>
    /// Static utility that allows mod developers to change the quality of
    /// <c>Thing</c>s with the <c>CompQuality</c> <c>ThingComp</c> in a
    /// non-invasive way.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class CommunityQualityUtility
    {
        /// <summary>
        /// <c>FieldInfo</c> used to refer to the private <c>qualityInt</c>
        /// field of the <c>CompQuality</c> class.
        /// </summary>
        public static FieldInfo QualityInt;

        /// <summary>
        /// No-arg constructor, run when the game starts. Initializes the value
        /// of <c>QualityUtility.QualityInt</c>.
        /// </summary>
        static CommunityQualityUtility()
        {
            QualityInt = typeof(CompQuality).GetField(
                "qualityInt", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// Directly sets the <c>QualityCategory</c> of a <c>CompQuality</c>
        /// without re-initializing any sibling <c>CompArt</c> that may exist.
        /// </summary>
        /// <param name="comp">
        /// The <c>CompQuality</c> to have its <c>QualityCategory</c> changed.
        /// </param>
        /// <param name="quality">
        /// The target <c>QualityCategory</c> to set.
        /// </param>
        public static void SetQualitySilent(
            this CompQuality comp,
            QualityCategory quality)
        {
            QualityInt.SetValue(comp, quality);
        }
    }
}
