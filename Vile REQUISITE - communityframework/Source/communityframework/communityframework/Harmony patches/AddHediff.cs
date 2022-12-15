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
    [ClassWithPatches("ApplyOnHediffAddedPatch")]
    static class OnHediffAddedPatch
    {
        [HarmonyPatch(typeof(Pawn_HealthTracker))]
        [HarmonyPatch("AddHediff")]
        static class AddHediff
        {
            [HarmonyPatch(new Type[] {
            typeof(Hediff),
            typeof(BodyPartRecord),
            typeof(DamageInfo?),
            typeof(DamageWorker.DamageResult),
        })]
            public static void Prefix(
                Hediff hediff,
                Pawn_HealthTracker __instance)
            {
                foreach (Hediff h in __instance.hediffSet.hediffs)
                {
                    if (h is HediffWithComps withComps)
                        foreach (HediffComp c in withComps.comps)
                        {
                            if (c is IHediffComp_OnHediffAdded n)
                                n.OnHediffAdded(ref hediff);
                        }
                }
            }
        }
    }    
}
