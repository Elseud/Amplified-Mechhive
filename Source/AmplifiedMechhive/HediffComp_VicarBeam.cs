using AthenaFramework;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AmplifiedMechhive
{
    public class HediffComp_VicarBeam : HediffComp
    {
        public Mote linkedBeam;
        public Hediff linkedHediff;

        private HediffCompProperties_VicarBeam Props => props as HediffCompProperties_VicarBeam;

        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();
            if (linkedBeam == null || parent.pawn == null || parent.pawn.MapHeld == null)
            {
                return;
            }

            if (linkedBeam != null)
            {
                linkedBeam.Destroy();
                linkedBeam = null;
                linkedHediff.pawn.health.RemoveHediff(linkedHediff);
                linkedHediff = null;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (!Props.destroyOnDowned)
            {
                return;
            }

            if (parent.pawn != null && parent.pawn.MapHeld != null && parent.pawn.Downed || parent.pawn.Map != linkedHediff.pawn.Map && linkedBeam != null)
            {
                linkedBeam.Destroy();
                linkedBeam = null;
                linkedHediff.pawn.health.RemoveHediff(linkedHediff);
                linkedHediff = null;
            }
        }
    }

    public class HediffCompProperties_VicarBeam : HediffCompProperties
    {
        public HediffCompProperties_VicarBeam()
        {
            this.compClass = typeof(HediffComp_VicarBeam);
        }

        public bool destroyOnDowned = false;
    }
}
