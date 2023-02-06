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
    public class JobGiver_CastAbilityOnPowerfulAlly : JobGiver_AICastAbility
    {

        public JobGiver_CastAbilityOnPowerfulAlly() { }

        static JobGiver_CastAbilityOnPowerfulAlly() { }

        public override Job TryGiveJob(Pawn pawn)
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

        public override LocalTargetInfo GetTarget(Pawn caster, Ability ability)
        {
            Pawn bestAlly = null;
            float bestCombatPoints = 0;
            foreach (Pawn pawn in PawnGroupUtility.GetNearbyAllies(caster, ability.verb.verbProps.range))
            {
                if (pawn == caster)
                {
                    continue;
                }

                if (!GenSight.LineOfSight(caster.Position, pawn.Position, pawn.MapHeld, false, null, 0, 0) || pawn.kindDef == null)
                {
                    continue;
                }

                if (bestAlly == null || pawn.kindDef.combatPower > bestCombatPoints)
                {
                    bestAlly = pawn;
                    bestCombatPoints = pawn.kindDef.combatPower;
                }
            }

            return new LocalTargetInfo(bestAlly);
        }
    }
}
