//#define PATCH_CALL_REGISTRY

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Xml;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace DoorsExpanded
{
    // This is a separate class from HarmonyPatches in case any other mod wants to patch transpilers in HarmonyPatches.
    // If those transpilers were in the same class, the static constructor would run before any patching of said transpilers
    // could be done, which makes such patching too late.
    [StaticConstructorOnStartup]
    public static class HarmonyPatchesOnStartup
    {
        static HarmonyPatchesOnStartup()
        {
            HarmonyPatches.Patches();
        }
    }

    // TODO: Reorganize this into multiple classes/files and use Harmony attribute-based patch classes.
    public static class HarmonyPatches
    {
        internal static Harmony harmony = new("rimworld.jecrell.doorsexpanded");

        public static void Patches()
        {
            var rwAssembly = typeof(Building_Door).Assembly;

            // Patches for ghost (pre-placement) and blueprints for door expanded.
            foreach (var original in typeof(Designator_Place).FindLambdaMethods(nameof(Designator_Place.DoExtraGuiControls), typeof(void)))
            {
                Patch(original,
                    transpiler: nameof(DoorExpandedDesignatorPlaceRotateAgainIfNeededTranspiler),
                    transpilerRelated: nameof(DoorExpandedRotateAgainIfNeeded));
            }
            Patch(original: AccessTools.Method(typeof(Designator_Place), "HandleRotationShortcuts"),
                transpiler: nameof(DoorExpandedDesignatorPlaceRotateAgainIfNeededTranspiler),
                transpilerRelated: nameof(DoorExpandedRotateAgainIfNeeded));
            Patch(original: AccessTools.Method(typeof(GhostDrawer), nameof(GhostDrawer.DrawGhostThing)),
                transpiler: nameof(DoorExpandedDrawGhostThingTranspiler),
                transpilerRelated: nameof(DoorExpandedDrawGhostGraphicFromDef));
            Patch(original: AccessTools.Method(typeof(GhostUtility), nameof(GhostUtility.GhostGraphicFor)),
                transpiler: nameof(DoorExpandedGhostGraphicForTranspiler));
            Patch(original: AccessTools.Method(typeof(Blueprint), nameof(Blueprint.SpawnSetup)),
                prefix: nameof(DoorExpandedBlueprintSpawnSetupPrefix));
            // Blueprint.Draw no longer exists since RW 1.3+, so we patch Thing.DrawAt, which is called for RealtimeOnly drawerType.
            // We can't just use a custom Blueprint subclass with overriden Draw, since ThingDefGenerator_Buildings hardcodes
            // Blueprint_Install for (re)install blueprints.
            // TODO: Instead of patching this, consider patching ThingDefGenerator_Buildings.NewBlueprintDef_Thing in EarlyPatches to
            // allow custom Blueprint for (re)install blueprints, and using custom Blueprint and Blueprint_Install subclasses
            // (potentially also a Blueprint_Install subclass for Building_Door to keep applying the rotation fix for vanilla doors).
            Patch(original: AccessTools.Method(typeof(Thing), "DrawAt"),
                prefix: nameof(DoorExpandedThingDrawAtPrefix));

            // Patches related to door remotes.
            Patch(original: AccessTools.Method(typeof(FloatMenuMakerMap), "AddJobGiverWorkOrders"),
                transpiler: nameof(DoorRemoteAddJobGiverWorkOrdersTranspiler),
                transpilerRelated: nameof(TranslateCustomizeUseDoorRemoteJobLabel));
        }

        private static void Patch(MethodInfo original, string prefix = null, string postfix = null, string transpiler = null,
            string transpilerRelated = null, int priority = Priority.Normal, string[] before = null, string[] after = null,
            bool? debug = null)
        {
            harmony.Patch(original,
                NewHarmonyMethod(prefix, priority, before, after, debug),
                NewHarmonyMethod(postfix, priority, before, after, debug),
                NewHarmonyMethod(transpiler, priority, before, after, debug));
        }

        private static HarmonyMethod NewHarmonyMethod(string methodName, int priority, string[] before, string[] after, bool? debug)
        {
            if (methodName is null)
                return null;
            return new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatches), methodName), priority, before, after, debug);
        }

        private static IEnumerable<MethodInfo> FindLambdaMethods(this Type type, string parentMethodName, Type returnType,
            Func<MethodInfo, bool> predicate = null)
        {
            // A lambda on RimWorld 1.0 on Unity 5.6.5f1 (mono .NET Framework 3.5 equivalent) is compiled into
            // a CompilerGenerated-attributed non-public inner type with name prefix "<{parentMethodName}>"
            // including an instance method with name prefix "<>".
            // A lambda on RimWorld 1.1 on Unity 2019.2.17f1 (mono .NET Framework 4.7.2 equivalent) is compiled into
            // a CompilerGenerated-attributed non-public inner type with name prefix "<>"
            // including an instance method with name prefix "<{parentMethodName}>".
            // Recent-ish versions of Visual Studio also compile this way.
            // So to be generic enough, return methods of a declaring inner type that:
            // a) either the method or the declaring inner type has name prefix "<{parentMethodName}>".
            // b) has given return type
            // c) satisfies predicate, if given
            var innerTypes = type.GetNestedTypes(AccessTools.all)
                .Where(innerType => innerType.IsDefined(typeof(CompilerGeneratedAttribute)));
            var foundMethod = false;
            foreach (var innerType in innerTypes)
            {
                if (innerType.Name.StartsWith("<" + parentMethodName + ">", StringComparison.Ordinal))
                {
                    foreach (var method in innerType.GetMethods(AccessTools.allDeclared))
                    {
                        if (method.Name.StartsWith("<", StringComparison.Ordinal) &&
                            method.ReturnType == returnType && (predicate is null || predicate(method)))
                        {
                            foundMethod = true;
                            yield return method;
                        }
                    }
                }
                else if (innerType.Name.StartsWith("<", StringComparison.Ordinal))
                {
                    foreach (var method in innerType.GetMethods(AccessTools.allDeclared))
                    {
                        if (method.Name.StartsWith("<" + parentMethodName + ">", StringComparison.Ordinal) &&
                            method.ReturnType == returnType && (predicate is null || predicate(method)))
                        {
                            foundMethod = true;
                            yield return method;
                        }
                    }
                }
            }
            if (!foundMethod)
            {
                throw new ArgumentException($"Could not find any lambda method for type {type} and method {parentMethodName}" +
                    " that satisfies given predicate");
            }
        }

        private static bool IsDoorExpandedDef(Def def) =>
            def is ThingDef thingDef && typeof(Building_DoorExpanded).IsAssignableFrom(thingDef.thingClass);

        private static bool IsVanillaDoorDef(Def def) =>
            def is ThingDef thingDef && typeof(Building_Door).IsAssignableFrom(thingDef.thingClass);


        // Designator_Place.DoExtraGuiControls (internal lambda)
        // Designator_Place.HandleRotationShortcuts
        public static IEnumerable<CodeInstruction> DoorExpandedDesignatorPlaceRotateAgainIfNeededTranspiler(
            IEnumerable<CodeInstruction> instructions)
        {
            // This transforms the following code:
            //  designatorPlace.placingRot.Rotate(rotDirection);
            // into:
            //  designatorPlace.placingRot.Rotate(rotDirection);
            //  DoorExpandedRotateAgainIfNeeded(designatorPlace, ref designatorPlace.placingRot, rotDirection);

            var fieldof_Designator_Place_placingRot = AccessTools.Field(typeof(Designator_Place), "placingRot");
            var methodof_Rot4_Rotate = AccessTools.Method(typeof(Rot4), nameof(Rot4.Rotate));
            var methodof_RotateAgainIfNeeded =
                AccessTools.Method(typeof(HarmonyPatches), nameof(DoorExpandedRotateAgainIfNeeded));
            var instructionList = instructions.AsList();

            var searchIndex = 0;
            var placingRotFieldIndex = instructionList.FindIndex(
                instr => instr.LoadsField(fieldof_Designator_Place_placingRot, byAddress: true));
            while (placingRotFieldIndex >= 0)
            {
                searchIndex = placingRotFieldIndex + 1;
                var rotateIndex = instructionList.FindIndex(searchIndex,
                    instr => instr.Calls(methodof_Rot4_Rotate));
                var nextPlacingRotFieldIndex = instructionList.FindIndex(searchIndex,
                    instr => instr.LoadsField(fieldof_Designator_Place_placingRot, byAddress: true));
                if (rotateIndex >= 0 && (nextPlacingRotFieldIndex < 0 || rotateIndex < nextPlacingRotFieldIndex))
                {
                    var replaceInstructions = new List<CodeInstruction>();
                    // Need copy the Designator_Place instance on top of CIL stack 2 times, in reverse order
                    // (due to stack popping):
                    // (2) placingRot field access for the original Rotate call
                    // (1) placingRot field access for 2nd arg to DoorExpandedRotateAgainIfNeeded call
                    // (0) instance itself for 1st arg to DoorExpandedRotateAgainIfNeeded call
                    replaceInstructions.AddRange(new[]
                    {
                        new CodeInstruction(OpCodes.Dup),
                        new CodeInstruction(OpCodes.Dup),
                    });
                    // Copy original instructions from placingRot field access to Rotate call (uses up (2)).
                    var copiedRotateArgInstructions = instructionList.GetRange(placingRotFieldIndex,
                        rotateIndex - placingRotFieldIndex);
                    replaceInstructions.AddRange(copiedRotateArgInstructions);
                    replaceInstructions.Add(new CodeInstruction(OpCodes.Call, methodof_Rot4_Rotate));
                    // Call DoorExpandedRotateAgainIfNeeded with required arguments.
                    replaceInstructions.AddRange(copiedRotateArgInstructions); // uses up (1)
                    replaceInstructions.Add(new CodeInstruction(OpCodes.Call, methodof_RotateAgainIfNeeded)); // uses up (0)
                    instructionList.SafeReplaceRange(placingRotFieldIndex, rotateIndex - placingRotFieldIndex + 1,
                        replaceInstructions);
                    searchIndex += replaceInstructions.Count - 1;
                    nextPlacingRotFieldIndex = instructionList.FindIndex(searchIndex,
                        instr => instr.LoadsField(fieldof_Designator_Place_placingRot, byAddress: true));
                }
                placingRotFieldIndex = nextPlacingRotFieldIndex;
            }

            return instructionList;
        }

        private static void DoorExpandedRotateAgainIfNeeded(Designator_Place designatorPlace, ref Rot4 placingRot,
              RotationDirection rotDirection)
        {
            // If placingRot is South and rotatesSouth is false, rotate again.
            if (placingRot == Rot4.South && designatorPlace.PlacingDef.GetDoorExpandedProps() is { rotatesSouth: false })
            {
                placingRot.Rotate(rotDirection);
            }
        }

        // GhostDrawer.DrawGhostThing
        public static IEnumerable<CodeInstruction> DoorExpandedDrawGhostThingTranspiler(
            IEnumerable<CodeInstruction> instructions) =>
            instructions.MethodReplacer(
                AccessTools.Method(typeof(Graphic), nameof(Graphic.DrawFromDef)),
                AccessTools.Method(typeof(HarmonyPatches), nameof(DoorExpandedDrawGhostGraphicFromDef)));

        private static void DoorExpandedDrawGhostGraphicFromDef(Graphic graphic, Vector3 loc, Rot4 rot, ThingDef thingDef,
            float extraRotation)
        {
            if (thingDef.GetDoorExpandedProps() is { } doorExProps)
            {
                // Always delegate door expanded graphics to our custom code.
                for (var i = 0; i < 2; i++)
                {
                    Building_DoorExpanded.Draw(thingDef, doorExProps, graphic, loc, rot, openPct: 0, flipped: i != 0);
                    if (doorExProps.singleDoor)
                        break;
                }
            }
            else
            {
                // extraRotation is always 0.
                graphic.DrawFromDef(loc, rot, thingDef, extraRotation);
            }
        }

        // GhostUtility.GhostGraphicFor
        public static IEnumerable<CodeInstruction> DoorExpandedGhostGraphicForTranspiler(
            IEnumerable<CodeInstruction> instructions)
        {
            // One consequence of the patch to ThingDef.IsDoor to include door expanded defs is that
            // GhostUtility.GhostGraphicFor can now return a graphic that our patch for GhostDrawer.DrawGhostThing
            // doesn't work with (it returns a Graphic_Single based off thingDef.uiIconPath rather than Graphic_Multi
            // based off thingDef.graphic). So we need to patch GhostDrawer.DrawGhostThing as well.
            // For door expanded, we always want to return a Graphic_Multi based off thingDef.graphic.

            // This transforms the following code:
            //  if (... || thingDef.IsDoor)
            // into:
            //  if (... || (thingDef.IsDoor && !IsDoorExpandedDef(thingDef))

            var methodof_ThingDef_IsDoor = AccessTools.PropertyGetter(typeof(ThingDef), nameof(ThingDef.IsDoor));
            var instructionList = instructions.AsList();

            var isDoorIndex = instructionList.FindIndex(instr => instr.Calls(methodof_ThingDef_IsDoor));
            // Assume prev instruction is ldarg(.s) or ldloc(.s) for thingDef argument.
            var thingDefLoad = instructionList[isDoorIndex - 1];
            // Assume the next brfalse(.s) operand is a label that skips the Graphic_Single code path.
            var skipGraphicSingleBranchIndex = instructionList.FindIndex(isDoorIndex + 1,
                instr => instr.IsBrfalse());
            var skipGraphicSingleLabel = (Label)instructionList[skipGraphicSingleBranchIndex].operand;
            // Note: Not using SafeInsertRange, since we want labels to stay at skipGraphicSingleBranchIndex + 1.
            instructionList.InsertRange(skipGraphicSingleBranchIndex + 1, new[]
            {
                thingDefLoad.Clone(),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(HarmonyPatches), nameof(IsDoorExpandedDef))),
                new CodeInstruction(OpCodes.Brtrue, skipGraphicSingleLabel),
            });

            return instructionList;
        }

        // Blueprint.SpawnSetup
        public static void DoorExpandedBlueprintSpawnSetupPrefix(Blueprint __instance, Map map)
        {
            ref var blueprint = ref __instance;
            // This needs to be a prefix (as opposed to a postfix), since Thing.SpawnSetup has logic which depends on rotation.
            if (blueprint.def.entityDefToBuild is ThingDef thingDef &&
                thingDef.GetDoorExpandedProps() is { } doorExProps)
            {
                // Historical note: following notes used to be the case, but RW 1.3 changed ThingDefGenerator_Buildings such that
                // blueprint defs now inherit their drawerType from the non-blueprint def (except for the now-redundant Building_Door
                // blueprint case which is always RealTime, even though vanilla doors should already RealTime drawerType).
                // Thus we no longer need to fix drawerType, and now this patch only fixes rotation.

                // ThingDefGenerator_Buildings.NewBlueprintDef_Thing configures generated blueprint defs such that their
                // def.drawerType is MapMeshAndRealTime. This means that they have both a "update-when-needed" drawing that
                // calls the Print method (MapMesh), and an "update-on-tick" drawing that calls the Draw method (RealTime).
                // All our custom graphics for door expanded are done in the Draw method, so we must use RealTimeOnly mode.
                // For build blueprints, it special cases any def with Building_Door thingClass, such that their blueprint's
                // def.drawerType is RealtimeOnly. However, it doesn't special case def.drawerType for (re)install blueprints,
                // since they're not (re)installable by default.
                // We need this special casing for both build and (re)install blueprints of Building_DoorExpanded
                // (which doesn't inherit Building_Door), the latter is needed in case any door expanded are (re)installable.
                // This could be done in a harmony patch that is applied before ThingDefGenerator_Buildings runs
                // (must happen before StaticConstructorOnStartup) and would be more efficient, but it's easier to patch here
                // in SpawnSetup and Draw (see below) and any performance cost is negligible.
                //blueprint.def.drawerType = DrawerType.RealtimeOnly;

                // Non-1x1 rotations change the footprint of the blueprint, so this needs to be done before that footprint
                // is cached in various ways in base.SpawnSetup, including in BlueprintGrid.
                // Fortunately once rotated, no further non-1x1 rotations will change the footprint further.
                blueprint.Rotation =
                    Building_DoorExpanded.DoorRotationAt(thingDef, doorExProps, blueprint.Position, blueprint.Rotation, map);
            }
            else if (blueprint is Blueprint_Install && IsVanillaDoorDef(blueprint.def.entityDefToBuild))
            {
                // Since it's convenient to do so, we'll also "fix" (re)install blueprints for Building_Door thingClass,
                // in case another mod makes them (re)installable.
                blueprint.def.drawerType = DrawerType.RealtimeOnly;
            }
        }

        // ThingWithComps.Draw
        public static bool DoorExpandedThingDrawAtPrefix(Thing __instance)
        {
            if (__instance is Blueprint blueprint)
            {
                if (blueprint.def.entityDefToBuild is ThingDef thingDef &&
                    thingDef.GetDoorExpandedProps() is { } doorExProps)
                {
                    // Always delegate door expanded graphics to our custom code.
                    var drawPos = blueprint.DrawPos;
                    var rotation = blueprint.Rotation;
                    rotation = Building_DoorExpanded.DoorRotationAt(thingDef, doorExProps, blueprint.Position, rotation, blueprint.Map);
                    blueprint.Rotation = rotation;
                    var graphic = blueprint.Graphic;
                    for (var i = 0; i < 2; i++)
                    {
                        Building_DoorExpanded.Draw(thingDef, doorExProps, graphic, drawPos, rotation, openPct: 0, flipped: i != 0);
                        if (doorExProps.singleDoor)
                            break;
                    }
                    Comps_PostDraw(blueprint, emptyObjArray);
                    return false;
                }
                else if (blueprint is Blueprint_Install && IsVanillaDoorDef(blueprint.def.entityDefToBuild))
                {
                    // Since it's convenient to do so, we'll also "fix" (re)install blueprints for Building_Door thingClass,
                    // in case another mod makes them (re)installable.
                    blueprint.Rotation = DoorUtility.DoorRotationAt(blueprint.Position, blueprint.Map, false);
                }
            }
            return true;
        }

        private static readonly FastInvokeHandler Comps_PostDraw =
            MethodInvoker.GetHandler(AccessTools.Method(typeof(ThingWithComps), "Comps_PostDraw"));
        private static readonly object[] emptyObjArray = Array.Empty<object>();


        // FloatMenuMakerMap.AddJobGiverWorkOrders
        public static IEnumerable<CodeInstruction> DoorRemoteAddJobGiverWorkOrdersTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Workaround to remove the "Prioritize" prefix for the remote press/flip job in the float menu.

            // This transforms the following code:
            //  TranslatorFormattedStringExtensions.Translate("PrioritizeGeneric", ...)
            // into:
            //  TranslateRemovePrioritizeJobLabelPrefix("PrioritizeGeneric".Translate(...))

            var methodof_TranslatorFormattedStringExtensions_Translate =
                AccessTools.Method(typeof(TranslatorFormattedStringExtensions), nameof(TranslatorFormattedStringExtensions.Translate),
                    new[] { typeof(string), typeof(NamedArgument), typeof(NamedArgument) });
            var enumerator = instructions.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var instruction = enumerator.Current;
                yield return instruction;
                if (instruction.Is(OpCodes.Ldstr, "PrioritizeGeneric"))
                    break;
            }

            while (enumerator.MoveNext())
            {
                var instruction = enumerator.Current;
                if (instruction.Calls(methodof_TranslatorFormattedStringExtensions_Translate))
                    break;
                if (instruction.IsLdloc())
                    yield return instruction;
            }

            yield return new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(HarmonyPatches), nameof(TranslateCustomizeUseDoorRemoteJobLabel)));

            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        private static TaggedString TranslateCustomizeUseDoorRemoteJobLabel(string translationKey, WorkGiver_Scanner scanner,
            Job job, Thing thing)
        {
            if (scanner is WorkGiver_UseRemoteButton)
                return "PH_UseButtonOrLever".Translate(thing.Label);
            // Following is copied from FloatMenuMakerMap.AddJobGiverWorkOrders.
            return translationKey.Translate(scanner.PostProcessedGerund(job), thing.Label);
        }

    }
}
