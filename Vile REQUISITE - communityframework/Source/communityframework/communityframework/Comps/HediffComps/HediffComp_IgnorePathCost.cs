using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CF
{
	/// <summary>
	/// A <c>HediffComp</c> used to create <c>Hediff</c>s that allow their
	/// affected <c>Pawn</c> to pass over any terrain type while ignoring the
	/// path cost. This <c>HediffComp</c> doesn't do much on its own, and only
	/// exits to be detected by a Harmony patch.
	/// </summary>
	/// <remarks>
	/// Powered by <seealso cref="IgnorePathCostPatch"/>.
	/// </remarks>
	class HediffComp_IgnorePathCost : HediffComp
    {
		/// <summary>
		/// Additional text displayed when the parent <c>Hediff</c> is
		/// mouse-highlighted in-game.
		/// </summary>
		public override string CompTipStringExtra
		{
			get
			{
				//English: "Terrain path cost is ignored"
				return "CF_IgnorePathCost_StringExtra".Translate();
			}
		}
	}
}
