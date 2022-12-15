using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace CF
{
    /// <summary>
    /// Recipe worker that can add and (optionally) remove <c>Hediff</c>s at
    /// the same tame. Intended for recipes that install bionic implants.
    /// </summary>
    class Recipe_InstallOrReplaceImplant : Recipe_InstallImplant
    {
        /// <summary>
        /// This method is run when the recipe is performed on a pawn via
        /// surgery. It functions almost identically to the base method, except
        /// that it also removes the <c>Hediff</c> defined in
        /// <c>RecipeDef.removesHediff</c>.
        /// </summary>
        /// <param name="pawn">The target of the surgery.</param>
        /// <param name="part">The body part that the operation is being
        /// performed on.</param>
        /// <param name="billDoer">The pawn performing the operation</param>
        /// <param name="ingredients">The items (in this case, usually bionic
        /// implants) that are being used to perform the surgery.</param>
        /// <param name="bill">The <c>def</c> of the recipe for the operation
        /// </param>
        public override void ApplyOnPawn(
           Pawn pawn,
           BodyPartRecord part,
           Pawn billDoer,
           List<Thing> ingredients,
           Bill bill)
        {
            //Most of this is just standard boilerplate for surgery-based
            //recipes.
            if (billDoer != null)
            {
                if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                    return;
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            }
            pawn.health.AddHediff(recipe.addsHediff, part);

            //Now we'll check for any hediffs that need to be removed
            foreach (Hediff h in pawn.health.hediffSet.hediffs)
            {
                if (h.def == recipe.removesHediff &&
                    (recipe.appliedOnFixedBodyParts.NullOrEmpty() ||
                        recipe.appliedOnFixedBodyParts.Contains(h.Part.def)))
                {
                    pawn.health.RemoveHediff(h);
                    if (h.def.spawnThingOnRemoved != null)
                        GenSpawn.Spawn(
                            h.def.spawnThingOnRemoved,
                            billDoer.Position,
                            billDoer.Map);
                    //We need to stop here. The collection has been modified,
                    //so continuing to enumerate through it will cause errors.
                    break;
                }
            }
        }
    }
}
