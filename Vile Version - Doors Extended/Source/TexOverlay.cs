using UnityEngine;
using Verse;

namespace DoorsExpanded
{
    [StaticConstructorOnStartup]
    internal static class TexOverlay
    {
        public static readonly Material LockedOverlay =
            MaterialPool.MatFrom("UI/Overlays/DE_Secured", ShaderDatabase.MetaOverlay);
    }
}
