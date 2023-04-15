using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;

namespace CF
{
    [ClassWithPatches("ApplyCompFacilityPatch")]
    static class CompFacilityPatch
    {
        /// <summary>
        /// This patches the method Notify_LinkRemoved so the CompUnlocksRecipe specific code is executed after the regular Notify_LinkRemoved code is run.
        /// CompUnlocksRecipe in this case checks when link is removed, if the removed facility was the only one of that type, and if so, remove the recipes from the target workbench. 
        /// </summary>
        [HarmonyPatch(typeof(CompFacility), "Notify_NewLink")]
        class CompFacility_Notify_NewLink
        {
            public static bool Prefix(Thing thing, CompFacility __instance)
            {
                for (int index = 0; index < __instance.LinkedBuildings.Count; ++index)
                {
                    if (__instance.LinkedBuildings[index] == thing)
                    {
                        if (DebugSettings.godMode)
                            Log.Message("Notify_NewLink was called but the link is already here.");
                        return false;
                    }
                }
                __instance.LinkedBuildings.Add(thing);

                return false;
            }
        }
        /// <summary>
        /// This patches the method Notify_NewLink so the CompUnlocksRecipe specific code is executed after the regular Notify_NewLink code is run.
        /// CompUnlocksRecipe in this case checks when a new link is created. If this new link is one with a new unique facility, and the added recipes are also not yet added to the workbench, they will be added.
        /// </summary>
        [HarmonyPatch(typeof(CompFacility), "Notify_LinkRemoved")]
        class CompFacility_Notify_LinkRemoved
        {
            public static bool Prefix(Thing thing, CompFacility __instance)
            {
                for (int index = 0; index < __instance.LinkedBuildings.Count; ++index)
                {
                    if (__instance.LinkedBuildings[index] == thing)
                    {
                        __instance.LinkedBuildings.RemoveAt(index);
                        return false;
                    }
                }
                if (DebugSettings.godMode)
                    Log.Error("Notify_LinkRemoved was called but there is no such link here.");
                return false;
            }
        }
    }
    
}

