using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using static HarmonyLib.Code;

namespace DoorsExpanded
{
    /// <summary>
    ///
    /// Building_DoorExpanded
    ///
    /// What: Originally, a class for multi-celled, larger and more complicated doors.
    /// RimWorld 1.5 officially implemented a Building_MultiTileDoor class. This class is
    /// somewhat obsolete and exists as an extension of MultiTileDoor. Previous versions of
    /// RimWorld required a complete new class.
    /// 
    /// There is still value in the Expanded class, as it allows for swinging doors, stretching doors,
    /// and is also extended into remote controlled doors.
    ///
    /// </summary>
    public class Building_DoorExpanded : Building_MultiTileDoor
    {

        private CompProperties_DoorExpanded props;
        private CompForbiddable forbiddenComp;
        private const float VisualDoorOffsetStart = 0f;
        internal const float VisualDoorOffsetEnd = 0.45f;
        public virtual bool Forbidden => forbiddenComp?.Forbidden ?? false;

        public CompProperties_DoorExpanded Props =>
    props ??= def.GetDoorExpandedProps() ?? throw new Exception("Missing " + typeof(CompProperties_DoorExpanded));


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            TLog.Log(this);
            // Non-1x1 rotations change the footprint of the blueprint, so this needs to be done before that footprint is
            // cached in various ways in base.SpawnSetup.
            // Fortunately once rotated, no further non-1x1 rotations will change the footprint further.
            // Restricted rotation logic in both patched Designator_Place and Blueprint shouldn't allow invalid rotations by
            // this point, but better safe than sorry, especially if this is spawned without Designator_Place or Blueprint.
            Rotation = DoorRotationAt(def, Props, Position, Rotation, map);

            base.SpawnSetup(map, respawningAfterLoad);


            powerComp = GetComp<CompPowerTrader>();
            forbiddenComp = GetComp<CompForbiddable>();
            ForbidUtility.SetForbidden(this,Forbidden);
            ClearReachabilityCache(map);
            if (BlockedOpenMomentary)
            {
                DoorOpen();
            }
        }

        public static Rot4 DoorRotationAt(ThingDef def, CompProperties_DoorExpanded props, IntVec3 loc, Rot4 rot, Map map)
        {
            if (!def.rotatable)
            {
                var size = def.Size;
                if ((size.x == 1 && size.z == 1) || props.doorType is DoorType.StretchVertical or DoorType.Stretch)
                    rot = DoorUtility.DoorRotationAt(loc, map, false);
            }
            if (!props.rotatesSouth && rot == Rot4.South)
                rot = Rot4.North;
            return rot;
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            drawLoc.y = AltitudeLayer.DoorMoveable.AltitudeFor();
            var rotation = DoorRotationAt(def, Props, Position, Rotation, Map);
            Rotation = rotation;
            var openPct = OpenPct;

            for (var i = 0; i < 2; i++)
            {
                var flipped = i != 0;
                var graphic = (!flipped && Props.doorAsync is { } doorAsyncGraphic)
                    ? doorAsyncGraphic.GraphicColoredFor(this)
                    : Graphic;
                Draw(def, Props, graphic, drawLoc, rotation, openPct, flipped, null);
                    //i == 0 && DebugViewSettingsMoreWrite.writeDoors ? debugDrawVectors : null);
                graphic.ShadowGraphic?.DrawWorker(drawLoc, rotation, def, this, 0f);
                if (props.singleDoor)
                    break;
            }

            if (props.doorFrame is not null)
            {
                DrawFrameParams(def, Props, drawLoc, rotation, false, out var fMesh, out var fMatrix);
                Graphics.DrawMesh(fMesh, fMatrix, props.doorFrame.GraphicColoredFor(this).MatAt(rotation), 0);

                if (props.doorFrameSplit is not null)
                {
                    DrawFrameParams(def, Props, drawLoc, rotation, true, out fMesh, out fMatrix);
                    Graphics.DrawMesh(fMesh, fMatrix, props.doorFrameSplit.GraphicColoredFor(this).MatAt(rotation), 0);
                }
            }

            Comps_PostDraw();
        }

