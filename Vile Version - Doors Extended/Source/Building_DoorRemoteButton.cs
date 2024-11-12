using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.Sound;

namespace DoorsExpanded
{
    public class Building_DoorRemoteButton : Building
    {
        private List<Building_DoorRemote> linkedDoors = new();
        private CompPowerTrader powerComp;
        private bool buttonOn = false;
        private bool needsToBeSwitched = false;

        public List<Building_DoorRemote> LinkedDoors
        {
            get
            {
                RemoveNullOrUnspawnedDoors();
                return linkedDoors;
            }
        }

        public bool ButtonOn
        {
            get => buttonOn;
            set => buttonOn = value;
        }

        public bool NeedsToBeSwitched
        {
            get => needsToBeSwitched;
            set => needsToBeSwitched = value;
        }

        // base.Graphic is off; fullGraveGraphicData.Graphic is on.
        public override Graphic Graphic => !ButtonOn ? base.Graphic : def.building.fullGraveGraphicData.Graphic;

        public bool NeedsPower => powerComp is { PowerOn: false };

        private void RemoveNullOrUnspawnedDoors()
        {
            if (TLog.Enabled)
            {
                var nullOrUnspawnedDoors = linkedDoors.FindAll(door => door is not { Spawned: true });
                if (nullOrUnspawnedDoors.Count > 0)
                    TLog.Log(this, $"{this}: removing null or unspawned linkedDoors: " +
                        nullOrUnspawnedDoors.ToStringSafeEnumerable());
            }
            linkedDoors.RemoveAll(door => door is not { Spawned: true });
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            var linkedDoors = LinkedDoors;
            for (var i = 0; i < linkedDoors.Count; i++)
            {
                GenDraw.DrawLineBetween(this.TrueCenter(), linkedDoors[i].TrueCenter());
            }
        }

        public void Notify_Linked(Building_DoorRemote remoteDoor)
        {
            // ButtonOn state is set to the first linked door's HoldOpen state.
            // From that point on, the relationship is reversed such that all linked door's HoldOpen state are synchronized
            // with this button's ButtonOn state.
            var linkedDoors = LinkedDoors;
            if (linkedDoors.Count == 0)
                ButtonOn = remoteDoor.HoldOpen;
            linkedDoors.Add(remoteDoor);
        }

        public void Notify_Unlinked(Building_DoorRemote remoteDoor)
        {
            var linkedDoors = LinkedDoors;
            linkedDoors.Remove(remoteDoor);
            if (linkedDoors.Count == 0)
                ButtonOn = false;
        }

        public void PushButton()
        {
            if (NeedsToBeSwitched)
                NeedsToBeSwitched = false;
            SoundDefOf.Tick_Tiny.PlayOneShot(this);
            ButtonOn = !ButtonOn;
            var linkedDoors = LinkedDoors;
            foreach (var linkedDoor in linkedDoors)
                linkedDoor.Notify_ButtonPushed();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
                yield return gizmo;

            var toggle = new Command_Toggle
            {
                defaultLabel = "PH_UseButtonOrLever".Translate(def.label),
                defaultDesc = "PH_UseButtonOrLeverDesc".Translate(def.label),
                icon = TexButton.UseButtonOrLever,
                isActive = () => NeedsToBeSwitched,
                toggleAction = () => NeedsToBeSwitched = !NeedsToBeSwitched,
            };
            if (IsDisabled(out var reason))
                toggle.Disable(reason);
            yield return toggle;
        }

        public bool IsDisabled(out string reason)
        {
            var linkedDoors = LinkedDoors;
            if (linkedDoors.Count == 0)
            {
                reason = "PH_UseButtonOrLeverNoConnection".Translate();
                return true;
            }
            else if (NeedsPower)
            {
                reason = "PH_UseButtonOrLeverNoPower".Translate();
                return true;
            }
            reason = null;
            return false;
        }

        public bool IsDisabledForJob(bool forced, out string reason)
        {
            if (!(forced || NeedsToBeSwitched))
            {
                reason = null;
                return true;
            }
            return IsDisabled(out reason);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode is LoadSaveMode.Saving)
                RemoveNullOrUnspawnedDoors();
            Scribe_Collections.Look(ref linkedDoors, nameof(linkedDoors), LookMode.Reference);
            Scribe_Values.Look(ref needsToBeSwitched, nameof(needsToBeSwitched), false);
            Scribe_Values.Look(ref buttonOn, nameof(buttonOn), false);
            if (TLog.Enabled)
                TLog.Log(this, $"{Scribe.mode}: {this} (linkedDoors={linkedDoors.ToStringSafeEnumerable()})");
        }
    }
}
