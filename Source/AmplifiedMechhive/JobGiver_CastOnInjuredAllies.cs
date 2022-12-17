using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using AthenaFramework;

namespace AmplifiedMechhive
{
    public class JobGiver_CastOnInjuredAllies : JobGiver_AICastAbility
    {
        public float healthPercentage = 0.5f;
        public int thresholdTicks = 2500;

        public JobGiver_CastOnInjuredAllies() { }

        static JobGiver_CastOnInjuredAllies() { }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.CurJobDef == ability.jobDef)
            {
                return (Job)null;
            }

            Ability pawnAbility = pawn.abilities?.GetAbility(ability);
            if (pawnAbility == null || !pawnAbility.CanCast)
            {
                return (Job)null;
            }

            LocalTargetInfo target = GetTarget(pawn, pawnAbility);

            if (target == null || !target.IsValid)
            {
                return (Job)null;
            }

            return pawnAbility.GetJob(target, target);
        }

        protected override LocalTargetInfo GetTarget(Pawn caster, Ability ability)
        {
            CompProjectileInterceptor comp = caster.GetComp<CompProjectileInterceptor>();
            if (comp == null)
            {
                return null;
            }

            List<Pawn> nearbyAllies = PawnGroupUtility.GetNearbyAllies(caster, ability.verb.verbProps.range); //Could've added checkDowned but mechs shouldn't really care about
            nearbyAllies.Remove(caster);
            List<PawnGroupup> groups = PawnGroupUtility.GroupPawns(nearbyAllies, comp.Props.radius);

            float bestCombatPoints = 0;
            PawnGroupup bestGroup = null;

            foreach (PawnGroupup group in groups)
            {
                if (!JumpUtility.ValidJumpTarget(caster.MapHeld, group.groupCenter))
                {
                    continue;
                }

                if (!GenSight.LineOfSight(caster.Position, group.groupCenter, caster.MapHeld, false, null, 0, 0))
                {
                    continue;
                }

                float combatPoints = 0;
                foreach(Pawn pawn in group.members)
                {
                    if (pawn.health.summaryHealth.SummaryHealthPercent < healthPercentage && pawn.mindState.lastRangedHarmTick > 0 && pawn.mindState.lastRangedHarmTick + thresholdTicks > Find.TickManager.TicksGame)
                    {
                        combatPoints += pawn.kindDef.combatPower;
                    }
                }

                if (combatPoints > bestCombatPoints)
                {
                    bestGroup = group;
                    bestCombatPoints = combatPoints;
                }
            }

            if (bestGroup == null)
            {
                return null;
            }

            return new LocalTargetInfo(bestGroup.groupCenter);
        }
    }
}
