using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace CF
{
    /// <summary>
    /// A <c>ThingComp</c> which implements tick distribution to reduce lag
    /// spikes and microstuttering when a large number of instances exist.
    /// </summary>
    /// <remarks>
    /// Only really necessary for comps which are expected to be on large
    /// numbers of <c>Thing</c>s, for example stuffed buildings.
    /// </remarks>
    public abstract class CompWithCheapHashInterval : ThingComp
    {       
        // Offset applied to individual instances of a Thing, used to ensure
        // that each instance isn't running code in the exact same tick.
        // Caching this means we don't have to grab the parent's hash offset
        // every tick.
        private int hashOffset = 0;

        /// <summary>
        /// Method used to determine if the current in-game tick is a targeted
        /// "cheap" tick.
        /// </summary>
        /// <param name="interval">
        /// The number of ticks between each method call.
        /// </param>
        /// <returns>
        /// <c>true</c> if the current tick is a "cheap" tick, <c>false</c> for
        /// any other tick.
        /// </returns>
        public bool IsCheapIntervalTick(int interval) => 
            (Find.TickManager.TicksGame + hashOffset) % interval == 0;

        /// <summary>
        /// Method run whenever the parent <c>ThingWithComps</c> is spawned in.
        /// Sets the hash offset to match the parent's <c>thingIDNumber</c>, so
        /// that seperate instances of the same parent <c>ThingWithComps</c>
        /// won't all run their methods at the exact same tick.  Since 
        /// <c>hashOffset</c> is cached, this allows the comp to run faster
        /// than if it were to call <c>.hashOffset()</c> every tick.
        /// </summary>
        /// <param name="respawningAfterLoad">
        /// Unused in this context.
        /// </param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            hashOffset = parent.thingIDNumber.HashOffset();
        }
    }
}
