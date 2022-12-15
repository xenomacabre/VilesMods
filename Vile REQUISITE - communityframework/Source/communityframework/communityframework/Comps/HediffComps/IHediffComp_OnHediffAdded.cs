using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace CF
{
    /// <summary>
    /// Interface to be implented by a <c>HediffComp</c>. Contains methods that
    /// are run whenever the affected <c>Pawn</c> has a hediff applied to it.
    /// </summary>
    /// <remarks>
    /// Powered by <seealso cref="OnHediffAddedPatch"/>
    /// </remarks>
    public interface IHediffComp_OnHediffAdded
    {
        /// <summary>
        /// Method run whenever the affected <c>Pawn</c> has a new
        /// <c>Hediff</c> applied to it.
        /// </summary>
        /// <param name="hediff">The <c>Hediff</c> that was just added.</param>
        void OnHediffAdded(ref Hediff hediff);
    }
}
