// Note: This is excluded from the build and is only provided for comparing with Building_DoorExpanded.

using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld
{
    public class Building_Door : Building
    {
        private const float OpenTicks = 45f;
        private const int CloseDelayTicks = 110;
        private const int WillCloseSoonThreshold = 111;
        private const int ApproachCloseDelayTicks = 300;
        private const int MaxTicksSinceFriendlyTouchToAutoClose = 120;
        private const float PowerOffDoorOpenSpeedFactor = 0.25f;
        private const float VisualDoorOffsetStart = 0f;
        private const float VisualDoorOffsetEnd = 0.45f;
        private const float NotifyFogGridDoorOpenPct = 0.4f;

        public CompPowerTrader powerComp;
        private bool openInt;
        private bool holdOpenInt;
        private int lastFriendlyTouchTick = -9999;
        protected int ticksUntilClose;
        protected int ticksSinceOpen;
        private bool freePassageWhenClearedReachabilityCache;
        private Pawn approachingPawn;

        public bool Open => openInt;

        public bool HoldOpen => holdOpenInt;

        public bool FreePassage => openInt && (holdOpenInt || !WillCloseSoon);

        public int TicksTillFullyOpened
        {
            get
            {
                var ticksTillFullyOpened = TicksToOpenNow - ticksSinceOpen;
                if (ticksTillFullyOpened < 0)
                {
                    ticksTillFullyOpened = 0;
                }
                return ticksTillFullyOpened;
            }
        }

        public bool WillCloseSoon
        {
            get
            {
                if (!Spawned)
                {
                    return true;
                }
                if (!openInt)
                {
                    return true;
                }
                if (holdOpenInt)
                {
                    return false;
                }
                if (ticksUntilClose > 0 && ticksUntilClose <= WillCloseSoonThreshold && !BlockedOpenMomentary)
                {
                    return true;
                }
                if (CanTryCloseAutomatically && !BlockedOpenMomentary)
                {
                    return true;
                }
                for (var i = 0; i < 5; i++)
                {
                    var c = Position + GenAdj.CardinalDirectionsAndInside[i];
                    if (!c.InBounds(Map))
                    {
                        continue;
                    }
                    var thingList = c.GetThingList(Map);
                    for (var j = 0; j < thingList.Count; j++)
                    {
                        if (thingList[j] is Pawn pawn && !pawn.HostileTo(this) && !pawn.Downed &&
                            (pawn.Position == Position || (pawn.pather.Moving && pawn.pather.nextCell == Position)))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public bool BlockedOpenMomentary
        {
            get
            {
                var thingList = Position.GetThingList(Map);
                for (var i = 0; i < thingList.Count; i++)
                {
                    var thing = thingList[i];
                    if (thing.def.category is ThingCategory.Item or ThingCategory.Pawn)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool DoorPowerOn => powerComp is { PowerOn: true };

        public bool SlowsPawns => !DoorPowerOn || TicksToOpenNow > 20;

        public int TicksToOpenNow
        {
            get
            {
                var ticksToOpenNow = OpenTicks / this.GetStatValue(StatDefOf.DoorOpenSpeed);
                if (DoorPowerOn)
                {
                    ticksToOpenNow *= PowerOffDoorOpenSpeedFactor;
                }
                return Mathf.RoundToInt(ticksToOpenNow);
            }
        }

        private bool CanTryCloseAutomatically => FriendlyTouchedRecently && !HoldOpen;

        private bool FriendlyTouchedRecently => Find.TickManager.TicksGame < lastFriendlyTouchTick + MaxTicksSinceFriendlyTouchToAutoClose;

        public override bool FireBulwark => !Open && base.FireBulwark;

        private float OpenPct => Mathf.Clamp01((float)ticksSinceOpen / (float)TicksToOpenNow);

        public override void PostMake()
        {
            base.PostMake();
            powerComp = GetComp<CompPowerTrader>();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            ClearReachabilityCache(map);
            if (BlockedOpenMomentary)
            {
                DoorOpen();
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            var map = Map;
            base.DeSpawn(mode);
            ClearReachabilityCache(map);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref openInt, "open", false);
            Scribe_Values.Look(ref holdOpenInt, "holdOpen", false);
            Scribe_Values.Look(ref lastFriendlyTouchTick, nameof(lastFriendlyTouchTick), 0);
            Scribe_References.Look(ref approachingPawn, nameof(approachingPawn));
            if (Scribe.mode is LoadSaveMode.LoadingVars && openInt)
            {
                ticksSinceOpen = TicksToOpenNow;
            }
        }

        public override void SetFaction(Faction newFaction, Pawn recruiter = null)
        {
            base.SetFaction(newFaction, recruiter);
            if (Spawned)
            {
                ClearReachabilityCache(Map);
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (FreePassage != freePassageWhenClearedReachabilityCache)
            {
                ClearReachabilityCache(Map);
            }
            if (!openInt)
            {
                if (ticksSinceOpen > 0)
                {
                    ticksSinceOpen--;
                }
                if ((Find.TickManager.TicksGame + thingIDNumber.HashOffset()) % TemperatureTuning.Door_TempEqualizeIntervalClosed == 0)
                {
                    GenTemperature.EqualizeTemperaturesThroughBuilding(this, TemperatureTuning.Door_TempEqualizeRate, twoWay: false);
                }
            }
            else
            {
                if (!openInt)
                {
                    return;
                }
                if (ticksSinceOpen < TicksToOpenNow)
                {
                    ticksSinceOpen++;
                }
                var thingList = Position.GetThingList(Map);
                for (var i = 0; i < thingList.Count; i++)
                {
                    if (thingList[i] is Pawn pawn)
                    {
                        CheckFriendlyTouched(pawn);
                    }
                }
                if (ticksUntilClose > 0)
                {
                    if (Map.thingGrid.CellContains(Position, ThingCategory.Pawn))
                    {
                        ticksUntilClose = CloseDelayTicks;
                    }
                    ticksUntilClose--;
                    if (ticksUntilClose <= 0 && !holdOpenInt && !DoorTryClose())
                    {
                        ticksUntilClose = 1;
                    }
                }
                else if (CanTryCloseAutomatically)
                {
                    ticksUntilClose = CloseDelayTicks;
                }
                if ((Find.TickManager.TicksGame + thingIDNumber.HashOffset()) % TemperatureTuning.Door_TempEqualizeIntervalOpen == 0)
                {
                    GenTemperature.EqualizeTemperaturesThroughBuilding(this, TemperatureTuning.Door_TempEqualizeRate, twoWay: false);
                }
                if (OpenPct >= NotifyFogGridDoorOpenPct && approachingPawn is not null)
                {
                    Map.fogGrid.Notify_PawnEnteringDoor(this, approachingPawn);
                    approachingPawn = null;
                }
            }
        }

        public void CheckFriendlyTouched(Pawn p)
        {
            if (!p.HostileTo(this) && PawnCanOpen(p))
            {
                lastFriendlyTouchTick = Find.TickManager.TicksGame;
            }
        }

        public void Notify_PawnApproaching(Pawn p, int moveCost)
        {
            CheckFriendlyTouched(p);
            var pawnCanOpen = PawnCanOpen(p);
            if (pawnCanOpen || Open)
            {
                approachingPawn = p;
            }
            if (pawnCanOpen && !SlowsPawns)
            {
                var ticksToClose = Mathf.Max(ApproachCloseDelayTicks, moveCost + 1);
                DoorOpen(ticksToClose);
            }
        }

        public bool CanPhysicallyPass(Pawn p)
        {
            return FreePassage || PawnCanOpen(p) || (Open && p.HostileTo(this));
        }

        public virtual bool PawnCanOpen(Pawn p)
        {
            if (Map is { } map && map.Parent.doorsAlwaysOpenForPlayerPawns && p.Faction == Faction.OfPlayer)
            {
                return true;
            }
            if (p.GetLord() is { LordJob: { } lordJob } && lordJob.CanOpenAnyDoor(p))
            {
                return true;
            }
            if (WildManUtility.WildManShouldReachOutsideNow(p))
            {
                return true;
            }
            if (p.RaceProps.FenceBlocked && !def.building.roamerCanOpen && (!p.roping.IsRopedByPawn || !PawnCanOpen(p.roping.RopedByPawn)))
            {
                return false;
            }
            if (Faction is null)
            {
                return true;
            }
            if (p.guest is { Released: true })
            {
                return true;
            }
            return GenAI.MachinesLike(Faction, p);
        }

        public override bool BlocksPawn(Pawn p) => !openInt && !PawnCanOpen(p);

        protected void DoorOpen(int ticksToClose = CloseDelayTicks)
        {
            if (openInt)
            {
                ticksUntilClose = ticksToClose;
            }
            else
            {
                ticksUntilClose = TicksToOpenNow + ticksToClose;
            }
            if (!openInt)
            {
                openInt = true;
                CheckClearReachabilityCacheBecauseOpenedOrClosed();
                if (DoorPowerOn)
                {
                    def.building.soundDoorOpenPowered.PlayOneShot(new TargetInfo(Position, Map));
                }
                else
                {
                    def.building.soundDoorOpenManual.PlayOneShot(new TargetInfo(Position, Map));
                }
            }
        }

        protected bool DoorTryClose()
        {
            if (holdOpenInt || BlockedOpenMomentary)
            {
                return false;
            }
            openInt = false;
            CheckClearReachabilityCacheBecauseOpenedOrClosed();
            if (DoorPowerOn)
            {
                def.building.soundDoorClosePowered.PlayOneShot(new TargetInfo(Position, Map));
            }
            else
            {
                def.building.soundDoorCloseManual.PlayOneShot(new TargetInfo(Position, Map));
            }
            return true;
        }

        public void StartManualOpenBy(Pawn opener)
        {
            DoorOpen();
        }

        public void StartManualCloseBy(Pawn closer)
        {
            ticksUntilClose = CloseDelayTicks;
        }

        public override void Draw()
        {
            Rotation = DoorRotationAt(Position, Map);
            var offsetMod = VisualDoorOffsetStart + VisualDoorOffsetEnd * OpenPct;
            for (var i = 0; i < 2; i++)
            {
                Vector3 offsetVector;
                Mesh mesh;
                if (i == 0)
                {
                    offsetVector = new Vector3(0f, 0f, -1f);
                    mesh = MeshPool.plane10;
                }
                else
                {
                    offsetVector = new Vector3(0f, 0f, 1f);
                    mesh = MeshPool.plane10Flip;
                }
                var rotation = Rotation;
                rotation.Rotate(RotationDirection.Clockwise);
                offsetVector = rotation.AsQuat * offsetVector;
                var drawPos = DrawPos;
                drawPos.y = AltitudeLayer.DoorMoveable.AltitudeFor();
                drawPos += offsetVector * offsetMod;
                Graphics.DrawMesh(mesh, drawPos, Rotation.AsQuat, Graphic.MatAt(Rotation), 0);
                Graphic.ShadowGraphic?.DrawWorker(drawPos, Rotation, def, this, 0f);
            }
            Comps_PostDraw();
        }

        private static int AlignQualityAgainst(IntVec3 c, IntVec3 offset, Map map)
        {
            var c2 = c + offset;
            if (!c2.InBounds(map))
            {
                return 0;
            }
            if (!c2.WalkableByNormal(map))
            {
                return 9;
            }
            var thingList = c2.GetThingList(map);
            for (var i = 0; i < thingList.Count; i++)
            {
                var thing = thingList[i];
                if (typeof(Building_Door).IsAssignableFrom(thing.def.thingClass))
                {
                    if ((c - offset).GetDoor(map) is null)
                    {
                        return 1;
                    }
                    return 5;
                }
                if (thing.def.IsFence)
                {
                    return 1;
                }
                if (thing is Blueprint blueprint)
                {
                    if (blueprint.def.entityDefToBuild.passability is Traversability.Impassable)
                    {
                        return 9;
                    }
                    if (blueprint.def.entityDefToBuild is ThingDef thingDef && thingDef.IsFence)
                    {
                        return 1;
                    }
                    if (typeof(Building_Door).IsAssignableFrom(thing.def.thingClass))
                    {
                        return 1;
                    }
                }
            }
            return 0;
        }

        public static Rot4 DoorRotationAt(IntVec3 loc, Map map)
        {
            var alignQualityEastWest = 0;
            var alignQualityNorthSouth = 0;
            alignQualityEastWest += AlignQualityAgainst(loc, IntVec3.East, map);
            alignQualityEastWest += AlignQualityAgainst(loc, IntVec3.West, map);
            alignQualityNorthSouth += AlignQualityAgainst(loc, IntVec3.North, map);
            alignQualityNorthSouth += AlignQualityAgainst(loc, IntVec3.South, map);
            if (alignQualityEastWest >= alignQualityNorthSouth)
            {
                return Rot4.North;
            }
            return Rot4.East;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (Faction == Faction.OfPlayer)
            {
                yield return new Command_Toggle
                {
                    defaultLabel = "CommandToggleDoorHoldOpen".Translate(),
                    defaultDesc = "CommandToggleDoorHoldOpenDesc".Translate(),
                    hotKey = KeyBindingDefOf.Misc3,
                    icon = TexCommand.HoldOpen,
                    isActive = () => holdOpenInt,
                    toggleAction = () => holdOpenInt = !holdOpenInt,
                };
            }
        }

        private void ClearReachabilityCache(Map map)
        {
            map.reachability.ClearCache();
            freePassageWhenClearedReachabilityCache = FreePassage;
        }

        private void CheckClearReachabilityCacheBecauseOpenedOrClosed()
        {
            if (Spawned)
            {
                Map.reachability.ClearCacheForHostile(this);
            }
        }
    }
}
