﻿using System;
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
    [StaticConstructorOnStartup]
    static class AutoLinksForUnlockPatch
    {
        /// <summary>
        /// This add recipe links automatically if AutoLinks is present.
        /// </summary>
        /// 
        static AutoLinksForUnlockPatch()
        {
            foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
                ProcessThing(allDef);
        }


        public static void ProcessThing(ThingDef def)
        {
            CompProperties_UnlocksRecipe unlockRecipes = def.GetCompProperties<CompProperties_UnlocksRecipe>();
            if (unlockRecipes != null && !unlockRecipes.recipes.NullOrEmpty())
            {
                foreach (RecipeDef recipe in unlockRecipes.recipes)
                {
                    foreach (IngredientCount ingredient in DefDatabase<RecipeDef>.AllDefsListForReading.Find(r => r.defName == recipe.defName).ingredients)
                    {
                        if (ingredient.IsFixedIngredient)
                            AddMulti((Def)def, (Def)ingredient.FixedIngredient);
                    }
                }
            }
        }

        private static void AddLink(Def def, Def toAdd)
        {
            if (def == null || toAdd == null)
                return;
            if (def.descriptionHyperlinks == null)
                def.descriptionHyperlinks = new List<DefHyperlink>();
            if (def.descriptionHyperlinks.Any<DefHyperlink>((Predicate<DefHyperlink>)(x => x.def == toAdd)))
                return;
            def.descriptionHyperlinks.Add((DefHyperlink)toAdd);
        }

        private static void AddMulti(Def def, Def toAdd)
        {
            AddLink(def, toAdd);
            AddLink(toAdd, def);
        }
    }
}

