using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace CF
{
    class CompRemoteTrigger : ThingComp
    {
        public CompProperties_RemoteTrigger Props => (CompProperties_RemoteTrigger)props;
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            // Check if graphicPath returns an actual texture.
            if (!ContentFinder<Texture2D>.Get(Props.texPath))
            {
                ULog.Error("No Gizmo texture found");
            }

            return base.CompGetGizmosExtra().Append(new Command_Action
            {
                defaultLabel = "Trigger",
                defaultDesc = $"Trigger {this.parent.def.defName}",
                icon = ContentFinder<Texture2D>.Get(Props.texPath),
                action = delegate
                {
                    Thing toDetonate = base.parent;
                    CompExplosive cE = toDetonate.TryGetComp<CompExplosive>();
                    if (!(cE?.wickStarted ?? true)) cE.StartWick();
                }
            });          
        }
    }
    class CompProperties_RemoteTrigger : CompProperties
    {
        public CompProperties_RemoteTrigger() => this.compClass = typeof(CompRemoteTrigger);
#pragma warning disable CS0649
        public string texPath;
#pragma warning restore CS0649
    }
}
