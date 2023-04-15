using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Verse;

namespace BiomesKit
{

    [StaticConstructorOnStartup]
    static class WorldGenStepConstructor
    {
        static WorldGenStepConstructor()
        {
            WorldGenStepDef biomesKitWorldGenStep = new WorldGenStepDef
            {
                defName = "BiomesKitWorldGenStep",
                order = 999,
                worldGenStep = new BiomesKit.LateBiomeWorker()

            };
            DefDatabase<Verse.WorldGenStepDef>.Add(biomesKitWorldGenStep);
        }
    }
}
