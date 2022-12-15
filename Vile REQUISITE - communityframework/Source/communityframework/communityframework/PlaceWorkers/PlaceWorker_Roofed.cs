using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace CF
{
    /// <summary>
    /// <c>PlaceWorker</c> requiring that the parent <c>Thing</c> be placed
    /// under a roof. <seealso cref="CF.CompValidator"/>
    /// </summary>
    /// <remarks>
    /// Originally by CuproPanda, for Additional Joy Objects.
    /// </remarks>
    public class PlaceWorker_Roofed : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(
            BuildableDef checkingDef,
            IntVec3 loc,
            Rot4 rot,
            Map map,
            Thing thingToIgnore = null,
            Thing thing = null
        )
        {
            if (!map.roofGrid.Roofed(loc))
            {
                return new AcceptanceReport(
                    "CF_Roofed_NeedsRoof".Translate(checkingDef.label)
                );
            }
            return true;
        }
    }
    /// <summary>
    /// <c>PlaceWorker</c> requiring that the parent <c>Thing</c> be placed
    /// under a roof and not over another <c>Thing</c> which is too tall.
    /// <seealso cref="CF.CompValidator"/>
    /// </summary>
    /// <remarks>
    /// Originally by CuproPanda, for Additional Joy Objects.
    /// </remarks>
    public class PlaceWorker_RoofHanger : PlaceWorker_Roofed
    {
		public override AcceptanceReport AllowsPlacing(
            BuildableDef checkingDef,
            IntVec3 loc,
            Rot4 rot,
            Map map,
            Thing thingToIgnore = null,
            Thing thing = null
        )
        {
            //check if tile is roofed
            AcceptanceReport roofedReport =
                base.AllowsPlacing(checkingDef, loc, rot, map, thingToIgnore);
            if (!roofedReport.Accepted) return roofedReport;

            // Don't allow placing on big things
            foreach (
                IntVec3 c in GenAdj.CellsOccupiedBy(loc, rot, checkingDef.Size)
            ) 
            {
                if (c.GetEdifice(map) != null)
                {
                    if (
                        c.GetEdifice(map).def.blockWind == true ||
                        c.GetEdifice(map).def.holdsRoof == true
                    )
                    {
                        return new AcceptanceReport(
                            "CF_Chandelier_TooTall".Translate(
                                c.GetEdifice(map).LabelCap,
                                checkingDef.LabelCap
                            )
                        );
                    }
                }

                IEnumerable<Thing> things = c.GetThingList(map);
                // don't hang if there's already a chandelier here
                if (things.Where(
                    x => x.def.placeWorkers != null
                    && x.def.placeWorkers.Where(
                        y => y.GetType() ==
                            typeof(PlaceWorker_RoofHanger))
                    .Any()
                ).Any())
                {
                    return new AcceptanceReport(
                        "IdenticalThingExists".Translate()
                    );
                }
            }
            // Otherwise, accept placing
            return true;
        }
    }
}
