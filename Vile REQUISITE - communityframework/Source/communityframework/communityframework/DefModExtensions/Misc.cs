using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CF
{
    // These extensions are just for holding data, and none of these fields are
    // going to be assigned to in this namespace.
#pragma warning disable CS0649

    /// <summary>
    /// <c>DefModExtension</c> for use with
    /// <see cref="CF.ShouldHaveNeedPatch"/>. Needs listed here will be ignored
    /// by pawns with this <c>DefModExtension</c>.
    /// </summary>
    public class IgnoreNeed : DefModExtension
    {
        public List<NeedDef> needs;
    }

    /// <summary>
    /// <c>DefModExtension</c> which flags the parent plant <c>ThingDef</c> as
    /// using negative fertility. Specifies minimum and maximum fertility
    /// values within which the final fertility is clamped.
    /// </summary>
    /// <remarks>
    /// See <see cref="CF.NegativeFertilityPatch"/> for implementation details.
    /// </remarks>
    public class UseNegativeFertility : DefModExtension
    {
        public float minFertility = 0.05f, maxFertility = 1.4f;
    }

    /// <summary>
    /// <c>DefModExtension</c> for use with
    /// <see cref="CF.CompFromStuffPatch"/>. Specifies the <c>ThingComps</c>,
    /// by their <c>CompProperties</c>, which should be added to newly-
    /// generated items made from the specified Stuff.
    /// </summary>
    public class CompsToAddWhenStuff : DefModExtension
    {
        public List<CompProperties> comps;

    }

    /// <summary>
    /// An extension meant for use alongside
    /// <see cref="RimWorld.CompFacility"/>, it is meant to be used on
    /// buildings that link to other buildings.
    /// </summary>
    public class BuildingFacilityExtension : DefModExtension
    {
        public bool facilityRequiresFuel = false;
    }

    /// <summary>
    /// An extension used by <see cref="Verse.RecipeDef"/>. It contains a list
    /// of <see cref="OutputWorker"/>s to run when the parent recipe is
    /// complete.
    /// </summary>
    class UseOutputWorkers : DefModExtension
    {
        /// <summary>
        /// A collection of non-initiated <see cref="OutputWorker"/>s to run
        /// when the parent recipe is completed.
        /// </summary>
        public IEnumerable<Type> outputWorkers;

        /// <summary>
        /// Instances of the <see cref="OutputWorker"/>s used by this
        /// extension. Stored so that their methods can be easily called when
        /// needed.
        /// </summary>
        [Unsaved(false)]
        private IEnumerable<OutputWorker> activeWorkers = null;

        /// <summary>
        /// Returns a list of instances of <see cref="OutputWorkers"/> used by
        /// this extension. If the workers haven't been instanced yet, this
        /// property will first create the necessary instances.
        /// </summary>
        public IEnumerable<OutputWorker> ActiveWorkers
        {
            get
            {
                if (activeWorkers != null)
                    return activeWorkers;

                activeWorkers = new List<OutputWorker>();
                foreach (Type t in outputWorkers)
                    activeWorkers.Append(
                        (OutputWorker)Activator.CreateInstance(t)
                    );

                return activeWorkers;
            }
        }
    }

    public class HatcherExtension : DefModExtension
    {
        public bool hatcheeForcePlayerFaction = false;
    }
#pragma warning restore CS0649
}
