using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace CF
{
    /// <summary>
    /// A static helper utility for handling the health of <c>Pawns</c>.
    /// </summary>
    public static class CommunityHealthUtility
    {
        /// <summary>
        /// Represents a side-effect that can be applied upon regeneration,
        /// including the <c>Hediff</c> to apply, the frequency at which it 
        /// occurs, how its severity should be calculated, and whether it
        /// should be applied to the target part or whole body.
        /// </summary>
        public class RegenSideEffect
        {
            /// <summary>
            /// The <c>Hediff</c> that this side-effect will apply.
            /// </summary>
            public HediffDef hediffDef = null;
            /// <summary>
            /// The chance as a decimal percent that this side-effect will be
            /// applied upon successful regeneration.
            /// </summary>
            public float percentChance = 1.0f;
            /// <summary>
            /// Range for the <c>Hediff</c>'s random starting severity.
            /// </summary>
            public FloatRange severity = new FloatRange(0f, 1f);
            /// /// <summary>
            /// If <c>true</c>, the <c>Hediff</c>'s severity will be multiplied
            /// by the cured <c>Hediff</c>'s severity. If it is an injury, it
            /// will be multiplied by the <c>Hediff</c>'s severity as a percent
            /// of the base health.
            /// </summary>
            public bool useInjurySeverityMult = false;
            /// <summary>
            /// If <c>true</c>, the <c>Hediff</c> will be applied to the whole 
            /// body, instead of the cured part.
            /// </summary>
            public bool isGlobalHediff = false;
        }

        /// <summary>
        /// Tries to cure chronic <c>Hediff</c>s, and applies side-effects
        /// randomly if defined.
        /// </summary>
        /// <param name="pawn">
        /// The <c>Pawn</c> that will have hediffs added/removed
        /// </param>
        /// <param name="cause">
        /// The <c>Hediff</c> that is causing regeneration, used to notify the
        /// player, and to ensure that <c>Hediffs</c> cannot cure themselves.
        /// </param>
        /// <param name="blacklist">
        /// List of chronic <c>HediffDef</c>s to ignore. Leave null to ignore
        /// none.
        /// </param>
        /// <param name="whitelist">
        /// <c>List</c> containing the only chronic <c>HediffDefs</c>s that
        /// will be healed. Leave null to accept all chronic <c>HediffDef</c>s.
        /// </param>
        /// <param name="sideEffects">
        /// A <c>List</c> of <c>RegenSideEffect</c>s to randomly apply after
        /// successful regeneration.
        /// </param>
        /// <param name="canHealDestroyed">
        /// If <c>false</c>, injuries that are destroyed parts will be ignored.
        /// </param>
        /// <param name="injuryMode">
        /// Dictates whether <c>Hediff</c>s that use the <c>Hediff_Injury</c>
        /// class should automatically be treated as whitelisted, blacklisted,
        /// or neither.
        /// </param>
        public static void TryHealRandomPermanentWoundFor(
            Pawn pawn,
            HediffWithComps cause,
            List<HediffDef> blacklist = null,
            List<HediffDef> whitelist = null,
            List<RegenSideEffect> sideEffects = null,
            bool canHealDestroyed = false,
            InjuryRegenListMode injuryMode = InjuryRegenListMode.None)
        {
            Random random = new Random(); //RNG utility
            Hediff result; //The hediff to be healed
            //the part and severity of the hediff we've just cured
            BodyPartRecord curedPart;
            float curedSeverity;

            //Try to find a valid hediff that meets all of the parameters, stop
            //execution if none found. The following code block is mostly one
            //search function and its parameters.
            IEnumerable<Hediff> curableHediffs = GetCurablePermanentHediffs(
                pawn, blacklist, whitelist, canHealDestroyed, injuryMode)
                //Hediffs should never be able to cure themselves.
                .Where(hd => hd != cause);
            //If curableHediffs contains empty enumerations, accessing it will
            //throw a null reference exception, so we need to catch that.
            try
            {

                if (curableHediffs == null ||
                    !curableHediffs.TryRandomElement(out result)
                )
                    //If the pawn has no curable Hediffs, stop here.
                    return;
            }
            catch (NullReferenceException)
            {
                return;
            }
            //Throw unexpected exceptions because those are actually a problem.
            catch (Exception e)
            {
                throw e;
            }

            //Save relevant information about the hediff before we remove it.
            //These fields are changed by the process of removing the hediff,
            //so we need to store them now.
            curedPart = result.Part;
            curedSeverity = result.Severity;

            //Cure the original target hediff.
            if (!(result is Hediff_MissingPart))
                //If it's a normal Hediff, do it the normal way.
                TryCureHediffInt(result);
            else
            {
                //if it's a missing part, use the method for missing parts.
                RestorePartInt(pawn, result.Part);
                //forcibly remove the hediff from its part so that we can apply
                //side effects to it.
                result.Part = null;
            }

            //Iterate through each side-effect, then try to apply it to the 
            //cured hediff's body part.
            if (sideEffects != null)
            {
                foreach (RegenSideEffect sideEffect in sideEffects)
                {
                    //If the random roll says the hediff should be applied...
                    if (sideEffect.percentChance >= 1.0f ||
                        sideEffect.percentChance >= (float)random.NextDouble())
                    {
                        //Make the side-effect hediff
                        Hediff newHediff = HediffMaker.MakeHediff(
                            sideEffect.hediffDef,
                            pawn,
                            //Only attach hediff to the cured part if it is not
                            //a global hediff.
                            sideEffect.isGlobalHediff ? null : curedPart);
                        //Set the new hediff's severity to something between 
                        //the minimum and maximum specified severity.
                        float newHediffSeverity = ((float)random.NextDouble() *
                            (sideEffect.severity.max -
                                sideEffect.severity.min)) +
                            sideEffect.severity.min;
                        //If the new hediff's severity is meant to be 
                        //multiplied by the old hediff's severity, do so now.
                        //If the cured part was destroyed, the multiplier is 
                        //ignored, so the full effect is always applied.
                        if (sideEffect.useInjurySeverityMult &&
                            !(result is Hediff_MissingPart))
                        {
                            if (HediffIsInjury(result))
                                newHediffSeverity *=
                                    curedSeverity /
                                    curedPart.def.hitPoints;
                            else
                                newHediffSeverity *= curedSeverity;
                        }

                        newHediff.Severity = newHediffSeverity;
                        pawn.health.AddHediff(newHediff);
                    }
                }
            }

            //Notify the player that an injury has been healed.
            if (!PawnUtility.ShouldSendNotificationAbout(pawn))
                return;
            Messages.Message(
                "MessagePermanentWoundHealed".Translate(
                    cause.LabelCap, pawn.LabelShort, result.LabelCap,
                    pawn.Named("PAWN")
                ),
                pawn,
                MessageTypeDefOf.PositiveEvent);
        }

        /// <summary>
        /// Returns a list of all <c>Hediff</c>s applied to a <c>Pawn</c> that
        /// meet a specified list of criteria.
        /// </summary>
        /// <param name="pawn">
        /// The <c>Pawn</c> whose <c>Hediff</c>s are being checked
        /// </param>
        /// <param name="blacklist"></param>
        /// List of chronic <c>HediffDef</c>s to ignore. Leave null to ignore
        /// none.
        /// <param name="whitelist">
        /// List containing the only chronic <c>HediffDefs</c>s that will be
        /// healed. Leave null to accept all <c>HediffDef</c>s.
        /// </param>
        /// <param name="canHealDestroyed">
        /// If <c>false</c>, injuries that are destroyed parts will be ignored.
        /// </param>
        /// <param name="injuryMode">
        /// Dictates whether <c>Hediff</c>s that use the <c>Hediff_Injury</c>
        /// class should automatically be treated as whitelisted, blacklisted,
        /// or neither.
        /// </param>
        /// <returns>
        /// An <c>IEnumerable</c> containing all of the <c>Hediff</c>s that
        /// meet the specified criteria.
        /// </returns>
        public static IEnumerable<Hediff> GetCurableHediffs(
            Pawn pawn,
            List<HediffDef> blacklist = null,
            List<HediffDef> whitelist = null,
            bool canHealDestroyed = false,
            InjuryRegenListMode injuryMode = InjuryRegenListMode.None)
        {
            IEnumerable<Hediff> result;

            result = pawn.health.hediffSet.hediffs
                //Whitelist and blacklist should allow condition.
                .Where(hd =>
                    IsHediffAllowed(hd, whitelist, blacklist, injuryMode)
                )
                //Injury shouldn't be a destroyed part, unless specified
                //otherwise.
                .Where(hd =>
                    //Always accept injury if it is not a destroyed part, or...
                    !(hd is Hediff_MissingPart) ||
                    //Always accept injury if canHealDestroyed is true.
                    canHealDestroyed
                )
                //Missing parts must have a non-destroyed parent, so 
                //that we don't have problems like armless hands.
                .Where(hd =>
                    !(hd is Hediff_MissingPart) ||
                    !pawn.health.hediffSet.PartIsMissing(hd.Part.parent)
                );

            return result;
        }

        /// <summary>
        /// Returns a list of permanent <c>Hediff</c>s applied to a <c>Pawn</c>
        /// that meet a specified list of criteria.
        /// </summary>
        /// <param name="pawn">
        /// The <c>Pawn</c> whose <c>Hediff</c>s are being checked
        /// </param>
        /// <param name="blacklist"></param>
        /// List of chronic <c>HediffDef</c>s to ignore. Leave null to ignore
        /// none.
        /// <param name="whitelist">
        /// List containing the only chronic <c>HediffDefs</c>s that will be
        /// healed. Leave null to accept all <c>HediffDef</c>s.
        /// </param>
        /// <param name="canHealDestroyed">
        /// If <c>false</c>, injuries that are destroyed parts will be ignored.
        /// </param>
        /// <param name="injuryMode">
        /// Dictates whether <c>Hediff</c>s that use the <c>Hediff_Injury</c>
        /// class should automatically be treated as whitelisted, blacklisted,
        /// or neither.
        /// </param>
        /// <returns>
        /// An <c>IEnumerable</c> containing all of the Permanent
        /// <c>Hediff</c>s that meet the specified criteria.
        /// </returns>
        public static IEnumerable<Hediff> GetCurablePermanentHediffs(
            Pawn pawn,
            List<HediffDef> blacklist = null,
            List<HediffDef> whitelist = null,
            bool canHealDestroyed = false,
            InjuryRegenListMode injuryMode = InjuryRegenListMode.None)
        {
            //Reuse the method for finding non-permanent hediffs
            IEnumerable<Hediff> curableHediffs =
                GetCurableHediffs(
                    pawn, blacklist, whitelist, canHealDestroyed, injuryMode
                );

            //Hediff must be chronic, permanent, or destroyed.
            return curableHediffs.Where(hd =>
                (hd.IsPermanent() || hd.def.chronic ||
                hd is Hediff_MissingPart)
            );
        }

        /// <summary>
        /// Used to check if a <c>Hediff</c> is inherited from
        /// <c>Hediff_Injury</c> or from <c>Hediff_MissingPart</c>. Useful for
        /// treating injuries and missing parts equally.
        /// </summary>
        /// <param name="hediff">
        /// The <c>Hediff</c> to check.
        /// </param>
        /// <param name="careIfAddedAncestor">If true, missing parts will not
        /// be counted as an injury if any ancestor has any ancestor with
        /// "added parts", i.e. bionics.</param>
        /// <returns>
        /// <c>true</c>, if the specified <c>Hediff</c> is some form of injury
        /// or missing part.
        /// </returns>
        public static bool HediffIsInjury(
            Hediff hediff,
            bool careIfAddedAncestor = false) =>
            hediff is Hediff_Injury ||
            hediff is Hediff_MissingPart &&
            (!careIfAddedAncestor ||
            // If we care about ancestors with added parts, then we check for
            // that here.
            !hediff.pawn.health.hediffSet.
                PartOrAnyAncestorHasDirectlyAddedParts(hediff.Part));

        /// <summary>
        /// Recursively restores each part of a given limb.
        /// </summary>
        /// <param name="pawn">
        /// The <c>Pawn</c> who owns the part.
        /// </param>
        /// <param name="part">
        /// The <c>BodyPartRecord</c> of the part to restore.
        /// </param>
        /// <param name="alreadyWarned">
        /// If <c>true</c>, the player will not be warned about possible errors
        /// caused by unsafely restoring parts during <c>HealthTick</c>.
        /// </param>
        private static void RestorePartRecursiveInt(
            Pawn pawn,
            BodyPartRecord part,
            ref bool alreadyWarned)
        {
            //hediff list must be instantiated, because the original list will
            //be modified by this process
            List<Hediff> hediffs =
                new List<Hediff>(pawn.health.hediffSet.hediffs);
            foreach (HediffWithComps hd in hediffs)
            {
                if (hd.Part == part && !hd.def.keepOnBodyPartRestoration)
                {
                    //Mark Hediff for removable if possible
                    alreadyWarned = TryCureHediffInt(hd, alreadyWarned);
                }
            }
            foreach (BodyPartRecord child in part.parts)
            {
                RestorePartRecursiveInt(pawn, child, ref alreadyWarned);
            }
        }

        /// <summary>
        /// Recursively restores each part of a given limb.
        /// </summary>
        /// <param name="pawn">
        /// The <c>Pawn</c> who owns the part.
        /// </param>
        /// <param name="part">
        /// The <c>BodyPartRecord</c> of the part to restore.
        /// </param>
        private static void RestorePartRecursiveInt(
            Pawn pawn,
            BodyPartRecord part)
        {
            bool warn = false;
            RestorePartRecursiveInt(pawn, part, ref warn);
        }

        /// <summary>
        /// Used to initiate a recursive restoration of a specified part. When
        /// done, it marks the <c>Pawn</c>'s health records to be re-cached.
        /// </summary>
        /// <param name="pawn">
        /// The <c>Pawn</c> who owns the part.
        /// </param>
        /// <param name="part">
        /// The <c>BodyPartRecord</c> of the part to restore.
        /// </param>
        private static void RestorePartInt(
            Pawn pawn,
            BodyPartRecord part
            )
        {
            if (part == null)
            {
                ULog.Error(
                    "ConfigurableRegenUtility: Tried to restore null part");
                return;
            }
            RestorePartRecursiveInt(pawn, part);
            pawn.health.hediffSet.DirtyCache();
        }

        /// <summary>
        /// Used to safely cure <c>Hediff</c>s during <c>HealthTick</c> using
        /// <c>HediffComp_ShouldRemove</c>, so as not to cause index array
        /// exceptions due to a shrinking <c>Hediff</c> list. If the
        /// <c>Hediff</c> does not have <c>HediffComp</c>s, it won't be
        /// possible to cure it the safe way, so a warning will be provided to
        /// the player.
        /// </summary>
        /// <param name="hediff">
        /// The <c>Hediff</c> to be cured.
        /// </param>
        /// <param name="alreadyWarned">
        /// If <c>true</c>, the player will not be warned about possible errors
        /// caused by unsafely restoring parts during <c>HealthTick</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if a warning was issued to the player, <c>false</c>
        /// otherwise.
        /// </returns>
        private static bool TryCureHediffInt(
            Hediff hediff,
            bool alreadyWarned)
        {
            //If the Hediff uses comps, mark it for safe removal.
            if (hediff is HediffWithComps withComps)
            {
                withComps.comps.Add(new HediffComp_ShouldRemove());
            }
            //If the Hediff does not use comps, we'll have to do it the dirty
            //way. We'll also inform the player, so as not to cause alarm.
            else
            {
                if (!alreadyWarned)
                    ULog.Warning("ConfigurableRegenUtility: " +
                        "Attempting to cure Hediff during HealthTick. " +
                        "This may cause a harmless error.");
                hediff.pawn.health.RemoveHediff(hediff);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Used to safely cure <c>Hediff</c>s during <c>HealthTick</c> using
        /// <c>HediffComp_ShouldRemove</c>, so as not to cause index array
        /// exceptions due to a shrinking <c>Hediff</c> list. If the
        /// <c>Hediff</c> does not have <c>HediffComp</c>s, it won't be
        /// possible to cure it the safe way, so a warning will be provided to
        /// the player.
        /// </summary>
        /// <param name="hediff">
        /// The <c>Hediff</c> to be cured.
        /// </param>
        private static void TryCureHediffInt(Hediff hediff)
        {
            TryCureHediffInt(hediff, false);
        }

        /// <summary>
        /// Method for determining if a given <c>Hediff</c> matches a given
        /// whitelist, blacklist, or both.
        /// </summary>
        /// <remarks>
        /// Bear in mind that, while it is
        /// possible to use both types of lists, doing so will lead to
        /// unexpected behavior. If an automatic injury list is used, however,
        /// you can override it by adding specific injuries to the opposite
        /// list.
        /// </remarks>
        /// <param name="hediff">
        /// The <c>Hediff</c> to check against the lists.
        /// </param>
        /// <param name="whitelist">
        /// A <c>List</c> of <c>Hediff</c>s that will only be accepted.
        /// </param>
        /// <param name="blacklist">
        /// A <c>List</c> of <c>Hediff</c>s that will never be accepted.
        /// </param>
        /// <param name="mode">
        /// The automatic injury listing mode, if any.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <c>Hediff</c> fits into the specified lists,
        /// <c>false</c> otherwise.
        /// </returns>
        public static bool IsHediffAllowed(
            Hediff hediff,
            List<HediffDef> whitelist = null,
            List<HediffDef> blacklist = null,
            InjuryRegenListMode mode = InjuryRegenListMode.None)
        {
            //Never accept Hediffs that are explicitly blacklisted, even if
            //it's an injury and auto-whitelist is on.
            if (blacklist != null && blacklist.Contains(hediff.def))
                return false;
            //If any whitelist is being used...
            if (whitelist != null || mode == InjuryRegenListMode.Whitelist)
            {
                //Always accept Hediffs that are explicitly whitelisted.
                //Checking the manual whitelist before the auto-blacklist means
                //we can exclude specific injuries from the auto-blacklist.
                if (whitelist != null && whitelist.Contains(hediff.def))
                    return true;
                else
                {
                    //Account for auto-whitelisted injuries.
                    if (mode == InjuryRegenListMode.Whitelist &&
                        HediffIsInjury(hediff, true))
                        return true;
                }
                //If any whitelist is in use, but neither whitelist accepts the
                //injury, then the injury is not acceptable.
                return false;
            }
            //Don't allow any injuries that have made it this far if injuries
            //are auto-blacklisted.
            if (mode == InjuryRegenListMode.Blacklist &&
                HediffIsInjury(hediff, true))
                return false;
            return true;
        }

        /// <summary>
        /// Calculates the amount of pain that a given <c>Pawn</c> is
        /// experiencing right now, with or without health-induced multipliers.
        /// </summary>
        /// <param name="pawn">
        /// The <c>Pawn</c> whose pain is being checked
        /// </param>
        /// <param name="usePainFactor">
        /// If <c>true</c>, the final value will account for <c>Hediff</c>s
        /// that multiply the <c>Pawn</c>'s total pain level. Set to
        /// <c>false</c> for the "raw" value.
        /// </param>
        /// <param name="careIfMech">
        /// If this value is <c>true</c>, and the given <c>Pawn</c> is not made
        /// of flesh (for example, if it is a mechanoid), then this method will
        /// always return 0 because robots have no feelings.
        /// </param>
        /// <returns
        /// >A <c>float</c> representing the current amount of pain that the
        /// given <c>Pawn</c> should be in. Does not include a maximum value.
        /// </returns>
        public static float CalculatePawnIntendedPain(
            Pawn pawn,
            bool usePainFactor=true,
            bool careIfMech=true)
        {
            if ((!pawn.RaceProps.IsFlesh && careIfMech) || pawn.Dead)
                return 0.0f;
            float num = 0.0f;
            foreach (Hediff h in pawn.health.hediffSet.hediffs)
                num += h.PainOffset;
            if (usePainFactor)
            {
                foreach (Hediff h in pawn.health.hediffSet.hediffs)
                    num *= h.PainFactor;
            }
            return num;
        }

        /// <summary>
        /// <c>InjuryRegenListMode</c> is used to dictate whether a 
        /// <c>ThingComp</c> or <c>HediffComp</c> that uses 
        /// <c>CommunityHealthUtility</c> should automatically treat
        /// <c>HediffDef</c>s with the <c>Hediff_Injury</c> class as
        /// whitelisted or blacklisted, or if they should be treated normally
        /// (<c>None</c>).
        /// </summary>
        public enum InjuryRegenListMode
        {
            /// <summary>
            /// Injuries should be treated as any other <c>Hediff</c>.
            /// </summary>
            None,
            /// <summary>
            /// Injuries should never be accepted, unless explicitly
            /// whitelisted.
            /// </summary>
            Blacklist,
            /// <summary>
            /// Injuries should always be accepted, unless explicitly
            /// blacklisted.
            /// </summary>
            Whitelist,
        }
    }
}
