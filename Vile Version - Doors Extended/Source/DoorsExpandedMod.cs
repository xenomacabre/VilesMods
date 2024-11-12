using UnityEngine;
using Verse;

namespace DoorsExpanded
{
    public class DoorsExpandedSettings : ModSettings
    {
        public TLogLevel logLevel;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref logLevel, nameof(logLevel), TLogLevel.Normal);
        }
    }

    public class DoorsExpandedMod : Mod
    {
        public static DoorsExpandedSettings Settings { get; private set; }

        public DoorsExpandedMod(ModContentPack content) : base(content)
        {
            //HarmonyPatches.EarlyPatches(); Removed in 1.5
            Settings = GetSettings<DoorsExpandedSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            Text.Font = GameFont.Medium;
            var labelKey = "DoorsExpanded_" + nameof(Settings.logLevel);
            listing.Label(labelKey.Translate());
            Text.Font = GameFont.Small;
            foreach (TLogLevel logLevel in typeof(TLogLevel).GetEnumValues())
            {
                if (listing.RadioButton($"{labelKey}_{logLevel}".Translate(), Settings.logLevel == logLevel, tabIn: 10f))
                    Settings.logLevel = logLevel;
            }
            listing.End();
        }

        public override string SettingsCategory()
        {
            return "DoorsExpanded".Translate();
        }
    }
}
