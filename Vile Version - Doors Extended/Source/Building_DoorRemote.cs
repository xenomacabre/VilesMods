using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace DoorsExpanded
{
    public class Building_DoorRemote : Building_DoorExpanded
    {
        private Building_DoorRemoteButton button;
        private bool securedRemotely = false;

        public Building_DoorRemoteButton Button
        {
            get => button;
            protected set
            {
                if (button != value)
                {
                    var oldButton = button;
                    button = value;
                    oldButton?.Notify_Unlinked(this);
                    button?.Notify_Linked(this);
                }
            }
        }

        public bool SecuredRemotely
        {
            get => securedRemotely;
            set
            {
                securedRemotely = value;
                // Reactive update due to Forbidden depending on SecuredRemotely (via ForcedClosed).
                ForbidUtility.SetForbidden(this, value);
            }
        }

        protected bool OpenInt
        {
            set
            {
                base.openInt = value;
                // Reactive update due to Forbidden depending on OpenInt (via Open via ForcedClosed).
                ForbidUtility.SetForbidden(this, value);
            }
        }

        protected bool HoldOpenInt
        {
            set
            {
                base.holdOpenInt = value;
            }
        }

        // When secured remotely, only care about whether its held open remotely;
        // else can be held open either remotely or by gizmo.
        public bool HoldOpen => securedRemotely ? HoldOpenRemotely : HoldOpenRemotely || base.HoldOpen;

        public bool HoldOpenRemotely => Button is { ButtonOn: true };

        public bool ForcedClosed => SecuredRemotely && !Open;

        public bool Forbidden => ForcedClosed || ForbidUtility.IsForbidden(this, this.Faction);


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref button, nameof(button));
            Scribe_Values.Look(ref securedRemotely, nameof(securedRemotely), false);
        }

        private const float LockPulseFrequency = 1.5f; // OverlayDrawer.PulseFrequency is 4f
        private const float LockPulseAmplitude = 0.7f * 0.6f; // OverlayDrawer.PulseAmplitude is 0.7f
        private const float LockPulseMinAlpha = 0.3f; // 1f - OverlayDrawer.PulseAmplitude (same as vanilla)
        private static readonly float LockDrawY =
            AltitudeLayer.MetaOverlays.AltitudeFor() + Altitudes.AltInc * 6; // from OverlayDrawer.RenderQuestionMarkOverlay

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (ForcedClosed)
            {
                // This is based off OverlayDrawer.RenderQuestionMarkOverlay/RenderPulsingOverlayInternal, with customized parameters.
                drawLoc.y = LockDrawY;
                var sineInput = (Time.realtimeSinceStartup + 397f * (thingIDNumber % 571)) * LockPulseFrequency;
                var alpha = (Mathf.Sin(sineInput) + 1f) * 0.5f;
                alpha = 1 - LockPulseMinAlpha + alpha * LockPulseAmplitude;
                var material = FadedMaterialPool.FadedVersionOf(TexOverlay.LockedOverlay, alpha);
                var drawBatch = OverlayDrawer_drawBatch(Map.overlayDrawer);
                drawBatch.DrawMesh(MeshPool.plane05, Matrix4x4.TRS(drawLoc, Quaternion.identity, Vector3.one), material,
                    layer: 0, renderInstanced: true);
            }
            base.DrawAt(drawLoc, flip);
        }

        private static readonly AccessTools.FieldRef<OverlayDrawer, DrawBatch> OverlayDrawer_drawBatch =
            AccessTools.FieldRefAccess<OverlayDrawer, DrawBatch>("drawBatch");

        public override void DrawExtraSelectionOverlays()
        {
            if (Button is { } button)
                GenDraw.DrawLineBetween(this.TrueCenter(), button.TrueCenter());
            base.DrawExtraSelectionOverlays();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                if (gizmo is Command_Toggle command && command.defaultLabel == "CommandToggleDoorHoldOpen".Translate())
                {
                    // Disable hold open toggle gizmo if secured remotely.
                    if (SecuredRemotely)
                    {
                        gizmo.Disable("PH_RemoteDoorSecuredRemotely".Translate());
                    }
                    yield return gizmo;

                    // Insert all our custom gizmos right after hold open toggle gizmo,
                    // with the secured remotely gizmo being the first.

                    var toggle = new Command_Toggle
                    {
                        defaultLabel = "PH_RemoteDoorSecuredRemotely".Translate(),
                        defaultDesc = "PH_RemoteDoorSecuredRemotelyDesc".Translate(),
                        icon = TexButton.SecuredRemotely,
                        isActive = () => SecuredRemotely,
                        toggleAction = () => SecuredRemotely = !SecuredRemotely,
                    };
                    if (Button is null)
                        toggle.Disable("PH_ButtonNeeded".Translate());
                    if (!DoorPowerOn)
                        toggle.Disable("PH_PowerNeeded".Translate());
                    yield return toggle;

                    yield return new Command_Action
                    {
                        defaultLabel = "PH_ButtonConnect".Translate(),
                        defaultDesc = "PH_ButtonConnectDesc".Translate(),
                        icon = TexButton.ConnectToButton,
                        action = ButtonConnect,
                    };

                    if (Button is not null)
                    {
                        yield return new Command_Action
                        {
                            defaultLabel = "PH_ButtonDisconnect".Translate(),
                            defaultDesc = "PH_ButtonDisconnectDesc".Translate(),
                            icon = TexButton.DisconnectButton,
                            action = ButtonDisconnect,
                        };
                    }
                }
                else
                {
                    yield return gizmo;
                }
            }
        }

        public void Notify_ButtonPushed()
        {
            if (powerComp is CompPowerTrader power && !power.PowerOn)
            {
                Messages.Message("PH_CannotOpenRemotelyWithoutPower".Translate(Label), this, MessageTypeDefOf.RejectInput);
                return;
            }
            UpdateOpenStateFromButtonEvent();
        }

        private void UpdateOpenStateFromButtonEvent()
        {
            if (Button.ButtonOn != Open)
            {
                if (Open)
                {
                    DoorTryClose();
                }
                else
                {
                    DoorOpen();
                }
            }
        }

        private void ButtonConnect()
        {
            var tp = new TargetingParameters
            {
                validator = t => t.Thing is Building_DoorRemoteButton,
                canTargetBuildings = true,
                canTargetPawns = false,
            };
            Find.Targeter.BeginTargeting(tp, t =>
            {
                if (t.Thing is Building_DoorRemoteButton otherButton)
                {
                    if (Button != otherButton)
                    {
                        Button = otherButton;
                        UpdateOpenStateFromButtonEvent();
                    }
                }
                else
                {
                    Messages.Message("PH_ButtonConnectFailed".Translate(t.ToString()), MessageTypeDefOf.RejectInput);
                }
            }, null);
        }

        private void ButtonDisconnect()
        {
            Button = null;
            SecuredRemotely = false;
        }
    }
}
