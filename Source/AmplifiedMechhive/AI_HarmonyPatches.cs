using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using AthenaFramework;
using Verse.AI.Group;
using System.Reflection;
using UnityEngine.Networking;
using static HarmonyLib.Code;

namespace AmplifiedMechhive
{
    [HarmonyPatch(typeof(JobGiver_Wander), "TryGiveJob")]
    public static class JobGiver_Wander_TryGiveJob_Patch
    {
        public static bool Prefix(JobGiver_Wander __instance, Pawn pawn, ref Job __result)
        {
            if (TryVicarJob(__instance, pawn, ref __result))
            {
                return false;
            }

            return true;
        }

        public static bool TryVicarJob(JobGiver_Wander __instance, Pawn pawn, ref Job __result)
        {
            if (pawn.Faction == null || pawn.Faction.IsPlayer)
            {
                return false;
            }

            if (pawn.equipment == null || pawn.equipment.PrimaryEq == null || pawn.equipment.PrimaryEq.PrimaryVerb is not Verb_CastVicarBeam)
            {
                return false;
            }

            Verb_CastVicarBeam verb = pawn.equipment.PrimaryEq.PrimaryVerb as Verb_CastVicarBeam;
            HediffWithComps trackerHediff = pawn.health.hediffSet.GetFirstHediffOfDef(AM_DefOf.AM_VicarTracker) as HediffWithComps;

            if (trackerHediff == null)
            {
                Pawn bestAlly = null;
                float bestCombatPoints = 0;
                foreach (Pawn target in PawnGroupUtility.GetNearbyAllies(pawn, verb.verbProps.range))
                {
                    if (target == pawn)
                    {
                        continue;
                    }

                    if (!GenSight.LineOfSight(pawn.Position, target.Position, target.MapHeld, false, null, 0, 0) || target.kindDef == null)
                    {
                        continue;
                    }

                    HediffWithComps buffHediff = target.health.hediffSet.GetFirstHediffOfDef(AM_DefOf.AM_VicarBuff) as HediffWithComps;

                    if (target.kindDef == AM_DefOf.AM_Vicar || buffHediff != null || !verb.ValidateTarget(new LocalTargetInfo(target)))
                    {
                        continue;
                    }

                    if (bestAlly == null || target.kindDef.combatPower > bestCombatPoints)
                    {
                        bestAlly = target;
                        bestCombatPoints = target.kindDef.combatPower;
                    }
                }

                if (bestAlly == null)
                {
                    return false;
                }

                if (!verb.TryStartCastOn(new LocalTargetInfo(bestAlly), false, false, false, false))
                {
                    return false;
                }

                Job newJob = JobMaker.MakeJob(JobDefOf.FollowClose, bestAlly);
                newJob.expiryInterval = 180;
                newJob.checkOverrideOnExpire = true;
                newJob.followRadius = verb.verbProps.range / 2;

                if (newJob == null)
                {
                    return false;
                }

                __result = newJob;
                return true;
            }

            Job job = JobMaker.MakeJob(JobDefOf.FollowClose, trackerHediff.TryGetComp<HediffComp_VicarBeam>().linkedHediff.pawn);
            job.expiryInterval = 180;
            job.checkOverrideOnExpire = true;
            job.followRadius = verb.verbProps.range / 2;

            if (job == null)
            {
                return false;
            }

            __result = job;
            return true;
        }
    }

    [HarmonyPatch(typeof(JobGiver_AIFightEnemy), "TryGiveJob"), StaticConstructorOnStartup]
    public static class JobGiver_AIFightEnemy_TryGiveJob_Patch
    {

        public static bool Prefix(JobGiver_AIFightEnemy __instance, Pawn pawn, ref Job __result)
        {
            if (TryVicarJob(__instance, pawn, ref __result))
            {
                return false;
            }

            return true;
        }

        public static bool TryVicarJob(JobGiver_AIFightEnemy __instance, Pawn pawn, ref Job __result)
        {
            if (pawn.Faction == null || pawn.Faction.IsPlayer)
            {
                return false;
            }

            if (pawn.equipment == null || pawn.equipment.PrimaryEq == null || pawn.equipment.PrimaryEq.PrimaryVerb is not Verb_CastVicarBeam)
            {
                return false;
            }

            Verb_CastVicarBeam verb = pawn.equipment.PrimaryEq.PrimaryVerb as Verb_CastVicarBeam;
            HediffWithComps trackerHediff = pawn.health.hediffSet.GetFirstHediffOfDef(AM_DefOf.AM_VicarTracker) as HediffWithComps;

            if (trackerHediff == null)
            {
                Pawn bestAlly = null;
                float bestCombatPoints = 0;
                foreach (Pawn target in PawnGroupUtility.GetNearbyAllies(pawn, verb.verbProps.range))
                {
                    if (target == pawn)
                    {
                        continue;
                    }

                    if (!GenSight.LineOfSight(pawn.Position, target.Position, target.MapHeld, false, null, 0, 0) || target.kindDef == null)
                    {
                        continue;
                    }

                    HediffWithComps buffHediff = target.health.hediffSet.GetFirstHediffOfDef(AM_DefOf.AM_VicarBuff) as HediffWithComps;

                    if (target.kindDef == AM_DefOf.AM_Vicar || buffHediff != null || !verb.ValidateTarget(new LocalTargetInfo(target)))
                    {
                        continue;
                    }

                    if (bestAlly == null || target.kindDef.combatPower > bestCombatPoints)
                    {
                        bestAlly = target;
                        bestCombatPoints = target.kindDef.combatPower;
                    }
                }

                if (bestAlly == null)
                {
                    return false;
                }

                if (!verb.TryStartCastOn(new LocalTargetInfo(bestAlly), false, false, false, false))
                {
                    return false;
                }

                Job newJob = JobMaker.MakeJob(JobDefOf.FollowClose, bestAlly);
                newJob.expiryInterval = 180;
                newJob.checkOverrideOnExpire = true;
                newJob.followRadius = verb.verbProps.range / 2;

                if (newJob == null)
                {
                    return false;
                }

                __result = newJob;
                return true;
            }

            Job job = JobMaker.MakeJob(JobDefOf.FollowClose, trackerHediff.TryGetComp<HediffComp_VicarBeam>().linkedHediff.pawn);
            job.expiryInterval = 180;
            job.checkOverrideOnExpire = true;
            job.followRadius = verb.verbProps.range / 2;

            if (job == null)
            {
                return false;
            }

            __result = job;
            return true;
        }
    }
}