        internal static void Draw(ThingDef def, CompProperties_DoorExpanded props,
            Graphic graphic, Vector3 drawPos, Rot4 rotation, float openPct,
            bool flipped, DebugDrawVectors drawVectors = null)
        {
            Mesh mesh;
            Quaternion rotQuat;
            Vector3 offsetVector, scaleVector;
            switch (props.doorType)
            {
                // There's no difference between Stretch and StretchVertical except for stretchOpenSize's default value.
                case DoorType.Stretch:
                case DoorType.StretchVertical:
                    DrawStretchParams(def, props, rotation, openPct, flipped,
                        out mesh, out rotQuat, out offsetVector, out scaleVector);
                    break;
                case DoorType.DoubleSwing:
                    // TODO: Should drawPos.y be set to Mathf.Max(drawPos.y, AltitudeLayer.BuildingOnTop.AltitudeFor())
                    // since AltitudeLayer.DoorMoveable is only used to hide sliding doors behind adjacent walls?
                    DrawDoubleSwingParams(def, props, drawPos, rotation, openPct, flipped,
                        out mesh, out rotQuat, out offsetVector, out scaleVector);
                    break;
                default:
                    DrawStandardParams(def, props, rotation, openPct, flipped,
                        out mesh, out rotQuat, out offsetVector, out scaleVector);
                    break;
            }
            var graphicVector = drawPos + offsetVector;
            var matrix = Matrix4x4.TRS(graphicVector, rotQuat, scaleVector);
            Graphics.DrawMesh(mesh, matrix, graphic.MatAt(rotation), layer: 0);

            if (drawVectors is not null)
            {
                drawVectors.openPct = openPct;
                drawVectors.offsetVector = offsetVector;
                drawVectors.scaleVector = scaleVector;
                drawVectors.graphicVector = graphicVector;
            }
        }

        private static void DrawStretchParams(ThingDef def, CompProperties_DoorExpanded props,
            Rot4 rotation, float openPct, bool flipped, out Mesh mesh, out Quaternion rotQuat,
            out Vector3 offsetVector, out Vector3 scaleVector)
        {
            var drawSize = def.graphicData.drawSize;
            var closeSize = props.stretchCloseSize;
            var openSize = props.stretchOpenSize;
            var offset = props.stretchOffset.Value;

            var verticalRotation = rotation.IsHorizontal;
            var persMod = verticalRotation && props.fixedPerspective ? 2f : 1f;

            offsetVector = new Vector3(offset.x * openPct * persMod, 0f, offset.y * openPct * persMod);

            var scaleX = Mathf.LerpUnclamped(openSize.x, closeSize.x, 1 - openPct) / closeSize.x * drawSize.x * persMod;
            var scaleZ = Mathf.LerpUnclamped(openSize.y, closeSize.y, 1 - openPct) / closeSize.y * drawSize.y * persMod;
            scaleVector = new Vector3(scaleX, 1f, scaleZ);

            // South-facing stretch animation should have same vertical direction as north-facing one.
            if (rotation == Rot4.South)
                offsetVector.z = -offsetVector.z;

            if (!flipped)
            {
                mesh = MeshPool.plane10;
            }
            else
            {
                offsetVector.x = -offsetVector.x;
                mesh = MeshPool.plane10Flip;
            }

            rotQuat = rotation.AsQuat;
            offsetVector = rotQuat * offsetVector;
        }

        private static void DrawDoubleSwingParams(ThingDef def, CompProperties_DoorExpanded props,
            Vector3 drawPos, Rot4 rotation, float openPct, bool flipped, out Mesh mesh, out Quaternion rotQuat,
            out Vector3 offsetVector, out Vector3 scaleVector)
        {
            var verticalRotation = rotation.IsHorizontal;
            if (!flipped)
            {
                offsetVector = new Vector3(-1f, 0f, 0f);
                if (verticalRotation)
                    offsetVector = new Vector3(1.4f, 0f, 1.1f);
                mesh = MeshPool.plane10;
            }
            else
            {
                offsetVector = new Vector3(1f, 0f, 0f);
                if (verticalRotation)
                    offsetVector = new Vector3(-1.4f, 0f, 1.1f);
                mesh = MeshPool.plane10Flip;
            }

            if (verticalRotation)
                rotQuat = Quaternion.AngleAxis(rotation.AsAngle + (openPct * (flipped ? 90f : -90f)), Vector3.up);
            else
                rotQuat = rotation.AsQuat;
            offsetVector = rotQuat * offsetVector;

            var offsetMod = ((VisualDoorOffsetStart + props.doorOpenMultiplier * openPct) * def.Size.x);
            offsetVector *= offsetMod;

            if (verticalRotation)
            {
                if (!flipped && rotation == Rot4.East
                    || flipped && rotation == Rot4.West)
                {
                    offsetVector.y = Mathf.Max(0f, AltitudeLayer.BuildingOnTop.AltitudeFor() - drawPos.y);
                }
            }

            var drawSize = def.graphicData.drawSize;
            var persMod = verticalRotation && props.fixedPerspective ? 2f : 1f;
            scaleVector = new Vector3(drawSize.x * persMod, 1f, drawSize.y * persMod);
        }

