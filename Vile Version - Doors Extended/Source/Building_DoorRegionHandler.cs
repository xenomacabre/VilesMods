using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace DoorsExpanded
{
    /// <summary>
    ///
    /// Door Region Handler
    ///
    /// Now deprecated code. This will exist only to destroy previously existing invisible doors.
    /// Previously this was used in RimWorld as a way to have a door spread across as many tiles as needed.
    /// A 3x5 door would simply be filled with invisible doors that acted all at once.
    /// 
    /// As this is no longer necessary, previously saved DoorRegionHandlers will now simply delete after
    /// their first tick.
    ///
    /// </summary>
    [Obsolete("No longer in use. RimWorld 1.5 introduced the MultiTileDoor and Invisible Doors inside " +
        "larger frames are no longer needed.")]
    public class Building_DoorRegionHandler : Building_Door
    {
        public override void Tick()
        {
            Log.Warning($"{this} remains from RimWorld v1.4 - destroying this to remain in-line with new" +
                "RW 1.5 MultiTileDoor code for DoorsExpanded");
            Destroy();
            return;
        }
    }
}
