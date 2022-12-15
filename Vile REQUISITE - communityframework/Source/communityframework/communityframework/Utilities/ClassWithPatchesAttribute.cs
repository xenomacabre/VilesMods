using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CF
{
    /// <summary>
    /// Used to make adding new Harmony patches internally easier. Not intended
    /// for other mods' use.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    internal class ClassWithPatchesAttribute : Attribute
    {
        /// <summary>
        /// Store the name translation key, Scribe save key, and description
        /// translation key, respectively, of the annotated Harmony patch.
        /// </summary>
        public string NameKey, SaveKey, DescKey;

        /// <summary>
        /// Constructor, takes a single <c>string</c> as an argument. Sets up
        /// the values of <c>NameKey</c>, <c>SaveKey</c>, and <c>DescKey</c>.
        /// </summary>
        /// <param name="saveKey">
        /// The string used to refer to the annotated patch's respective mod
        /// setting during Scribe reads and writes. Also used to generate the
        /// translation keys for the mod setting.
        /// </param>
        public ClassWithPatchesAttribute(string saveKey)
        {
            SaveKey = saveKey;
            NameKey = $"{CFSettings.KeyPrefix}{saveKey}_Name";
            DescKey = $"{CFSettings.KeyPrefix}{saveKey}_Desc";
        }
    }
}
