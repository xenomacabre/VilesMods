using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;

namespace CF
{
    /// <summary>
    /// This patche adds function to the CF_CaravanCapacity stat, allowing
    /// equipment to modify the mass that a pawn ca carry in a caravan.
    /// </summary>
    [ClassWithPatches("ApplyCapacityPatch")]
    static class CapacityPatch
    {
        [HarmonyPatch(typeof(MassUtility))]
        [HarmonyPatch("Capacity")]
        class Capacity
        {
            public static void Postfix(
                Pawn p,
                ref StringBuilder explanation,
                ref float __result
            )
            {
                float cap = p.GetStatValue(CF_StatDefOf.CF_CaravanCapacity);
                __result += cap;

                if (explanation == null) return;
                if (explanation.Length > 0) explanation.AppendLine();

                // Example Strings:
                // "  - Urist: 7kg"
                // "  - megasloth: 12kg"
                // "  - Error: 5kg"
                explanation.Append(
                    "  - " +
                    (p?.LabelShortCap ?? p?.def?.defName ?? "Error") +
                    ": " + cap.ToStringMassOffset());


                // Enabling the following code block will cause the vanilla
                // carry mass stat (used for the hauling job) to also affect
                // caravan carrying mass, but only for equipment.
                /*
                try
                {
                    if (p?.apparel?.WornApparel?.Count > 0)
                    {
                        foreach (var app in p.apparel.WornApparel)
                        {
                            var stat = app?.def?.equippedStatOffsets?
                                .FirstOrDefault(
                                    x => x?.stat == StatDefOf.CarryingCapacity
                                );
                            if (stat == null) continue;
                            float val = stat?.value ?? 0f;
                            {
                                __result += val;
                            }
                        }

                        if (explanation == null) return;
                        if (explanation?.Length > 0)
                        {
                            explanation.AppendLine();
                        }

                        explanation.Append(
                            "  - " +
                            (p?.LabelShortCap ?? p?.def?.defName ?? "Error") +
                            ": " + __result.ToStringMassOffset());
                    }
                }
                catch (Exception e)
                {
                    Log.ErrorOnce(e.ToString(), 13131313);
                }
                */
            }
        }
    }    
}
