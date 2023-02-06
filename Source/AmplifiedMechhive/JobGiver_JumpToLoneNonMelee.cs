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
    [StaticConstructorOnStartup]
    public static class GroupSwarmerTracker
    {
        public static List<Pawn> groupedPawns = new List<Pawn>(); //Prevents pawns from the same group from running this code in order to lower the TPS impact.
                                                                  //That would be quite pointless considering the fact that if they're in the same group then all of them will be dragged along with the first group pawn.
    }
    public class JobGiver_JumpToLoneNonMelee : JobGiver_AICastAbility
    {
        public int soloMaxPawns = 2; //Maximum amount of pawns that AI will target when it doesn't have enough allies
        public int allySizeMultiplier = 1; //How much hostile pawns are added to the minimum per ally nearby. With default settings, it will be 2 with 0 and 1 allies, 3 with 2, 4 with 3 and etc
        public float maxAllyDistance = 3f; //Distance at which allies are considered to be in the pawn's "group"
        public float groupingDistance = 3f; //Distance at which enemies are considered to be in one "group"

        public float groupSizeModifier = 5f; //How much group size matters to target search
        public float distanceModifier = 0.1f; //How much distance matters to target search. Distance is squared so keep this low

        private List<Pawn> allyGroup = new List<Pawn>();

        public JobGiver_JumpToLoneNonMelee() { }

        static JobGiver_JumpToLoneNonMelee() { }

        public override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.CurJobDef == ability.jobDef)
            {
                return (Job)null;
            }

            if (GroupSwarmerTracker.groupedPawns.Contains(pawn))
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
                if (allyGroup != null)
                {
                    GroupSwarmerTracker.groupedPawns.RemoveAll((Pawn x) => allyGroup.Contains(x));
                }
                return (Job)null;
            }

            GroupSwarmerTracker.groupedPawns.Add(pawn);
            foreach (Pawn ally in allyGroup)
            {
                if (pawn != ally && ally.abilities.abilities.Any((Ability a) => a.def == AM_DefOf.AM_SwarmerLongjump))
                {
                    Ability allyAbility = ally.abilities.abilities.FirstOrDefault((Ability a) => a.def == AM_DefOf.AM_SwarmerLongjump);
                    if (allyAbility != null && allyAbility.CanCast && allyAbility.AICanTargetNow(target))
                    {
                        ally.jobs.StartJob(allyAbility.GetJob(target, target), JobCondition.InterruptForced, null, false, true, null, null, false, false, new bool?(false), false, true);
                    }
                }
            }

            return pawnAbility.GetJob(target, target);
        }

        private float LocateAllyGroup(Pawn pawn)
        {
            allyGroup = new List<Pawn>();
            float groupDist = 0f;

            foreach (var pawnAndDistance in PawnGroupUtility.GetNearbyAlliesWithDistances(pawn, maxAllyDistance))
            {
                if (GroupSwarmerTracker.groupedPawns.Contains(pawnAndDistance.Key)) //Already reserved by another jumper
                {
                    continue;
                }

                allyGroup.Add(pawnAndDistance.Key);
                groupDist = Math.Max(groupDist, pawnAndDistance.Value);
            }

            return groupDist;
        }

        public override LocalTargetInfo GetTarget(Pawn caster, Ability ability)
        {
            float groupDist = LocateAllyGroup(caster);
            GroupSwarmerTracker.groupedPawns = GroupSwarmerTracker.groupedPawns.Concat(allyGroup).ToList();

            List<Pawn> potentialTargets = new List<Pawn>();
            foreach (Pawn pawn in PawnGroupUtility.GetNearbyHostiles(caster, (ability.verb.verbProps.range - groupDist)))
            {
                if (!JumpUtility.ValidJumpTarget(pawn.MapHeld, pawn.Position))
                {
                    continue;
                }

                if (GenSight.LineOfSight(caster.Position, pawn.Position, pawn.MapHeld, false, null, 0, 0))
                {
                    potentialTargets.Add(pawn);
                }
            }

            List<PawnGroupup> pawnGroups = PawnGroupUtility.GroupPawns(potentialTargets, groupingDistance);

            if (pawnGroups.Count == 0)
            {
                GroupSwarmerTracker.groupedPawns.RemoveAll((Pawn x) => allyGroup.Contains(x));
                return null;
            }

            PawnGroupup bestGroup = null;
            float bestCoeff = 0f;

            foreach (PawnGroupup jumperGroup in pawnGroups)
            {
                if (jumperGroup.members.Count > Math.Max(allyGroup.Count * allySizeMultiplier, soloMaxPawns))
                {
                    continue;
                } 

                float groupCoeff = jumperGroup.groupCenter.DistanceToSquared(caster.Position) * distanceModifier + jumperGroup.members.Count * groupSizeModifier;

                if (bestGroup == null || groupCoeff < bestCoeff)
                {
                    bestGroup = jumperGroup;
                    bestCoeff = groupCoeff;
                }
            }
            
            if(bestGroup == null)
            {
                GroupSwarmerTracker.groupedPawns.RemoveAll((Pawn x) => allyGroup.Contains(x));
                return null;
            }

            return new LocalTargetInfo(bestGroup.groupCenter);
        }
    }
}
