using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace CF {
    /// <summary>
    /// Minifies or destroys the parent <c>Thing</c> if it wouldn't be allowed
    /// to be placed at its current location. Must be used with one or more
    /// <c>PlaceWorker</c>s.
    /// </summary>
    public class CompValidator : CompWithCheapHashInterval
    {
        /// <summary>
        /// Pre-cast reference to this comp's <c>CompProperties_Validator</c>.
        /// </summary>
        CompProperties_Validator Props => (CompProperties_Validator)props;

        /// <summary>
        /// Called every tick. Every <c>props.tickInterval</c> ticks, this
        /// method will check if its parent is allowed to be where it is
        /// according to its place workers, and will minify or destroy it if
        /// not.
        /// </summary>
        public override void CompTick()
        {
            base.CompTick();

            if (!Props.ShouldUse || !IsCheapIntervalTick(Props.tickInterval))
                return;

            foreach(PlaceWorker pw in parent.def.PlaceWorkers)
            {
                if (
                    !pw.AllowsPlacing(
                        parent.def,
                        parent.Position,
                        parent.Rotation,
                        parent.Map
                    ).Accepted
                )
                {
                    parent.MinifyOrDestroy();
                    break;
                }
            }
        }

        /// <summary>
        /// This method is used to display information on the in-game status
        /// panel. If the game is running in developer mode, this method will
        /// give the number of place workers, and the <c>ToString</c> of the
        /// first 3 place workers found.
        /// </summary>
        /// <returns></returns>
        public override string CompInspectStringExtra()
        {
            string ret = base.CompInspectStringExtra();
            if (Prefs.DevMode)
            {
                ret += "PlaceWorkers: (count = " +
                    parent.def.PlaceWorkers.Count + "):";
                for (
                    int i = 0;
                    i < Math.Min(3, parent.def.PlaceWorkers.Count);
                    i++
                )
                    ret +="\n\t" + parent.def.PlaceWorkers.ElementAt(i)
                        .ToString();
            }
            return ret;
        }

        
    }
    /// <summary>
    /// <c>CompProperties</c> for use with <see cref="CF.CompValidator"/>.
    /// Allows specifying the tick interval in XML, auto-assigns the
    /// appropriate class, and disables the comp if misconfigured.
    /// </summary>
    public class CompProperties_Validator : CompProperties
    {
        public int tickInterval = 250;

        /// <value>
        /// Whether the validator should run its checks in-game. True by
        /// default, unless it's misconfigured, in which case checks will not
        /// run.
        /// </value>
        public bool ShouldUse { get; private set; }

        /// <summary>
        /// No-arg constructor , sets the value of <c>compClass</c> to the type
        /// of <c>CompValidator</c>.
        /// </summary>
        public CompProperties_Validator() => compClass = typeof(CompValidator);

        /// <summary>
        /// Returns an <c>IEnumerable</c> of strings, each describing an error
        /// that the XML modder made while setting up this comp properties. In
        /// this case, it will return strings if the parent
        /// <c>ThingWithComps</c> does not have any <c>PlaceWorker</c>s set, or
        /// if its ticker type is not <c>Normal</c>.
        /// </summary>
        /// <param name="parentDef">Not used in this override.</param>
        /// <returns>Strings containing any errors caused by bad XML.</returns>
        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            if (
                parentDef.placeWorkers == null ||
                parentDef.placeWorkers.Count < 1
            )
            {
                ShouldUse = false;
                yield return "CompProperties_Validator: no PlaceWorkers set!";
            }
            if (parentDef.tickerType != TickerType.Normal)
                yield return "CompProperties_Validator: " +
                    "TickerType is not Normal!";
        }
    }
}
