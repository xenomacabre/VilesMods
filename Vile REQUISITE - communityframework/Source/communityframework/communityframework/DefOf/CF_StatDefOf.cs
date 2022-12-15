using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace CF
{
    [DefOf]
    public static class CF_StatDefOf
    {
        public static StatDef CF_CaravanCapacity;

        static CF_StatDefOf() =>
            DefOfHelper.EnsureInitializedInCtor(typeof(CF_StatDefOf));
    }
}
