using HarmonyLib;
using HotSwap;
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

namespace AmplifiedMechhive
{
    [HarmonyPatch(typeof(JobGiver_Wander), "TryGiveJob"), HotSwappable]
    public static class JobGiver_Wander_TryGiveJob_Patch
    {
        public static bool Prefix(JobGiver_Wander __instance, Pawn pawn, ref Job __result)
        {
            if (!pawn.RaceProps.IsMechanoid || pawn.Faction.IsPlayer || pawn.kindDef == AM_DefOf.AM_Bishop)
            {
                return true;
            }

            if (pawn.equipment.Primary == null || pawn.equipment.Primary.def.Verbs.Select((VerbProperties x) => (x.range > 1)).ToList().Count == 0)
            {
                return true;
            }

            List<HediffComp_BishopBlessing> blessings = pawn.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany((HediffWithComps x) => x.comps).OfType<HediffComp_BishopBlessing>().ToList();

            if (blessings.Count == 0)
            {
                return true;
            }

            HediffComp_BishopBlessing blessing = blessings[0];
            CompProperties_BishopBlessing Props = blessing.bishopComp.Props;

            if (!blessing.bishopComp.followingPawns.Contains(pawn))
            {
                return true;
            }

            Job job = JobMaker.MakeJob(JobDefOf.FollowClose, blessing.bishop);
            job.expiryInterval = Props.updateFrequency;
            job.checkOverrideOnExpire = true;
            job.followRadius = Props.range;

            if (job == null)
            {
                return true;
            }

            __result = job;
            return false;
        }
    }

    [HarmonyPatch(typeof(JobGiver_AIFightEnemy), "TryGiveJob"), HotSwappable, StaticConstructorOnStartup]
    public static class JobGiver_AIFightEnemy_TryGiveJob_Patch
    {
        public static JobGiver_AIDefendBishop jobGiver;

        static JobGiver_AIFightEnemy_TryGiveJob_Patch()
        {
            jobGiver = new JobGiver_AIDefendBishop();
        }

        public static bool Prefix(JobGiver_AIFightEnemy __instance, Pawn pawn, ref Job __result)
        {
            if (!pawn.RaceProps.IsMechanoid || pawn.Faction.IsPlayer || pawn.kindDef == AM_DefOf.AM_Bishop || __instance is JobGiver_AIDefendBishop)
            {
                return true;
            }

            if (pawn.equipment.Primary == null || pawn.equipment.Primary.def.Verbs.Select((VerbProperties x) => (x.range > 1)).ToList().Count == 0)
            {
                return true;
            }

            List<HediffComp_BishopBlessing> blessings = pawn.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany((HediffWithComps x) => x.comps).OfType<HediffComp_BishopBlessing>().ToList();

            if (blessings.Count == 0)
            {
                return true;
            }

            HediffComp_BishopBlessing blessing = blessings[0];
            CompProperties_BishopBlessing Props = blessing.bishopComp.Props;
            Pawn bishop = blessing.bishop;

            if (!blessing.bishopComp.followingPawns.Contains(pawn))
            {
                return true;
            }

            MethodInfo method = typeof(JobGiver_AIDefendBishop).GetMethod("TryGiveJob", BindingFlags.NonPublic | BindingFlags.Instance);
            Job job = method.Invoke(jobGiver, new object[] { pawn }) as Job;

            if (job == null)
            {
                return true;
            }

            __result = job;

            return true;
        }
    }

    [HotSwappable]
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

    /*
    [HarmonyPatch(typeof(JobGiver_AIFightEnemy), "TryGiveJob"), HotSwappable]
    public static class JobGiver_AIFightEnemy_TryGiveJob
    {
        public static void Postfix(JobGiver_AIFightEnemy __instance, Pawn pawn, ref Job __result)
        {
            if (!pawn.RaceProps.IsMechanoid || pawn.Faction.IsPlayer || pawn.kindDef == AM_DefOf.AM_Bishop)
            {
                return;
            }

            if (pawn.CurJobDef == AM_DefOf.AM_ProtectBishop)
            {
                return;
            }

            List<HediffComp_BishopBlessing> blessings = pawn.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany((HediffWithComps x) => x.comps).OfType<HediffComp_BishopBlessing>().ToList();
            if (blessings.Count == 0)
            {
                return;
            }

            HediffComp_BishopBlessing blessing = blessings[0];

            __result = JobMaker.MakeJob(AM_DefOf.AM_ProtectBishop, blessing.bishop);
            __result.followRadius = blessing.bishopComp.Props.range;
        }
    }

    [HarmonyPatch(typeof(JobGiver_Wander), "TryGiveJob"), HotSwappable]
    public static class JobGiver_Wander_TryGiveJob
    {
        public static bool Prefix(JobGiver_AIFightEnemy __instance, Pawn pawn, ref Job __result)
        {
            if (!pawn.RaceProps.IsMechanoid || pawn.Faction.IsPlayer || pawn.kindDef == AM_DefOf.AM_Bishop)
            {
                return true;
            }

            if (pawn.CurJobDef == AM_DefOf.AM_ProtectBishop)
            {
                __result = pawn.CurJob;
                return false;
            }

            return true;
        }
    }*/

}
