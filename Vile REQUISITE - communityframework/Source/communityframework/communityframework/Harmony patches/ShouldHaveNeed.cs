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
    /// This patches the method ShouldHaveNeed so it checks if a pawn has the IgnoreNeed <c>DefModExtension</c>. 
    /// If this <c>DefModExtension</c> contains the need in its list, ignore it.
    /// </summary>
    [ClassWithPatches("ApplyShouldHaveNeedPatch")]
    static class ShouldHaveNeedPatch
    {
        [HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed")]
        class ShouldHaveNeed
        {
            public static void Postfix(
                ref bool __result,
                NeedDef nd,
                Pawn ___pawn)
            {
                //Ensure that the pawn has the ModExtension before trying to access
                IgnoreNeed ignore =
                    ___pawn.def.GetModExtension<IgnoreNeed>() ??
                    ___pawn.kindDef.GetModExtension<IgnoreNeed>();
                if (ignore != null && ignore.needs.Contains(nd))
                {
                    __result = false;
                }
            }
        }
    }    
}
