﻿using HotSwap;
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
    [HotSwappable]
    public class Verb_CastVicarBeam : Verb_CastAbility
    {
        public bool cachedCanHit = false;
        public IntVec3 cachedPosition;
        public IntVec3 cachedRoot;

        protected override bool TryCastShot()
        {
            if (!base.TryCastShot())
            {
                return false;
            }

            if (CasterPawn == null || CasterPawn.MapHeld == null || !currentTarget.HasThing || !(currentTarget.Thing is Pawn))
            {
                return false;
            }

            MapComponent_AthenaRenderer renderer = CasterPawn.MapHeld.GetComponent<MapComponent_AthenaRenderer>();

            if (renderer == null)
            {
                return false;
            }

            Pawn target = currentTarget.Thing as Pawn;

            HediffWithComps buffHediff = HediffMaker.MakeHediff(AM_DefOf.AM_VicarBuff, target) as HediffWithComps;
            target.health.AddHediff(buffHediff);
            HediffComp_VicarBeam hediffBuffComp = buffHediff.comps.Where((HediffComp x) => x is HediffComp_VicarBeam).ToList()[0] as HediffComp_VicarBeam;

            HediffWithComps trackerHediff = CasterPawn.health.hediffSet.GetFirstHediffOfDef(AM_DefOf.AM_VicarTracker) as HediffWithComps;

            if (trackerHediff != null)
            {
                HediffComp_VicarBeam trackerComp = trackerHediff.comps.Where((HediffComp x) => x is HediffComp_VicarBeam).ToList()[0] as HediffComp_VicarBeam;
                renderer.DestroyBeam(trackerComp.linkedBeam);
            }

            trackerHediff = HediffMaker.MakeHediff(AM_DefOf.AM_VicarTracker, CasterPawn) as HediffWithComps;
            CasterPawn.health.AddHediff(trackerHediff);

            HediffComp_VicarBeam hediffTrackerComp = trackerHediff.comps.Where((HediffComp x) => x is HediffComp_VicarBeam).ToList()[0] as HediffComp_VicarBeam;

            BeamInfo beamInfo = renderer.CreateActiveBeam(CasterPawn, target, AM_DefOf.AM_VicarBeam);

            hediffBuffComp.linkedHediff = trackerHediff;
            hediffTrackerComp.linkedHediff = buffHediff;
            hediffBuffComp.linkedBeam = beamInfo.beam;
            hediffTrackerComp.linkedBeam = beamInfo.beam;

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

            return targetPawn.health.hediffSet.GetFirstHediffOfDef(AM_DefOf.AM_VicarBuff) == null && CanHitTarget(target);
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

            return cachedCanHit;
        }
    }
}