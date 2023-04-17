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
    [ClassWithPatches("ApplyCompAffectedByFacilitiesPatch")]
    static class CompAffectedByFacilitiesPatch
    {
        /// <summary>
        /// This patches the method Notify_LinkRemoved so the CompUnlocksRecipe specific code is executed after the regular Notify_LinkRemoved code is run.
        /// CompUnlocksRecipe in this case checks when link is removed, if the removed facility was the only one of that type, and if so, remove the recipes from the target workbench. 
        /// </summary>
        [HarmonyPatch(typeof(CompAffectedByFacilities), "Notify_NewLink")]
        class Notify_NewLink
        {
            public static bool Prefix(Thing facility, CompAffectedByFacilities __instance)
            {
                for (int index = 0; index < __instance.LinkedFacilitiesListForReading.Count; ++index)
                {
                    if (__instance.LinkedFacilitiesListForReading[index] == facility)
                    {
                        if (DebugSettings.godMode)
                            Log.Message("Notify_NewLink was called but the link is already here.");
                        return false;
                    }
                }
                Thing supplantedFacility = GetPotentiallySupplantedFacility(facility.def, facility.Position, facility.Rotation, __instance);
                if (supplantedFacility != null)
                {
                    supplantedFacility.TryGetComp<CompFacility>().Notify_LinkRemoved((Thing)__instance.parent);
                    __instance.LinkedFacilitiesListForReading.Remove(supplantedFacility);
                }
                __instance.LinkedFacilitiesListForReading.Add(facility);

                return false;
            }

            private static Thing GetPotentiallySupplantedFacility(
                ThingDef facilityDef,
                IntVec3 facilityPos,
                Rot4 facilityRot,
                CompAffectedByFacilities comp)
            {
                Thing thing = (Thing)null;
                int num = 0;
                for (int index = 0; index < comp.LinkedFacilitiesListForReading.Count; ++index)
                {
                    if (comp.LinkedFacilitiesListForReading[index].def == facilityDef)
                    {
                        if (thing == null)
                            thing = comp.LinkedFacilitiesListForReading[index];
                        ++num;
                    }
                }
                if (num == 0)
                    return (Thing)null;
                CompProperties_Facility compProperties = facilityDef.GetCompProperties<CompProperties_Facility>();
                if (num + 1 <= compProperties.maxSimultaneous)
                    return (Thing)null;
                Thing supplantedFacility = thing;
                for (int index = 0; index < comp.LinkedFacilitiesListForReading.Count; ++index)
                {
                    if (facilityDef == comp.LinkedFacilitiesListForReading[index].def && IsBetter(supplantedFacility.def, supplantedFacility.Position, supplantedFacility.Rotation, comp.LinkedFacilitiesListForReading[index], comp))
                        supplantedFacility = comp.LinkedFacilitiesListForReading[index];
                }
                return supplantedFacility;
            }

            private static bool IsBetter(
              ThingDef facilityDef,
              IntVec3 facilityPos,
              Rot4 facilityRot,
              Thing thanThisFacility,
              CompAffectedByFacilities comp)
            {
                if (facilityDef != thanThisFacility.def)
                {
                    if (DebugSettings.godMode)
                        Log.Error("Comparing two different facility defs.");
                    return false;
                }
                Vector3 b = GenThing.TrueCenter(facilityPos, facilityRot, facilityDef.size, facilityDef.Altitude);
                Vector3 a = comp.parent.TrueCenter();
                float num1 = Vector3.Distance(a, b);
                float num2 = Vector3.Distance(a, thanThisFacility.TrueCenter());
                if ((double)num1 != (double)num2)
                    return (double)num1 < (double)num2;
                return facilityPos.x != thanThisFacility.Position.x ? facilityPos.x < thanThisFacility.Position.x : facilityPos.z < thanThisFacility.Position.z;
            }

            public static void Postfix(Thing facility, CompAffectedByFacilities __instance)
            {
                CompUnlocksRecipe.AddRecipe(facility, __instance);
            }
        }
        /// <summary>
        /// This patches the method Notify_NewLink so the CompUnlocksRecipe specific code is executed after the regular Notify_NewLink code is run.
        /// CompUnlocksRecipe in this case checks when a new link is created. If this new link is one with a new unique facility, and the added recipes are also not yet added to the workbench, they will be added.
        /// </summary>
        [HarmonyPatch(typeof(CompAffectedByFacilities), "Notify_LinkRemoved")]
        class Notify_LinkRemoved
        {
            public static bool Prefix(Thing thing, CompAffectedByFacilities __instance)
            {
                for (int index = 0; index < __instance.LinkedFacilitiesListForReading.Count; ++index)
                {
                    if (__instance.LinkedFacilitiesListForReading[index] == thing)
                    {
                        __instance.LinkedFacilitiesListForReading.RemoveAt(index);
                        return false;
                    }
                }
                if (DebugSettings.godMode)
                    Log.Message("Notify_LinkRemoved was called but there is no such link here.");
                return false;
            }

            public static void Postfix(Thing thing, CompAffectedByFacilities __instance)
            {
                CompUnlocksRecipe.RemoveRecipe(thing, __instance);
            }
        }
    }
    
}

