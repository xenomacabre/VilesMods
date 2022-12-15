using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace CF
{
    /// <summary>
    /// <c>PlaceWorker</c> requiring that the parent <c>Thing</c> be placed on 
    /// a cell adjacent to but facing away from a wall.
    /// <seealso cref="CF.CompValidator"/>
    /// <remarks>
    /// Originally by CuproPanda, for Additional Joy Objects.
    /// </remarks>
    /// </summary>
    public class PlaceWorker_AgainstWall : PlaceWorker
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
            // Get the tile behind this object
            IntVec3 c = loc - rot.FacingCell;
            // Determine if the tile is an edifice 
            Building edifice = c.GetEdifice(map);
            // Don't place outside of the map
            if (!c.InBounds(map) || !loc.InBounds(map)) return false;
            // Only allow placing on walls, and not if another faction owns the
            // wall
            if (!edifice.IsWall())
                // || (edifice.Faction != null
                // || edifice.Faction != Faction.OfPlayer))    
                return new AcceptanceReport(
                    "CF_MustBePlacedOnWall".Translate(checkingDef.LabelCap)
                );
            // Otherwise, accept placing
            return true;                                                    
        }
    }
    /// <summary>
    /// <c>PlaceWorker</c> requiring that the parent <c>Thing</c> be placed on
    /// a wall but not overlapping the same <c>Thing</c> in the same rotation. 
    /// <seealso cref="CF.CompValidator"/>
    /// </summary> 
    public class PlaceWorker_OnWall : PlaceWorker
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
            Building buil = loc.GetEdifice(map);
            // Building must be in map bounds
            if (!loc.InBounds(map))
                return false;

            // Building must be a wall, and must be owned by the player's
            // faction
            if (
                !buil.IsWall()
                || buil.Faction != Faction.OfPlayer
            )
            {
                return new AcceptanceReport(
                    "CF_MustBePlacedOnWall".Translate(checkingDef.LabelCap)
                );
            }

            // An identical building with the same rotation cannot exist in the
            // same location.
            if (loc.ConflictingThing(checkingDef, rot, map))
            {
                return new AcceptanceReport(
                    "IdenticalThingExists".Translate()
                );
            }
            return true;
        }

        public override bool ForceAllowPlaceOver(BuildableDef other)
        {
            return true;
        }
    }
}

