using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace DoorsExpanded
{
    public class CompDoorExpanded : ThingComp
    {
        public CompProperties_DoorExpanded Props => (CompProperties_DoorExpanded)props;
    }

    public class CompProperties_DoorExpanded : CompProperties
    {
        public const float DefaultStretchPercent = 0.2f;

        public bool remoteDoor = false;
        public DoorType doorType = DoorType.Standard;
        public bool fixedPerspective = false;
        public bool singleDoor = false;
        public bool rotatesSouth = true;
        public float tempEqualizeRate = TemperatureTuning.Door_TempEqualizeRate;
        public float doorOpenMultiplier = Building_DoorExpanded.VisualDoorOffsetEnd;
        public GraphicData doorFrame;
        public Vector3 doorFrameOffset;
        public GraphicData doorFrameSplit;
        public Vector3 doorFrameSplitOffset;
        public GraphicData doorAsync;

        // Following properties are only relevant for Stretch/StretchVertical doors.
        // The size of a closed door, relative to stretchOpenSize.
        // This is typically the "actual" size ignoring transparent sections of the texture.
        // Like DrawPos, its origin point is assumed to be at the center of the rectangle.
        public Vector2 stretchCloseSize;
        // Size to stretch (typically shrink) to for an opened door, relative to stretchCloseSize.
        // Defaults to DefaultStretchPercent * x or y of stretchCloseSize, depending on Stretch or StretchVertical.
        public Vector2 stretchOpenSize;
        // Offset from stretchCloseSize's center to stretchOpenSize's center.
        // Supposing north-facing door and stretch size shrinks, default to offsetting left (-x) and up (+y),
        // such the left/up side looks like it hasn't moved.
        public Vector2? stretchOffset;

        public CompProperties_DoorExpanded()
        {
            compClass = typeof(CompDoorExpanded);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var error in base.ConfigErrors(parentDef))
                yield return error;

            foreach (var comp in parentDef.comps)
            {
                if (comp != this && comp is CompProperties_DoorExpanded)
                    yield return $"contains multiple {typeof(CompProperties_DoorExpanded)}s";
            }

            if (parentDef.category is not ThingCategory.Building)
                yield return $"{this} must have category {ThingCategory.Building}";
            if (parentDef.tickerType is not TickerType.Normal)
                yield return $"{this} must have tickerType {TickerType.Normal}";
            if (parentDef.drawerType is not DrawerType.RealtimeOnly)
                yield return $"{this} must have drawerType {DrawerType.RealtimeOnly}";
        }

        public override void ResolveReferences(ThingDef parentDef)
        {
            parentDef.thingClass = remoteDoor ? typeof(Building_DoorRemote) : typeof(Building_DoorExpanded);

            if (typeof(Building_DoorRemote).IsAssignableFrom(parentDef.thingClass))
            {
                var remoteDoorDescription = "PH_RemoteDoor".Translate();
                if(parentDef.description is null)
                {
                    parentDef.description = remoteDoorDescription;
                }
                else if(!parentDef.description.Contains(remoteDoorDescription))
                {
                    parentDef.description += " " + remoteDoorDescription;
                }
            }

            base.ResolveReferences(parentDef);

            // See comments regarding stretch property defaults in the fields above.
            if (parentDef.graphicData is { } graphicData && doorType is DoorType.Stretch or DoorType.StretchVertical)
            {
                if (stretchCloseSize == Vector2.zero)
                    stretchCloseSize = graphicData.drawSize;
                if (stretchOpenSize == Vector2.zero)
                {
                    if (doorType is DoorType.Stretch)
                        stretchOpenSize = new Vector2(stretchCloseSize.x * DefaultStretchPercent, stretchCloseSize.y);
                    else
                        stretchOpenSize = new Vector2(stretchCloseSize.x, stretchCloseSize.y * DefaultStretchPercent);
                }
                stretchOffset ??= new Vector2(
                    (stretchOpenSize.x - stretchCloseSize.x) / 2,
                    (stretchCloseSize.y - stretchOpenSize.y) / 2);
                //Log.Message($"Stretch door {defName} properties:\n" +
                //    $"- stretchCloseSize: {stretchCloseSize}\n" +
                //    $"- stretchOpenSize: {stretchOpenSize}\n" +
                //    $"- stretchOffset: {stretchOffset}");
            }

            if (parentDef.building is { relatedBuildCommands: null or { Count: 0 } } buildingProps)
            {
                cachedDoorRemoteDefs ??= DefDatabase<ThingDef>.AllDefsListForReading
                    .FindAll(def => typeof(Building_DoorRemoteButton).IsAssignableFrom(def.thingClass));
                buildingProps.relatedBuildCommands = cachedDoorRemoteDefs;
            }

        }

        [ThreadStatic]
        private static List<ThingDef> cachedDoorRemoteDefs;
    }

    // Convenience extensions methods for getting CompProperties_DoorExpanded
    public static class CompProperties_DoorExpanded_Extensions
    {
        public static CompProperties_DoorExpanded GetDoorExpandedProps(this Def def) =>
            def is ThingDef thingDef ? thingDef.GetDoorExpandedProps() : null;

        // Avoiding ThingDef.GetCompProperties<T> and implementing a specific non-generic version of it here.
        // That method is slow because the `isinst` instruction with generic type arg operands is very slow,
        // while `isinst` instruction against non-generic type operand like used below is fast.
        public static CompProperties_DoorExpanded GetDoorExpandedProps(this ThingDef def)
        {
            var allProps = def.comps;
            for (int i = 0, count = allProps.Count; i < count; i++)
            {
                if (allProps[i] is CompProperties_DoorExpanded props)
                    return props;
            }
            return null;
        }
    }
}
