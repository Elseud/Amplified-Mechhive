using HotSwap;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VFECore.Shields;

namespace VanMechanoids
{
    [HotSwappable]
    public class HediffComp_EMPShield : HediffComp_Draw
    {
        public float energy;
        public int ticksToReset;

        public HediffCompProperties_EMPShield Props => props as HediffCompProperties_EMPShield;

        public override void DrawAt(Vector3 drawPos)
        {
            if (energy <= 0)
            {
                return;
            }

            float sizeCoeff = 1f + Props.chargeSizeOffset * (energy / Props.maxEnergy - 0.5f);
            drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(drawPos, Quaternion.AngleAxis(Rand.Range(0, 360), Vector3.up),
                new Vector3(Props.graphic.drawSize.x * sizeCoeff, 1f, Props.graphic.drawSize.y * sizeCoeff)),
                Graphic.MatSingleFor(Pawn), 0);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (ticksToReset > 0)
            {
                ticksToReset--;
            }
            else
            {
                energy = Math.Min(energy + Props.energyPerTick, Props.maxEnergy);
            }
        }

        public bool Notify_DamageApplied(ref DamageInfo dinfo)
        {
            if (dinfo.Def != DamageDefOf.EMP || energy <= 0)
            {
                return false;
            }

            if (energy <= dinfo.Amount)
            {
                dinfo.SetAmount(dinfo.Amount - energy);
                Break();
                return dinfo.Amount == 0;
            }

            energy -= dinfo.Amount;

            return true;
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            energy = Props.maxEnergy;
        }

        public void Break()
        {
            EffecterDefOf.Shield_Break.SpawnAttached(parent.pawn, parent.pawn.MapHeld, Props.graphic.drawSize.x * (1f + Props.chargeSizeOffset * (energy / Props.maxEnergy - 0.5f)) * 0.5f);
            FleckMaker.Static(parent.pawn.TrueCenter(), parent.pawn.MapHeld, FleckDefOf.ExplosionFlash, 12f);
            for (int i = 0; i < 6; i++)
            {
                FleckMaker.ThrowDustPuff(parent.pawn.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle((float)Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), parent.pawn.MapHeld, Rand.Range(0.8f, 1.2f));
            }
            energy = 0f;
            ticksToReset = Props.rechargeDelay;
            GenExplosion.DoExplosion(parent.pawn.Position, parent.pawn.MapHeld, Props.shieldExplosionRadius, VM_DefOf.VM_ShieldExplosion, parent.pawn);
        }
    }

    [HotSwappable]
    public class HediffCompProperties_EMPShield : HediffCompProperties_Draw
    {

        public float maxEnergy;
        public float energyPerTick;
        public int rechargeDelay;
        public float chargeSizeOffset;
        public float shieldExplosionRadius;

        public SoundDef absorbDamageSound;
        public SoundDef brokenSound;
        public SoundDef resetSound;

        public HediffCompProperties_EMPShield()
        {
            this.compClass = typeof(HediffComp_EMPShield);
        }
    }
}
