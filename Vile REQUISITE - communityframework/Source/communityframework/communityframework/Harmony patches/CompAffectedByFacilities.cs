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
    [ClassWithPatches("ApplyCompAffectedByFacilitiesPatch")]
    static class CompAffectedByFacilitiesPatch
    {
        /// <summary>
        /// This patches the method Notify_LinkRemoved so the CompUnlocksRecipe specific code is executed after the regular Notify_LinkRemoved code is run.
        /// CompUnlocksRecipe in this case checks when link is removed, if the removed facility was the only one of that type, and if so, remove the recipes from the target workbench. 
        /// </summary>
        [HarmonyPatch(typeof(CompAffectedByFacilities), "Notify_LinkRemoved")]
        class Notify_LinkRemoved
        {
            public static void Postfix(Thing thing, CompAffectedByFacilities __instance)
            {
                CompUnlocksRecipe.RemoveRecipe(thing, __instance);
            }
        }
        /// <summary>
        /// This patches the method Notify_NewLink so the CompUnlocksRecipe specific code is executed after the regular Notify_NewLink code is run.
        /// CompUnlocksRecipe in this case checks when a new link is created. If this new link is one with a new unique facility, and the added recipes are also not yet added to the workbench, they will be added.
        /// </summary>
        [HarmonyPatch(typeof(CompAffectedByFacilities), "Notify_NewLink")]
        class Notify_NewLink
        {
            public static void Postfix(Thing facility, CompAffectedByFacilities __instance)
            {
                CompUnlocksRecipe.AddRecipe(facility, __instance);
            }
        }
    }
    
}

