using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace CF
{
    /// <summary>
    /// Allows modder to add comps to any items created with a certain Stuff
    /// using the <c>CompsToAddWhenStuff</c> <c>ModExtension</c>.
    /// </summary>
    [ClassWithPatches("ApplyCompFromStuffPatch")]
    static class CompFromStuffPatch
    {
        [HarmonyPatch(
            typeof(ThingMaker),
            nameof(ThingMaker.MakeThing),
            new Type[] { typeof(ThingDef), typeof(ThingDef) }
        )]
        class AddCompPostfix
        {
            [HarmonyPostfix]
            public static void MakeThingPostfix(
                ref Thing __result,
                ref ThingDef stuff
            )
            {
                if (stuff != null && __result is ThingWithComps twc)
                {
                    CompsToAddWhenStuff ext =
                        stuff.GetModExtension<CompsToAddWhenStuff>();
                    if(ext != null && ext.comps != null && ext.comps.Count > 0)
                    {
                        for(int i = 0; i < ext.comps.Count; i++)
                        {
                            ThingComp comp = (ThingComp)Activator
                                .CreateInstance(ext.comps[i].compClass);
                            comp.parent = twc;
                            twc.AllComps.Add(comp);
                            comp.Initialize(ext.comps[i]);
                        }
                    }
                }
            }
        }
    }
}