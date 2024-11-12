// Note: This is copied from JecsTools.

using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace JecsTools
{
    public class PlaceWorker_Outline : PlaceWorker
    {
        private static readonly Color transparentWhite = new(1f, 1f, 1f, 0f);

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            var drawFieldCells = new List<IntVec3>();
            foreach (var c in GenAdj.CellsOccupiedBy(center, rot, def.size))
                drawFieldCells.Add(c);
            GenDraw.DrawFieldEdges(drawFieldCells, Color.Lerp(ghostCol, transparentWhite, 0.5f));
        }
    }
}
