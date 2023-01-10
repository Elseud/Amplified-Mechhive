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
            if (TryBishopJob(__instance, pawn, ref __result))
            {
                return false;
            }

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
                newJob.expiryInterval = 200;
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
            job.expiryInterval = 200;
            job.checkOverrideOnExpire = true;
            job.followRadius = verb.verbProps.range / 2;

            if (job == null)
            {
                return false;
            }

            __result = job;
            return true;
        }

        public static bool TryBishopJob(JobGiver_Wander __instance, Pawn pawn, ref Job __result)
        {
            if (!pawn.RaceProps.IsMechanoid || pawn.Faction.IsPlayer || pawn.TryGetComp<Comp_BishopBlessing>() != null)
            {
                return false;
            }

            if (pawn.equipment.Primary == null || pawn.equipment.Primary.def.Verbs.Select((VerbProperties x) => (x.range > 1)).ToList().Count == 0)
            {
                return false;
            }

            List<HediffComp_BishopBlessing> blessings = pawn.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany((HediffWithComps x) => x.comps).OfType<HediffComp_BishopBlessing>().ToList();

            if (blessings.Count == 0)
            {
                return false;
            }

            HediffComp_BishopBlessing blessing = blessings[0];
            CompProperties_BishopBlessing Props = blessing.bishopComp.Props;

            if (!blessing.bishopComp.followingPawns.Contains(pawn))
            {
                return false;
            }

            Job job = JobMaker.MakeJob(JobDefOf.FollowClose, blessing.bishop);
            job.expiryInterval = Props.updateFrequency;
            job.checkOverrideOnExpire = true;
            job.followRadius = Props.range;

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
        public static JobGiver_AIDefendBishop jobGiver;

        static JobGiver_AIFightEnemy_TryGiveJob_Patch()
        {
            jobGiver = new JobGiver_AIDefendBishop();
        }

        public static bool Prefix(JobGiver_AIFightEnemy __instance, Pawn pawn, ref Job __result)
        {
            if (TryBishopJob(__instance, pawn, ref __result))
            {
                return false;
            }

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
                newJob.expiryInterval = 200;
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
            job.expiryInterval = 200;
            job.checkOverrideOnExpire = true;
            job.followRadius = verb.verbProps.range / 2;

            if (job == null)
            {
                return false;
            }

            __result = job;
            return true;
        }

        public static bool TryBishopJob(JobGiver_AIFightEnemy __instance, Pawn pawn, ref Job __result)
        {
            if (!pawn.RaceProps.IsMechanoid || pawn.Faction.IsPlayer || pawn.TryGetComp<Comp_BishopBlessing>() != null || __instance is JobGiver_AIDefendBishop)
            {
                return false;
            }

            if (pawn.equipment == null || pawn.equipment.Primary == null || pawn.equipment.Primary.def.Verbs.Select((VerbProperties x) => (x.range > 1)).ToList().Count == 0)
            {
                return false;
            }

            List<HediffComp_BishopBlessing> blessings = pawn.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany((HediffWithComps x) => x.comps).OfType<HediffComp_BishopBlessing>().ToList();

            if (blessings.Count == 0)
            {
                return false;
            }

            HediffComp_BishopBlessing blessing = blessings[0];
            CompProperties_BishopBlessing Props = blessing.bishopComp.Props;
            Pawn bishop = blessing.bishop;

            if (!blessing.bishopComp.followingPawns.Contains(pawn))
            {
                return false;
            }

            MethodInfo method = typeof(JobGiver_AIDefendBishop).GetMethod("TryGiveJob", BindingFlags.NonPublic | BindingFlags.Instance);
            Job job = method.Invoke(jobGiver, new object[] { pawn }) as Job;

            if (job == null)
            {
                return false;
            }

            __result = job;

            return true;
        }
    }

    public class JobGiver_AIDefendBishop : JobGiver_AIDefendPawn
    {
        protected override Pawn GetDefendee(Pawn pawn)
        {
            List<HediffComp_BishopBlessing> blessings = pawn.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany((HediffWithComps x) => x.comps).OfType<HediffComp_BishopBlessing>().ToList();

            if (blessings.Count == 0)
            {
                return null;
            }

            HediffComp_BishopBlessing blessing = blessings[0];

            if (!blessing.bishopComp.followingPawns.Contains(pawn))
            {
                return null;
            }

            return blessing.bishop;
        }

        protected override float GetFlagRadius(Pawn pawn)
        {
            List<HediffComp_BishopBlessing> blessings = pawn.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany((HediffWithComps x) => x.comps).OfType<HediffComp_BishopBlessing>().ToList();

            if (blessings.Count == 0)
            {
                return 0;
            }

            HediffComp_BishopBlessing blessing = blessings[0];
            CompProperties_BishopBlessing Props = blessing.bishopComp.Props;

            if (!blessing.bishopComp.followingPawns.Contains(pawn))
            {
                return 0;
            }

            return Props.range;
        }

        private Thing FindAttackTargetIfPossible(Pawn pawn)
        {
            if (pawn.TryGetAttackVerb(null, !pawn.IsColonist, false) == null)
            {
                return null;
            }
            return this.FindAttackTarget(pawn);
        }

        public JobGiver_AIDefendBishop() { }
    }
}