        private static void DrawStandardParams(ThingDef def, CompProperties_DoorExpanded props,
            Rot4 rotation, float openPct, bool flipped, out Mesh mesh, out Quaternion rotQuat,
            out Vector3 offsetVector, out Vector3 scaleVector)
        {
            var verticalRotation = rotation.IsHorizontal;
            if (!flipped)
            {
                offsetVector = new Vector3(-1f, 0f, 0f);
                mesh = MeshPool.plane10;
            }
            else
            {
                offsetVector = new Vector3(1f, 0f, 0f);
                mesh = MeshPool.plane10Flip;
            }

            rotQuat = rotation.AsQuat;
            offsetVector = rotQuat * offsetVector;

            var offsetMod = (VisualDoorOffsetStart + props.doorOpenMultiplier * openPct) * def.Size.x;
            offsetVector *= offsetMod;

            var drawSize = def.graphicData.drawSize;
            var persMod = verticalRotation && props.fixedPerspective ? 2f : 1f;
            scaleVector = new Vector3(drawSize.x * persMod, 1f, drawSize.y * persMod);
        }


        private static void DrawFrameParams(ThingDef def, CompProperties_DoorExpanded props,
            Vector3 drawPos, Rot4 rotation, bool split,
            out Mesh mesh, out Matrix4x4 matrix)
        {
            var verticalRotation = rotation.IsHorizontal;
            var offsetVector = new Vector3(-1f, 0f, 0f);
            mesh = MeshPool.plane10;

            if (props.doorFrameSplit is not null)
            {
                if (rotation == Rot4.West)
                {
                    offsetVector.x = 1f;
                    //offsetVector.z *= -1f;
                    //offsetVector.y *= -1f;
                }
            }

            var rotQuat = rotation.AsQuat;
            offsetVector = rotQuat * offsetVector;

            var offsetMod = (VisualDoorOffsetStart + props.doorOpenMultiplier * 1f) * def.Size.x;
            offsetVector *= offsetMod;

            var drawSize = props.doorFrame.drawSize;
            var persMod = verticalRotation && props.fixedPerspective ? 2f : 1f;
            var scaleVector = new Vector3(drawSize.x * persMod, 1f, drawSize.y * persMod);

            var graphicVector = drawPos;
            graphicVector.y = AltitudeLayer.Blueprint.AltitudeFor();
            if (rotation == Rot4.North || rotation == Rot4.South)
                graphicVector.y = AltitudeLayer.PawnState.AltitudeFor();
            if (!verticalRotation)
                graphicVector.x += offsetMod;
            if (rotation == Rot4.East)
            {
                graphicVector.z -= offsetMod;
                if (split)
                    graphicVector.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
            }
            else if (rotation == Rot4.West)
            {
                graphicVector.z += offsetMod;
                if (split)
                    graphicVector.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
            }
            graphicVector += offsetVector;

            var frameOffsetVector = props.doorFrameOffset;
            if (props.doorFrameSplit is not null)
            {
                if (rotation == Rot4.West)
                {
                    rotQuat = Quaternion.Euler(0f, 270f, 0f);
                    graphicVector.z -= 2.7f;
                    mesh = MeshPool.plane10Flip;
                    frameOffsetVector = props.doorFrameSplitOffset;
                }
            }
            graphicVector += frameOffsetVector;

            matrix = Matrix4x4.TRS(graphicVector, rotQuat, scaleVector);
        }

        private void ClearReachabilityCache(Map map)
        {
            map.reachability.ClearCache();
            //freePassageWhenClearedReachabilityCache = FreePassage;
        }

        // This exists to expose draw vectors for debugging purposes.
        internal class DebugDrawVectors
        {
            public float openPct;
            public Vector3 offsetVector, scaleVector, graphicVector;
        }

        internal DebugDrawVectors debugDrawVectors = new();
    }
}
