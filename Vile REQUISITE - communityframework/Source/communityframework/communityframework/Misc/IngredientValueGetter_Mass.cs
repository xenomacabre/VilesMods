using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace CF
{
    class IngredientValueGetter_Mass : IngredientValueGetter
    {
        public override float ValuePerUnitOf(ThingDef t)
        {
            return t.BaseMass;
        }
        public override string BillRequirementsDescription(
            RecipeDef r,
            IngredientCount ing
        )
        {
            return "CF_BillRequiresMass".Translate(
                ing.GetBaseCount(),
                ing.filter.Summary
            );
        }
    }
}