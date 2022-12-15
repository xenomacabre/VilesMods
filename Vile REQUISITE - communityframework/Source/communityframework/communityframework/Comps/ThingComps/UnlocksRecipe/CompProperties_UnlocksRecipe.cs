using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CF
{
    public class CompProperties_UnlocksRecipe : CompProperties
    {
        public class LinkableFacilities
        {
            public ThingDef targetFacility;
            public QualityCategory minQuality;
        }
        public CompProperties_UnlocksRecipe() => compClass = typeof(CompUnlocksRecipe);
        public List<LinkableFacilities> linkableFacilities; // Which facilities should be targeted
        public List<RecipeDef> recipes; // Which recipes should be added
    }
}
