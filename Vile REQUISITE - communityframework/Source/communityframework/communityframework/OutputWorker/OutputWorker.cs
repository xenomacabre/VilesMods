using System.Collections.Generic;
using Verse;
using RimWorld;

namespace CF
{
    /// <summary>
    /// Abstract base class containing methods to run upon completing certain
    /// recipes.
    /// </summary>
   abstract class OutputWorker
    {
        /// <summary>
        /// Method to run when a recipe is completed, but before the products
        /// of the recipe are created.
        /// </summary>
        /// <param name="recipeDef">The def of the recipe done.</param>
        /// <param name="worker">The pawn doing the recipe</param>
        /// <param name="ingredients">
        /// A list of things consumed by the recipe
        /// </param>
        /// <param name="billGiver">
        /// The IBillGiver of whatever produced the recipe bill
        /// </param>
        /// <param name="precept">The style precepts of the worker</param>
        /// <param name="style">
        /// The style to be applied, independent of ideoligious precepts.
        /// </param>
        /// <param name="overrideGraphicIndex">
        /// Index of the desired graphic override.
        /// </param>
        public abstract void PreCraft(
            RecipeDef recipeDef,
            Pawn worker,
            IEnumerable<Thing> ingredients,
            IBillGiver billGiver,
            Precept_ThingStyle precept=null,
            ThingStyleDef style=null,
            int? overrideGraphicIndex=null
        );

        /// <summary>
        /// Method to run when a recipe is completed, after the products are
        /// created and post-processed.
        /// </summary>
        /// <remarks>
        /// Any <c>Thing</c>s returned by this method will be post-processed
        /// automatically. This means that <c>CompQuality</c>, <c>CompArt</c>,
        /// ideo styles, and minification will be taken care of for you.
        /// </remarks>
        /// <param name="products">
        /// A list of finalized things produced by the recipe.
        /// </param>
        /// <param name="recipeDef">The def of the recipe done.</param>
        /// <param name="worker">The pawn doing the recipe</param>
        /// <param name="ingredients">
        /// A list of things consumed by the recipe
        /// </param>
        /// <param name="billGiver">
        /// The IBillGiver of whatever produced the recipe bill
        /// </param>
        /// <param name="precept">The style precepts of the worker</param>
        /// <param name="style">
        /// The style to be applied, independent of ideoligious precepts.
        /// </param>
        /// <param name="overrideGraphicIndex">
        /// Index of the desired graphic override.
        /// </param>
        /// <returns>
        /// Any additional <c>Thing</c>s to be added to recipe products.
        /// </returns>
        public abstract IEnumerable<Thing> PostCraft(
            IEnumerable<Thing> products,
            RecipeDef recipeDef,
            Pawn worker,
            IEnumerable<Thing> ingredients,
            IBillGiver billGiver,
            Precept_ThingStyle precept=null,
            ThingStyleDef style=null,
            int? overrideGraphicIndex=null
        );
    }
}
