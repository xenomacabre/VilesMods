using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace DoorsExpanded
{
    public class WorkGiver_UseRemoteButton : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);

        private static HashSet<Thing> ButtonsOrLevers(Pawn pawn, bool forced)
        {
            var buttonsOrLevers =
                from Building_DoorRemoteButton t
                in pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_DoorRemoteButton>()
                where forced || t.NeedsToBeSwitched
                select t;
            return new HashSet<Thing>(buttonsOrLevers);
        }

        public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

        public override Danger MaxPathDanger(Pawn pawn) => Danger.Deadly;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return ButtonsOrLevers(pawn, forced: true);
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return ButtonsOrLevers(pawn, forced).Count == 0;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t is not Building_DoorRemoteButton remote)
            {
                return false;
            }
            if (remote.IsDisabledForJob(forced, out var reason))
            {
                JobFailReason.Is(reason);
                return false;
            }
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(HeronDefOf.PH_UseRemoteButton, t);
        }
    }
}
