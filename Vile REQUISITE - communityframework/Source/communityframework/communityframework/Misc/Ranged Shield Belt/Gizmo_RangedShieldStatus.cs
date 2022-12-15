using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace CF
{
    /// <summary>
    /// <c>Gizmo</c> which displays the current energy state of the <see cref="CF.RangedShieldBelt"/>. Identical to the vanilla one, except references the aforementioned class since it's no longer a subclass.
    /// </summary>
    [StaticConstructorOnStartup]
    class Gizmo_RangedShieldStatus : Gizmo
    {
        #region misc properties
        public RangedShieldBelt shield;

        private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));
        private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public Gizmo_RangedShieldStatus()
        {   
            Order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }
        #endregion misc properties

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect outerRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect innerRect = outerRect.ContractedBy(6f);
            Widgets.DrawWindowBackground(outerRect);
            Rect labelRect = innerRect;
            labelRect.height = innerRect.height / 2f;
            Text.Font = GameFont.Tiny;
            Widgets.Label(labelRect, shield.LabelCap);
            Rect rect4 = innerRect;
            rect4.yMin = innerRect.y + innerRect.height / 2f;
            float fillPercent = shield.Energy / Mathf.Max(1f, shield.GetStatValue(StatDefOf.EnergyShieldEnergyMax));
            Widgets.FillableBar(rect4, fillPercent, FullShieldBarTex, EmptyShieldBarTex, doBorder: false);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect4, $"{(shield.Energy * 100f):F0} / {(shield.GetStatValue(StatDefOf.EnergyShieldEnergyMax) * 100f):F0}");
            Text.Anchor = TextAnchor.UpperLeft;
            return new GizmoResult(GizmoState.Clear);
        }
    }
}
