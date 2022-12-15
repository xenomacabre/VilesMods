using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CF
{
    /// <summary>
    /// <c>ModSettings</c> class for Community Framework. Mainly handles which
    /// Harmony patches should be applied and saves all specified settings.
    /// </summary>
    /// <remarks>Static for convenience.</remarks>
    public class CFSettings : ModSettings
    {
        public static readonly string KeyPrefix = "CFSettings_";
        public static readonly string NamePostfix = "_Name";
        public static readonly string DescPostfix = "_Desc";
        public static bool DEBUG = false; //for release set false by default
        public static bool PrintPatchedMethods => DEBUG && printPatchedMethods;
        public static bool printPatchedMethods = false;

        // despite being public, please don't access these. Access patch
        // application settings with ShouldPatch.
        // They're only public so they can be used in the mod settings screen.
        public static Dictionary<string, PatchSave> Patches =
            new Dictionary<string, PatchSave>();

        public class PatchSave : IExposable
        {
            public bool apply;
            public string saveKey;

            // The game requires a no-arg constructor
            public PatchSave() { }

            public PatchSave(string s, bool a)
            {
                saveKey = s;
                apply = a;
            }

            public override string ToString()
            {
                return "<{" + saveKey + ": " + apply + "}>";
            }

            public void ExposeData()
            {
                Scribe_Values.Look(ref saveKey, "saveKey");
                Scribe_Values.Look(ref apply, "apply");
            }
        } 

        public override void ExposeData()
        {
            base.ExposeData();
            List<PatchSave> savePatches = new List<PatchSave>();
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                savePatches = SerializePatches();     
            }
            Scribe_Collections.Look(ref savePatches, "Patches", LookMode.Deep);
            if(Scribe.mode != LoadSaveMode.Saving)
            {
                DeserializePatches(savePatches);
            }
            Scribe_Values.Look(ref DEBUG, "debug", false);
            Scribe_Values.Look(ref printPatchedMethods, "printPatchedMethods");
        }

        public static bool ShouldPatch(string patchkey)
        {
            if (!DEBUG) return true;
            if (!Patches.ContainsKey(patchkey))
            {
                ULog.Warning(
                    "ShouldPatch called for non-initialized patchkey.");
                return true;
            }
            return Patches[patchkey].apply;
        }

        public static List<PatchSave> SerializePatches()
        {
            List<PatchSave> ret = new List<PatchSave>();
            foreach (string key in Patches.Keys.ToList())
            {
                if (HarmonyLoader.SaveKeyExists(key))
                    ret.Add(new PatchSave(key, Patches[key].apply));
            }
            return ret;
        }

        public static void DeserializePatches(List<PatchSave> list)
        {
            foreach (PatchSave pi in list)
            {
                if (HarmonyLoader.SaveKeyExists(pi.saveKey))
                    Patches[pi.saveKey] = pi;
            }
        }
    }
    /// <summary>
    /// <c>Mod</c> class for Community Framework. Mainly handles the settings
    /// screen.
    /// </summary>
    public class CFMod : Mod
    {
        CFSettings settings;
        public CFMod(ModContentPack con) : base(con)
        {
            this.settings = GetSettings<CFSettings>();
            new HarmonyLoader();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.CheckboxLabeled(
                $"{CFSettings.KeyPrefix}Debug".Translate(), 
                ref CFSettings.DEBUG,
                $"{CFSettings.KeyPrefix}DebugTooltip".Translate()
            );
            if (CFSettings.DEBUG)
            {
                listing.CheckboxLabeled(
                    $"{CFSettings.KeyPrefix}PPM".Translate(),
                    ref CFSettings.printPatchedMethods, 
                    $"{CFSettings.KeyPrefix}PPMTooltip".Translate());
                listing.Label(
                    $"{CFSettings.KeyPrefix}ApplyAtOwnRisk".Translate());
                listing.Label(
                    $"{CFSettings.KeyPrefix}RestartToApply".Translate());
                listing.Label(
                    $"{CFSettings.KeyPrefix}DebugModeRequired".Translate());

                List<CFSettings.PatchSave> patches =
                    CFSettings.SerializePatches();
                foreach(CFSettings.PatchSave pi in patches)
                {
                    listing.CheckboxLabeled(
                        $"{CFSettings.KeyPrefix}ApplyPatch".Translate(
                            NameKeyOf(pi.saveKey).Translate()),
                        ref pi.apply,
                        $"{CFSettings.KeyPrefix}ApplyPatchTooltip".Translate(
                            NameKeyOf(pi.saveKey).Translate(),
                            DescKeyOf(pi.saveKey).Translate()
                        )
                    );
                }
                CFSettings.DeserializePatches(patches);
            }
            listing.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "CFSettings_Category".Translate();
        }

        private static string NameKeyOf(string patchKey) =>
            CFSettings.KeyPrefix + patchKey + CFSettings.NamePostfix;

        private static string DescKeyOf(string patchKey) =>
            CFSettings.KeyPrefix + patchKey + CFSettings.DescPostfix;
    }
}
