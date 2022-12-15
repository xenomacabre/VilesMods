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
    /// This patches the <c>Hatch</c> method so when no parent can be found (which is the case when a pawn is spawned from a crafted item), it is set so the player faction. 
    /// </summary>
    [ClassWithPatches("ApplyHatchPatch")]
    static class HatchPatch
    {
        [HarmonyPatch(typeof(CompHatcher))]
        [HarmonyPatch("Hatch")]
        class Hatch
        {
            public static void Prefix(CompHatcher __instance)
            {
                // if (__instance.hatcheeParent == null) //If no parent is found for the hatchee, set the hatchee's faction to that of the player.
                HatcherExtension ext = __instance.parent.def.GetModExtension<HatcherExtension>();
                if (ext != null && ext.hatcheeForcePlayerFaction)
                {
                    // "parent" here refers to the Thing this CompHatcher is attached to, not the parent as described above.
                    __instance.hatcheeFaction = Faction.OfPlayer;
                }
            }
        }
    }   
}
