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
    /// Patches applied to <c>Pawn_HealthTracker.HealthTick</c>, allowing us to
    /// read and write a pawn's health on a per-tick basis.
    /// </summary>
    [ClassWithPatches("ApplyHealthTickPatch")]
    static class HealthTickPatch
    {
        [HarmonyPatch(typeof(Pawn_HealthTracker))]
        [HarmonyPatch("HealthTick")]
        class HealthTick
        {
            /// <summary>
            /// Postfix applied to <c>Pawn_HealthTracker.HealthTick</c>,
            /// allowing additional code to be run after the base method.
            /// </summary>
            /// <param name="__instance">The specific instance of 
            /// <c>Pawn_HealthTracker</c> currently being ticked.</param>
            /// <param name="___pawn">The pawn that this
            /// <c>Pawn_HealthTracker</c> belongs to.</param>
            static void Postfix(Pawn_HealthTracker __instance, Pawn ___pawn)
            {
                //Dead pawns are not processed beyond this point:
                if (__instance.Dead)
                    return;

                //This block is used to check information stored in the pawn's
                //traits.
                if (___pawn.story != null)
                {
                    foreach (Trait trait in ___pawn.story.traits.allTraits)
                    {
                        //Provides functionality of TraitRandomDiseasePools
                        TraitRandomDiseasePool diseasePools =
                            trait.def.
                                GetModExtension<TraitRandomDiseasePool>();
                        if (diseasePools != null)
                        {
                            foreach (
                                TraitRandomDiseasePool.DiseasePool pool in
                                diseasePools.pools)
                            {
                                if (pool.degree != trait.Degree)
                                    continue;
                                //If the current trait has diseases to cause
                                //over random intervals, try to cause them now.
                                if (Rand.MTBEventOccurs(
                                    pool.mtbDiseaseDays, 60000f, 60f))
                                {
                                    pool.CauseIncidentFromPool(___pawn);
                                }
                            }
                        }
                    }
                }
            }
        }
    }    
}
