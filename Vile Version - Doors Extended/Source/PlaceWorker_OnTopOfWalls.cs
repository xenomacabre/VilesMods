// Note: This is copied from JecsTools.

using Verse;

namespace JecsTools
{
    public class PlaceWorker_OnTopOfWalls : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
            Thing thingToIgnore = null, Thing thing = null)
        {
            if (loc.GetThingList(map).Exists(IsWall))
                return true;
            // Note: Using different translation key ("JT" => "PH") to avoid "duplicate keyed translation key" warnings.
            return new AcceptanceReport("PH_PlaceWorker_OnTopOfWalls".Translate());
        }

        static bool IsWall(Thing thing)
        {
            // In RW 1.3+, it seems like BuildingProperties.isPlaceOverableWall or ThingDef.IsSmoothed indicates whether something is a "wall".
            if (thing.def is { building: { isPlaceOverableWall: true } } or { IsSmoothed: true })
                return true;
            // Legacy heuristic for mods that don't use isPlaceOverableWall.
            if (thing.def.defName.Contains("Wall"))
                return true;
            return false;
        }
    }
}
