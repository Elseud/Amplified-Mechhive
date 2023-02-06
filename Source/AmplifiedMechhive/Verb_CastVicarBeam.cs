using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using UnityEngine;
using AthenaFramework;
using static UnityEngine.GraphicsBuffer;
using Mono.Unix.Native;

namespace AmplifiedMechhive
{
    public class Verb_CastVicarBeam : Verb
    {
        public bool cachedCanHit = false;
        public IntVec3 cachedPosition;
        public IntVec3 cachedRoot;

        public override bool TryCastShot()
        {
            if (CasterPawn == null || CasterPawn.MapHeld == null || !currentTarget.HasThing || currentTarget.Thing is not Pawn)
            {
                return false;
            }

            Pawn target = currentTarget.Thing as Pawn;

            if (!target.Faction.AllyOrNeutralTo(CasterPawn.Faction) || target.health.hediffSet.GetFirstHediffOfDef(AM_DefOf.AM_VicarBuff) != null)
            {
                return false;
            }

            HediffWithComps buffHediff = HediffMaker.MakeHediff(AM_DefOf.AM_VicarBuff, target) as HediffWithComps;
            target.health.AddHediff(buffHediff);
            HediffComp_VicarBeam hediffBuffComp = buffHediff.comps.Where((HediffComp x) => x is HediffComp_VicarBeam).ToList()[0] as HediffComp_VicarBeam;

            HediffWithComps trackerHediff = CasterPawn.health.hediffSet.GetFirstHediffOfDef(AM_DefOf.AM_VicarTracker) as HediffWithComps;

            if (trackerHediff != null)
            {
                HediffComp_VicarBeam trackerComp = trackerHediff.comps.Where((HediffComp x) => x is HediffComp_VicarBeam).ToList()[0] as HediffComp_VicarBeam;
                if (trackerComp.linkedBeam != null)
                {
                    trackerComp.linkedBeam.Destroy();
                    trackerComp.linkedBeam = null;
                    trackerComp.linkedHediff.pawn.health.RemoveHediff(trackerComp.linkedHediff);
                    trackerComp.linkedHediff = null;
                }
            }

            trackerHediff = HediffMaker.MakeHediff(AM_DefOf.AM_VicarTracker, CasterPawn) as HediffWithComps;
            CasterPawn.health.AddHediff(trackerHediff);

            HediffComp_VicarBeam hediffTrackerComp = trackerHediff.comps.Where((HediffComp x) => x is HediffComp_VicarBeam).ToList()[0] as HediffComp_VicarBeam;

            Mote beam = MoteMaker.MakeInteractionOverlay(AM_DefOf.AM_Mote_VicarBeamBase, CasterPawn, target);

            hediffBuffComp.linkedHediff = trackerHediff;
            hediffTrackerComp.linkedHediff = buffHediff;
            hediffBuffComp.linkedBeam = beam;
            hediffTrackerComp.linkedBeam = beam;

            return true;
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (CasterPawn == null || target == null)
            {
                return false;
            }

            if (!target.HasThing)
            {
                return false;
            }

            if (!(target.Thing is Pawn))
            {
                return false;
            }

            Pawn targetPawn = target.Thing as Pawn;

            if (targetPawn.Faction != CasterPawn.Faction)
            {
                return false;
            }

            if (!targetPawn.def.race.IsMechanoid)
            {
                return false;
            }

            if (!targetPawn.Faction.AllyOrNeutralTo(CasterPawn.Faction))
            {
                return false;
            }

            HediffWithComps hediff = targetPawn.health.hediffSet.GetFirstHediffOfDef(AM_DefOf.AM_VicarBuff) as HediffWithComps;

            if (hediff != null){
                HediffComp_VicarBeam beamComp = hediff.TryGetComp<HediffComp_VicarBeam>();
                if (beamComp != null && beamComp.linkedHediff.pawn != CasterPawn)
                {
                    return false;
                }
            }

            return CanHitTarget(target);
        }

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            if (CasterPawn == null)
            {
                return false;
            }

            if (cachedRoot != root || cachedPosition != targ.Cell)
            {
                cachedRoot = root;
                cachedPosition = targ.Cell;
                cachedCanHit = GenSight.LineOfSight(root, targ.Cell, CasterPawn.MapHeld);
            }

            if (targ.Pawn == null || !targ.Pawn.Faction.AllyOrNeutralTo(CasterPawn.Faction))
            {
                return false;
            }

            return cachedCanHit;
        }
    }
}
