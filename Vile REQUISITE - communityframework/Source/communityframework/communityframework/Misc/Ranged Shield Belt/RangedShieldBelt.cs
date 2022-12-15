using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.Sound;

namespace CF
{
    /// <summary>
    /// <c>ThingClass</c> which acts like the vanilla <c>ShieldBelt</c> class but allows the user to fire ranged weapons while worn.
    /// </summary>
    /// <remarks>
    /// Used to be a pretty simple subclass of <c>ShieldBelt</c>, but that ran into compatibility problems:
    /// <list type="bullet">
    /// <item>Some mods check whether ranged shots were allowed by seeing if the thingClass <c>is</c> a ShieldBelt, which returned true in this case, 
    /// causing errors including them being automatically, erroneously, unequipped on ranged pawns.</item>
    /// <item>A growing number of patches were necessary to prevent the vanilla game treating ranged shield belts in the same way.</item>
    /// </list>
    /// </remarks>
    [StaticConstructorOnStartup]
    public class RangedShieldBelt : Apparel
    {
        #region fields
#pragma warning disable CS0649 // "will always be null" from visible code (managed by reflected code)
#pragma warning disable CS0169 // "never used" from visible code (managed by reflected code)
#pragma warning disable CS0414 // "assigned to but never used" from visible code (managed by reflected code)
        private float energy;
        // todo: get all these values by reflection (should:tm: be way easier than the stubs thing)
        private int ticksToReset = -1;
        private int lastKeepDisplayTick = -9999;
        private Vector3 impactAngleVect;
        private int lastAbsorbDamageTick = -9999;
        private const float MinDrawSize = 1.2f;
        private const float MaxDrawSize = 1.55f;
        private const float MaxDamagedJitterDist = 0.05f;
        private const int JitterDurationTicks = 8;
        private int StartingTicksToReset = 3200;
        private float EnergyOnReset = 0.2f;
        private float EnergyLossPerDamage = 0.033f;
        private int KeepDisplayingTicks = 1000;
        private float ApparelScorePerEnergyMax = 0.25f;
        private static readonly Material BubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);
        #endregion fields
        #region properties
        private float EnergyMax => this.GetStatValue(StatDefOf.EnergyShieldEnergyMax);
        private float EnergyGainPerTick => this.GetStatValue(StatDefOf.EnergyShieldRechargeRate) / 60f;
        public float Energy => energy;
        public bool ShieldIsResetting => ticksToReset > 0;
        private bool ShouldDisplay
        {
            get
            {
                Pawn wearer = base.Wearer;
                if (!wearer.Spawned || wearer.Dead || wearer.Downed) return false;
                if (wearer.InAggroMentalState || wearer.Drafted || wearer.Faction.HostileTo(Faction.OfPlayer) && !wearer.IsPrisoner) return true;
                if (Find.TickManager.TicksGame < lastKeepDisplayTick + KeepDisplayingTicks) return true;
                return false;
            }
        }
        #endregion properties
        /// <summary>
        /// The only meaningful change here, allowing ranged verb casts (or, more precisely, not disallowing them as the original <c>ShieldBelt</c> does).
        /// </summary>
        /// <param name="v">Disregarded.</param>
        /// <returns>True.</returns>
        public override bool AllowVerbCast(Verb v) => true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref energy, nameof(energy));
            Scribe_Values.Look(ref ticksToReset, nameof(ticksToReset), -1);
            Scribe_Values.Look(ref lastKeepDisplayTick, nameof(lastKeepDisplayTick));
        }
        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            foreach (Gizmo wornGizmo in base.GetWornGizmos()) yield return wornGizmo;
            if(Find.Selector.SingleSelectedThing == base.Wearer) yield return new Gizmo_RangedShieldStatus() { shield = this };
        }
        public override float GetSpecialApparelScoreOffset() => EnergyMax * ApparelScorePerEnergyMax;
        public override void Tick()
        {
            base.Tick();
            if (Wearer is null)
            {
                energy = 0f;
            }
            else if(ShieldIsResetting && --ticksToReset <= 0)
            {
                Reset();
            }
            else if((energy += EnergyGainPerTick) > EnergyMax)
            {
                energy = EnergyMax;
            }
        }
        public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
        {
            if (ShieldIsResetting) return false;
            if(dinfo.Def == DamageDefOf.EMP)
            {
                Break();
                return false;
            }
            if(dinfo.Def.isRanged || dinfo.Def.isExplosive)
            {
                energy -= dinfo.Amount * EnergyLossPerDamage;
                if (energy < 0f) Break();
                else AbsorbedDamage(dinfo);
                return true;
            }
            return false;
        }
        public void KeepDisplaying()
        {
            lastKeepDisplayTick = Find.TickManager.TicksGame;
        }
        private void AbsorbedDamage(DamageInfo dinfo)
        {
            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(Wearer.Position, Wearer.Map));
            impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            Vector3 loc = Wearer.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
            float num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
            FleckMaker.Static(loc, Wearer.Map, FleckDefOf.ExplosionFlash, num);
            for(int i = 0; i < num; i++)
            {
                FleckMaker.ThrowDustPuff(loc, Wearer.Map, Rand.Range(0.5f, MinDrawSize));
            }
            lastAbsorbDamageTick = Find.TickManager.TicksGame;
            KeepDisplaying();
        }
        private void Break()
        {
            SoundDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(Wearer.Position, Wearer.Map));
            FleckMaker.Static(Wearer.TrueCenter(), Wearer.Map, FleckDefOf.ExplosionFlash, 12f);
            for(int i = 0; i < 6; i++)
            {
                FleckMaker.ThrowDustPuff(Wearer.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f),
                    Wearer.Map, Rand.Range(0.8f, 1.2f));
            }
        }
        private void Reset()
        {
            if(Wearer.Spawned)
            {
                SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(Wearer.Position, Wearer.Map));
                FleckMaker.ThrowLightningGlow(Wearer.TrueCenter(), Wearer.Map, 3f);
            }
            ticksToReset = -1;
            energy = EnergyOnReset;
        }
        public override void DrawWornExtras()
        {
            if(!ShieldIsResetting && ShouldDisplay)
            {
                float size = Mathf.Lerp(MinDrawSize, MaxDrawSize, Energy);
                Vector3 drawPos = Wearer.Drawer.DrawPos;
                drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                int ticksSinceLastAbsorbedDamage = Find.TickManager.TicksGame - lastAbsorbDamageTick;
                if(ticksSinceLastAbsorbedDamage < JitterDurationTicks)
                {
                    float jitterDistance = (float)(JitterDurationTicks - ticksSinceLastAbsorbedDamage) / JitterDurationTicks * MaxDamagedJitterDist;
                    drawPos += impactAngleVect * jitterDistance;
                    size -= jitterDistance;
                }
                float angle = Rand.Range(0, 360);
                Vector3 s = new Vector3(size, 1f, size);
                Matrix4x4 matrix = default;
                matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
            }
        }
    }
}