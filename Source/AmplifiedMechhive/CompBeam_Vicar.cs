using AthenaFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AmplifiedMechhive
{
    public class CompBeam_Vicar : CompBeam
    {
        public override void PreDestroyBeam()
        {
            MapComponent_AthenaRenderer renderer = parent.MapHeld.GetComponent<MapComponent_AthenaRenderer>();
            BeamInfo beamInfo = renderer.GetActiveBeamInfo(Beam);
            Pawn channelerPawn = beamInfo.beamStart as Pawn;
            Pawn targetPawn = beamInfo.beamEnd as Pawn;

            HediffWithComps trackerHediff = channelerPawn.health.hediffSet.GetFirstHediffOfDef(AM_DefOf.AM_VicarTracker) as HediffWithComps;
            HediffWithComps buffHediff = targetPawn.health.hediffSet.GetFirstHediffOfDef(AM_DefOf.AM_VicarBuff) as HediffWithComps;
            channelerPawn.health.RemoveHediff(trackerHediff);
            targetPawn.health.RemoveHediff(buffHediff);
        }
    }

    public class CompProperties_VicarBeam : CompProperties
    {
        public CompProperties_VicarBeam()
        {
            this.compClass = typeof(CompBeam_Vicar);
        }
    }
}
