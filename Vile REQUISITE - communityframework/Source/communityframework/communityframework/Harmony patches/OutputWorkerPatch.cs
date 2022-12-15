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
    /// Adds patches that will run before and after any recipe is complete.
    /// Each patch will check the recipe for a <see cref="UseOutputWorkers"/>
    /// extension, and will run the relevant method from each
    /// <see cref="OutputWorker"/> found.
    /// </summary>
    [ClassWithPatches("ApplyOutputWorkerPatch")]
    static class OutputWorkerPatch
    {
        [HarmonyPatch(typeof(GenRecipe))]
        [HarmonyPatch(nameof(GenRecipe.MakeRecipeProducts))]
        class MakeRecipeProducts
        {
            public static void Prefix(
                RecipeDef recipeDef,
                Pawn worker,
                List<Thing> ingredients,
                IBillGiver billGiver,
                Precept_ThingStyle precept,
                ThingStyleDef style,
                int? overrideGraphicIndex
            )
            {
                // Get the extension, quit if none found
                UseOutputWorkers ext =
                    recipeDef.GetModExtension<UseOutputWorkers>();
                if (ext == null) return;

                // Run every pre-craft method
                foreach (OutputWorker o in ext.ActiveWorkers)
                    o.PreCraft(
                        recipeDef,
                        worker,
                        ingredients,
                        billGiver,
                        precept,
                        style,
                        overrideGraphicIndex
                    );
            }

            public static void Postfix(
                ref IEnumerable<Thing> __result,
                RecipeDef recipeDef,
                Pawn worker,
                List<Thing> ingredients,
                IBillGiver billGiver,
                Precept_ThingStyle precept,
                ThingStyleDef style,
                int? overrideGraphicIndex
            )
            {
                // Stores any new products that each OutputWorker produces, so
                // that they can be finalized later.
                IEnumerable<Thing> newProducts;

                // Get the extension, quit if none found
                UseOutputWorkers ext =
                    recipeDef.GetModExtension<UseOutputWorkers>();
                if (ext == null) return;

                // Run each post-craft method, then finalize any Things that
                // they produce before adding them to the list of products.
                foreach (OutputWorker o in ext.ActiveWorkers)
                {
                    newProducts = o.PostCraft(
                        __result,
                        recipeDef,
                        worker,
                        ingredients,
                        billGiver,
                        precept,
                        style,
                        overrideGraphicIndex
                    );

                    foreach (Thing t in newProducts)
                    {
                        CommunityRecipeUtility.PostProcessProduct(
                            t,
                            recipeDef,
                            worker,
                            precept,
                            style,
                            overrideGraphicIndex
                        );
                        __result.AddItem(t);
                    }
                }
            }
        }
    }
}
