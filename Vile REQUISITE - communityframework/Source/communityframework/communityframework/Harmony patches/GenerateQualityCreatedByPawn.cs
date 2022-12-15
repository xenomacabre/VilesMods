using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;

namespace CF
{
    /// <summary>
    /// Patch of <c>QualityUtility.QualityCreatedByPawn</c>. Add to this patch
    /// if you want to add conditions that affect the quality of crafted items.
    /// </summary>
    [ClassWithPatches("ApplyQualityUtilityPatch")]
    static class QualityUtilityPatch
    {
        [HarmonyPatch(typeof(RimWorld.QualityUtility))]
        [HarmonyPatch("GenerateQualityCreatedByPawn")]
        [HarmonyPatch(new Type[] { typeof(Pawn), typeof(SkillDef), })]
        class GenerateQualityCreatedByPawn
        {
            /// <summary>
            /// Postfix to be run after the base method.
            /// </summary>
            /// <param name="__result">
            /// The orignal quality output by the base method.
            /// </param>
            /// <param name="pawn">
            /// The <c>Pawn</c> crafting the item in question.
            /// </param>
            public static void Postfix(
                ref QualityCategory __result,
                Pawn pawn
            )
            {
                //Checks pawn's health for HediffComp_QualityOffset
                HediffComp_CraftedQualityOffset c;
                foreach (Hediff h in pawn.health.hediffSet.hediffs)
                {
                    c = h.TryGetComp<HediffComp_CraftedQualityOffset>();
                    if (c is null) continue;

                    //If the random chance for a quality offset is rolled, then
                    //offset the quality.
                    Random random = new Random();
                    if (random.Next(100) <= c.PercentChance * 100)
                        //Based on QualityUtility.AddLevels
                        __result = (QualityCategory)
                            Math.Min((int)__result + c.QualityOffset, 6);
                }
            }
        }
    }    
}
