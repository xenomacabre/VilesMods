using RimWorld;
using Verse;

namespace DoorsExpanded
{
    [DefOf]
    public static class HeronDefOf
    {
        public static JobDef PH_UseRemoteButton;

        static HeronDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(HeronDefOf));
    }
}
