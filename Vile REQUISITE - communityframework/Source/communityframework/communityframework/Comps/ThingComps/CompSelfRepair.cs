using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace CF
{
    /// <summary>
    /// Automatically repairs a specified hitpoint-based (non-pawn) item.
    /// Example implementation of <see cref="CF.CompWithCheapHashInterval"/>
    /// and designed for use with <see cref="CF.CompFromStuffPatch"/>.
    /// </summary>
    public class CompSelfRepair : CompWithCheapHashInterval
    {   
        /// <summary>
        /// Pre-cast reference to this class's
        /// <seealso cref="CompProperties_SelfhealHitpoints"/>.
        /// </summary>
        CompProperties_SelfRepair Props => (CompProperties_SelfRepair)props;

        /// <summary>
        /// This method is run whenever the <c>ThingComp</c> is ticked. If the
        /// current in-game tick is a "cheap" tick, then the comp's parent
        /// <c>ThingWithComps</c> will regenerate a single hitpoint.
        /// </summary>
        public override void CompTick()
        {
            base.CompTick();
            if (
                IsCheapIntervalTick(Props.tickInterval) &&
                parent.def.useHitPoints &&
                parent.HitPoints < parent.MaxHitPoints
            )
            {
                parent.HitPoints++;
            }
        }


        /// <summary>
        /// This method returns text that is displayed whenever this comp's
        /// parent <c>ThingWithComps</c> is selected in-game. It will display
        /// the number of ticks between each hitpoint gained, but only if
        /// developer mode is turned on.
        /// </summary>
        /// <returns></returns>
        public override string CompInspectStringExtra()
        {
            string ret = base.CompInspectStringExtra();
            if(Prefs.DevMode)
                ret += "CompSelfRepair with TicksPerRepair " +
                    Props.tickInterval;
            return ret;
        }
    }

    /// <summary>
    /// <c>CompProperties</c> for use with <see cref="CF.CompSelfRepair"/>.
    /// Allows specifying the tick interval in XML, and auto-assigns the
    /// appropriate class.
    /// </summary>
    class CompProperties_SelfRepair : CompProperties
    {
        //disable the warning that this field is never assigned to, as the game
        //handles that.
#pragma warning disable CS0649
        /// <summary>
        /// The number of ticks between each hitpoint repaired.
        /// </summary>
        public int tickInterval = 250;
#pragma warning restore CS0649

        /// <summary>
        /// No-arg contructor. Sets <c>compClass</c> to
        /// <seealso cref="CompSelfRepair"/>
        /// </summary>
        public CompProperties_SelfRepair() =>
            compClass = typeof(CompSelfRepair);

        /// <summary>
        /// This method will return a list of configuration errors, notifying
        /// modders that they have set up this <c>CompProperties</c>
        /// incorrectly in their XML. Will print errors if the parent 
        /// <c>ThingWithComps</c> uses a <c>TickerType</c> other than
        /// <c>Normal</c>.
        /// </summary>
        /// <param name="parentDef">
        /// The <c>ThingDef</c> of this comp's parent <c>ThingWithComps</c>.
        /// </param>
        /// <returns>
        /// An <c>IEnumerable</c> of error messages.
        /// </returns>
        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string s in base.ConfigErrors(parentDef)) yield return s;
            if (parentDef.tickerType != TickerType.Normal)
                yield return
                    "CompProperties_SelfRepair: TickerType is not Normal!";
        }
    }
}