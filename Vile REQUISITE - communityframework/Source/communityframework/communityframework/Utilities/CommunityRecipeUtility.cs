using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Verse;
using RimWorld;

namespace CF
{
    /// <summary>
    /// Static helper utility that contains methods pertaining to starting,
    /// doing, and completing bills and recipes.
    /// </summary>
    [StaticConstructorOnStartup]
    static class CommunityRecipeUtility
    {
        /// <summary>
        /// An empty delegate to define the method signature used by
        /// <see cref="Verse.GenRecipe.MakeRecipeProducts"/>.
        /// </summary>
        private delegate Thing PostProcessProductDelegate(
            Thing product,
            RecipeDef recipeDef,
            Pawn worker,
            Precept_ThingStyle
            precept=null,
            ThingStyleDef style=null,
            int? overrideGraphicIndex=null);

        /// <summary>
        /// This delegate refers to the private method
        /// <see cref="Verse.GenRecipe.MakeRecipeProducts"/>.
        /// </summary>
        private static Delegate postProcessProductDelegate;

        /// <summary>
        /// Sets up delegates refering to private methods.
        /// </summary>
        static CommunityRecipeUtility()
        {
            postProcessProductDelegate = typeof(GenRecipe).GetMethod(
                "PostProcessProduct",
                BindingFlags.NonPublic | BindingFlags.Static
            ).CreateDelegate(typeof(PostProcessProductDelegate));
        }

        /// <summary>
        /// Calls the vanilla private method used to finalize crafted items.
        /// This method will set up <c>CompQuality</c> and <c>CompArt</c>,
        /// apply any ideo styles, and will minify the product if possible.
        /// </summary>
        /// <remarks>
        /// This method doesn't do anything other than call a private method
        /// from the vanilla API. Normally, we shouldn't be doing this.
        /// However, this method has no reason to be private in the first
        /// place; it is static and completely stateless.
        /// </remarks>
        /// <param name="product">The crafting product to finalize</param>
        /// <param name="recipeDef">The recipe that created the product</param>
        /// <param name="worker">The pawn doing the recipe</param>
        /// <param name="precept">The pawn's ideo style precept</param>
        /// <returns>A reference to <c>product</c>.</returns>
        public static Thing PostProcessProduct(
            Thing product,
            RecipeDef recipeDef,
            Pawn worker,
            Precept_ThingStyle precept = null,
            ThingStyleDef style=null,
            int? overrideGraphicIndex=null
        )
            => postProcessProductDelegate.DynamicInvoke(
                new object[] {
                    product, recipeDef, worker, precept, style,
                    overrideGraphicIndex
                }
            ) as Thing;
    }
}
