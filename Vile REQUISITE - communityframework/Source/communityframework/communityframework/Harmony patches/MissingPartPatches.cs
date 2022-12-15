using Verse;
using HarmonyLib;

namespace CF
{
    /// <summary>
    /// Harmony patch applied to the <c>Verse.Hediff_MissingPart</c> class,
    /// intended to provide functionality to <c>HediffComp_ShouldRemove</c>,
    /// even though <c>Hediff_MissingComp</c> overrides the method normally
    /// used to detect it.
    /// </summary>
    [ClassWithPatches("ApplyMissingPartPatches")]
    static class MissingPartPatches
    {
        [HarmonyPatch(typeof(Hediff_MissingPart))]
        [HarmonyPatch]
        static class ShouldRemove
        {
            /// <summary>
            /// By default, <c>Hediff_MissingPart.ShouldRemove</c> is
            /// hard-coded to always return <c>false</c>. This patch causes it
            /// to return <c>true</c> if its comps contains 
            /// <c>HediffComp_ShouldRemove</c>. A specific reference to
            /// <c>HediffComp_ShouldRemove</c> is used so that we don't break 
            /// the behavior of other comps that are applied to missing parts.
            /// </summary>
            /// <remarks>
            /// Now, since we already know that the base method returns a
            /// constant value, we don't *need* to incorporate its return
            /// value. However, other modders may change this method, and we 
            /// need to account for that.
            /// </remarks>
            /// <param name="__result">
            /// The original return value of
            /// <c>Hediff_MissingPart.ShouldRemove</c>.
            /// </param>
            /// <param name="__instance">
            /// The <c>Hediff_MissingPart</c> instance.
            /// </param>
            [HarmonyPatch("ShouldRemove", MethodType.Getter)]
            [HarmonyPostfix]
            static void Postfix_ShouldRemove(
                ref bool __result,
                Hediff_MissingPart __instance
            )
            {
                __result = __result ||
                    __instance.TryGetComp<HediffComp_ShouldRemove>() != null;
            }

            /// <summary>
            /// Prevents missing parts from trying to generate a new label if
            /// they're about to be removed anyways. This fixes a
            /// <c>NullReferenceException</c> caused by setting the hediff's
            /// part to <c>null</c>, but only if we've also added
            /// <see cref="HediffComp_ShouldRemove"/> to it.
            /// </summary>
            /// <param name="__instance">
            /// The <c>Hediff_MissingPart</c> instance.
            /// </param>
            /// <returns>
            /// <c>false</c> if the hediff is supposed to be removed,
            /// <c>true</c> otherwise
            /// </returns>
            [HarmonyPatch("LabelBase", MethodType.Getter)]
            [HarmonyPrefix]
            static bool Prefix_LabelBase(Hediff_MissingPart __instance)
            {
                return
                    __instance.TryGetComp<HediffComp_ShouldRemove>() == null;
            }
        }
    }    
}
