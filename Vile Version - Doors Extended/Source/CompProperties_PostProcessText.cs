using System.Collections.Generic;
using Verse;

namespace DoorsExpanded
{
    public class CompPostProcessText : ThingComp
    {
        public CompProperties_PostProcessText Props => (CompProperties_PostProcessText)props;
    }

    // ThingDefs with this comp will have their label and description updated after translation def injection.
    public class CompProperties_PostProcessText : CompProperties
    {
        public ThingDef defaultLabelAndDescriptionFrom;
        public bool appendSizeToLabel;

        [Unsaved]
        public string origLabel;

        [Unsaved]
        public string baseLabel;

        [Unsaved]
        private bool finalized = false;

        public static readonly Dictionary<ThingDef, CompProperties_PostProcessText> defToComp = new();

        public CompProperties_PostProcessText()
        {
            compClass = typeof(CompPostProcessText);
        }

        public override void ResolveReferences(ThingDef parentDef)
        {
            // This method can be called multiple times per instance, so ensure idempotency.
            if (finalized)
                return;

            defToComp[parentDef] = this;

            origLabel = parentDef.label;
            // This logic can't be done in PostLoadSpecial due to needing defaultLabelAndDescriptionFrom needing to be resolved.
            if (defaultLabelAndDescriptionFrom is not null)
            {
                var otherPostProcessText = defaultLabelAndDescriptionFrom.GetCompProperties<CompProperties_PostProcessText>();
                otherPostProcessText?.ResolveReferences(defaultLabelAndDescriptionFrom);
                if (parentDef.label.NullOrEmpty())
                {
                    var newLabel = otherPostProcessText?.baseLabel ?? defaultLabelAndDescriptionFrom.label;
                    if (TLog.Enabled)
                        TLog.Log(this, $"{parentDef}.label: {QuoteString(parentDef.label)} => {QuoteString(newLabel)}");
                    parentDef.label = newLabel;
                }
                if (parentDef.description.NullOrEmpty())
                {
                    var newDescription = defaultLabelAndDescriptionFrom.description;
                    if (TLog.Enabled)
                        TLog.Log(this, $"{parentDef}.description: {QuoteString(parentDef.description)} => {QuoteString(newDescription)}");
                    parentDef.description = newDescription;
                }
            }
            baseLabel = parentDef.label;
            if (appendSizeToLabel)
            {
                var size = parentDef.Size;
                var newLabel = $"{parentDef.label} ({size.x}x{size.z})";
                if (TLog.Enabled)
                    TLog.Log(this, $"{parentDef}.label: {QuoteString(parentDef.label)} => {QuoteString(newLabel)}");
                parentDef.label = newLabel;
            }
            finalized = true;
        }

        internal static string QuoteString(string str) => str is null ? "null" : $"\"{str.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
    }

    [StaticConstructorOnStartup]
    public static class PostProcessTextOnStartup
    {
        static PostProcessTextOnStartup()
        {
            // Update frames and blueprints that were implicitly generated for defs with CompProperties_PostProcessText.
            // These are generated before ResolveReferences are ever called.
            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.entityDefToBuild is ThingDef entityDef &&
                    CompProperties_PostProcessText.defToComp.TryGetValue(entityDef, out var postProcessTextComp))
                {
                    var newLabel = entityDef.label +
                        (postProcessTextComp.origLabel.NullOrEmpty() ? def.label : def.label.Substring(postProcessTextComp.origLabel.Length));
                    if (TLog.Enabled)
                        TLog.Log(typeof(PostProcessTextOnStartup), string.Format("{0}.label: {1} => {2}",
                            def, CompProperties_PostProcessText.QuoteString(def.label), CompProperties_PostProcessText.QuoteString(newLabel)));
                    def.label = newLabel;
                }
            }

            // Comps add overhead, so since we're done, remove the comps now that they're no longer needed.
            foreach (var (def, postProcessTextComp) in CompProperties_PostProcessText.defToComp)
            {
                if (TLog.Enabled)
                    TLog.Log(typeof(PostProcessTextOnStartup), $"Removing {postProcessTextComp} from {def}");
                def.comps.Remove(postProcessTextComp);
            }
            CompProperties_PostProcessText.defToComp.Clear();
        }
    }
}
