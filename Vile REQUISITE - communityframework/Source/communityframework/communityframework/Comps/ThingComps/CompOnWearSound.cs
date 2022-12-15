using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;
using RimWorld;

namespace CF
{
    /// <summary>
    /// The functional component of <see cref="CompProperties_OnWearSound"/>
    /// When applied to an apparel item, it can play a sound whenever the item
    /// is put on or taken off.
    /// </summary>
    class CompOnWearSound : ThingComp
    {
        /// <summary>
        /// Pre-cast reference to this comp's respective <c>CompProperties</c>.
        /// </summary>
        public CompProperties_OnWearSound Props =>
            (CompProperties_OnWearSound)props;

        /// <summary>
        /// Runs after the parent <c>Thing</c> is put on by a <c>Pawn</c>.
        /// Plays the <c>SoundDef</c> stored in <c>Props.wornSound</c>.
        /// </summary>
        /// <param name="pawn">The <c>Pawn</c> putting on the item.</param>
        public override void Notify_Equipped(Pawn pawn)
        {
            if (Props.wornSound != null && pawn.Map == Find.CurrentMap)
            {
                Props.wornSound.PlayOneShot(
                    new TargetInfo(pawn.Position, pawn.Map, false));
            }
            base.Notify_Equipped(pawn);
        }

        /// <summary>
        /// Runs after the parent <c>Thing</c> is taken off of a <c>Pawn</c>.
        /// Plays the <c>SoundDef</c> stored in <c>Props.removedSound</c>.
        /// </summary>
        /// <param name="pawn">The <c>Pawn</c> that the item was removed from.
        /// </param>
        public override void Notify_Unequipped(Pawn pawn)
        {
            if (Props.removedSound != null && pawn.Map == Find.CurrentMap)
            {
                Props.removedSound.PlayOneShot(
                    new TargetInfo(pawn.Position, pawn.Map, false));
            }
            base.Notify_Unequipped(pawn);
        }
    }

    /// <summary>
    /// The properties that control a respective <see cref="CompOnWearSound"/>
    /// When applied to an apparel item, it can play a sound whenever the item
    /// is put on or taken off.
    /// </summary>
    class CompProperties_OnWearSound : CompProperties
    {
        /// <summary>
        /// The sound to play after the item is put on.
        /// </summary>
        public SoundDef wornSound = null;
        /// <summary>
        /// The sound to play after the item is taken off.
        /// </summary>
        public SoundDef removedSound = null;

        /// <summary>
        /// Default constructor, sets <c>compClass</c> to
        /// <c>ThingComp_OnWearSound</c>.
        /// </summary>
        public CompProperties_OnWearSound() =>
            compClass = typeof(CompOnWearSound);

        /// <summary>
        /// Override of method used to report errors made by modders using this
        /// <c>ThingComp</c>. This will inform the modder if they have applied
        /// the <c>ThingComp</c> to something other than an apparel item.
        /// </summary>
        /// <param name="parentDef">The <c>ThingDef</c> of the item that the
        /// <c>ThingComp</c> was applied to.</param>
        /// <returns>A full list of config errors.</returns>
        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            List<string> errors =
                new List<string>(base.ConfigErrors(parentDef));
            if (!parentDef.IsApparel)
            {
                errors.Add("ThingCompProperties_OnWearSound applied to " +
                    "ThingDef " + parentDef.label + ", which is not an " +
                    "apparel item.");
            }
            return errors.AsEnumerable();
        }
    }
}
