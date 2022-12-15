using Verse;

namespace CF
{
    /// <summary>
    /// A <c>HediffComp</c> that allows <c>Hediff</c>s to be removed during the
    /// <c>HealthTick</c> without causing errors.
    /// </summary>
    /// <remarks>
    /// Powered by <seealso cref="MissingPartPatches"/>.
    /// </remarks>
    class HediffComp_ShouldRemove : HediffComp
    {
        /// <summary>
        /// Informs developers if a <c>Hediff</c> is supposed to be removed. If
        /// this text is visible on an active <c>Hediff</c>, something is
        /// wrong.
        /// </summary>
        /// <returns>
        /// Updated debug string.
        /// </returns>
        public override string CompDebugString()
        {
            return base.CompDebugString() +
                "Should be removed next tick.";
        }

        /// <summary>
        /// This method is used to determine if a <c>HediffComp</c>'s parent 
        /// <c>Hediff</c> should be removed when it is safe to do so. It is run
        /// by <c>HediffWithComps.ShouldRemove</c>. Some Hediffs,
        /// like <c>Hediff_MissingPart</c>, override this method and must be
        /// patched.
        /// </summary>
        public override bool CompShouldRemove => true;
    }
}
