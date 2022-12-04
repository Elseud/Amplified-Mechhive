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
        public BeamRenderer linkedBeam;
        public Hediff linkedHediff;

        private HediffCompProperties_VicarBeam Props => props as HediffCompProperties_VicarBeam;

        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();
            if (linkedBeam == null || parent.pawn == null || parent.pawn.MapHeld == null)
            {
                return;
            }

            MapComponent_AthenaRenderer renderer = parent.pawn.MapHeld.GetComponent<MapComponent_AthenaRenderer>();

            if (renderer != null)
            {
                renderer.DestroyBeam(linkedBeam);
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (!Props.destroyOnDowned)
            {
                return;
            }

            if (parent.pawn != null && parent.pawn.MapHeld != null && parent.pawn.Downed)
            {
                MapComponent_AthenaRenderer renderer = parent.pawn.MapHeld.GetComponent<MapComponent_AthenaRenderer>();
                if (renderer != null)
                {
                    renderer.DestroyBeam(linkedBeam);
                }
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
