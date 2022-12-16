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

namespace AmplifiedMechhive
{
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
    }
}
