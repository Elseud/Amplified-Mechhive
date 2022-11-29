using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanMechanoids;
using Verse;
using Verse.AI;

namespace VanMechanoids
{
    [StaticConstructorOnStartup]
    public static class groupSwarmerTracker
    {
        public static List<Pawn> groupedPawns = new List<Pawn>(); //Prevents pawns from the same group from running this code.
                                                                  //That would be quite pointless considering the fact that if they're in the same group then all of them will be dragged along with the first group pawn
    }
    public class JobGiver_JumpToLoneNonMelee : JobGiver_AICastAbility
    {
        public int soloMaxPawns = 2; //Maximum amount of pawns that AI will target when it doesn't have enough allies
        public int allySizeMultiplier = 1; //How much hostile pawns are added to the minimum per ally nearby. With default settings, it will be 2 with 0 and 1 allies, 3 with 2, 4 with 3 and etc
        public float maxDistance = 15f; //Maximum distance that the pawn can jump. When allies are present, it's deducted by the distance of the furthest ally
        public float maxAllyDistance = 3f; //Distance at which allies are considered to be in the pawn's "group"
        public float groupingDistance = 3f; //Distance at which enemies are considered to be in one "group"

        // I have no fucking idea on how to balance this shit

        public float groupSizeModifier = 5f; //How much group size matters to target search
        public float distanceModifier = 0.1f; //How much distance matters to target search. Distance is squared so keep this low

        private List<Pawn> allyGroup = new List<Pawn>();
        private jumperPawnGroup holderGroup = new jumperPawnGroup();

        public JobGiver_JumpToLoneNonMelee() { }

        static JobGiver_JumpToLoneNonMelee() { }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.CurJobDef == ability.jobDef)
            {
                return (Job)null;
            }

            if (groupSwarmerTracker.groupedPawns.Contains(pawn))
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
                    groupSwarmerTracker.groupedPawns.RemoveAll((Pawn x) => allyGroup.Contains(x));
                }
                return (Job)null;
            }

            groupSwarmerTracker.groupedPawns.Add(pawn);
            foreach (Pawn ally in allyGroup)
            {
                if (pawn != ally && ally.abilities.abilities.Any((Ability a) => a.def == VM_DefOf.VM_SwarmerLongjump))
                {
                    Ability allyAbility = ally.abilities.abilities.FirstOrDefault((Ability a) => a.def == VM_DefOf.VM_SwarmerLongjump);
                    if (allyAbility != null && allyAbility.CanCast && allyAbility.AICanTargetNow(target))
                    {
                        ally.jobs.StartJob(allyAbility.GetJob(target, target), JobCondition.InterruptForced, null, false, true, null, null, false, false, new bool?(false), false, true);
                    }
                }
            }

            return pawnAbility.GetJob(target, target);
        }

        private float LocateGroup(Pawn pawn)
        {
            allyGroup = new List<Pawn>();
            float groupDist = 0f;

            foreach (Pawn ally in pawn.Map.mapPawns.PawnsInFaction(pawn.Faction))
            {
                if (groupSwarmerTracker.groupedPawns.Contains(ally)) //Already reserved by another jumper
                {
                    continue;
                }

                float allyDist = ally.Position.DistanceToSquared(pawn.Position);
                if (pawn != ally && ally.Spawned && !ally.Downed && !ally.Dead && allyDist <= maxAllyDistance * maxAllyDistance)
                {
                    allyGroup.Add(ally);
                    groupDist = Math.Max(groupDist, allyDist);
                }
            }

            return groupDist;
        }

        public List<jumperPawnGroup> groupPawns(List<Pawn> potentialTargets)
        {
            List<jumperPawnGroup> pawnGroups = new List<jumperPawnGroup>();
            for (int i = 0; i < potentialTargets.Count; i++)
            {
                Pawn target = potentialTargets[i];
                if (target.health.Downed || target.health.Dead)
                {
                    continue;
                }

                jumperPawnGroup groupZero = holderGroup;
                bool foundGroup = false;
                for (int j = 0; j < pawnGroups.Count; j++)
                {
                    jumperPawnGroup group = pawnGroups[j];
                    if (group.groupCenter.DistanceToSquared(target.Position) > groupingDistance * groupingDistance)
                    {
                        continue;
                    }

                    if (!foundGroup)
                    {
                        groupZero = group;
                        groupZero.members.Add(target);
                        groupZero.groupCenter = ((groupZero.groupCenter * groupZero.members.Count) + target.Position) * (1 / (groupZero.members.Count + 1));
                    }
                    else
                    {
                        groupZero.groupCenter = ((groupZero.groupCenter * groupZero.members.Count) + (group.groupCenter * group.members.Count)) * (1 / (groupZero.members.Count + group.members.Count));
                        groupZero.members = groupZero.members.Concat(group.members).ToList();
                        pawnGroups.Remove(group);
                        j--;
                    }
                }

                if (!foundGroup)
                {
                    jumperPawnGroup newGroup = new jumperPawnGroup(new List<Pawn>() { target }, new IntVec3(target.Position.x, target.Position.y, target.Position.z));
                    pawnGroups.Add(newGroup);
                }
            }

            return pawnGroups;
        }

        protected override LocalTargetInfo GetTarget(Pawn caster, Ability ability)
        {
            float groupDist = LocateGroup(caster);
            groupSwarmerTracker.groupedPawns = groupSwarmerTracker.groupedPawns.Concat(allyGroup).ToList();

            List<Pawn> potentialTargets = new List<Pawn>();
            for (int i = caster.Map.mapPawns.AllPawnsSpawned.Count - 1; i >= 0; i--)
            {
                Pawn pawn = caster.Map.mapPawns.AllPawnsSpawned[i];
                if (!pawn.Faction.HostileTo(caster.Faction) || pawn.Position.DistanceToSquared(caster.Position) > (maxDistance - groupDist) * (maxDistance - groupDist))
                {
                    continue;
                }

                if (!JumpUtility.ValidJumpTarget(pawn.MapHeld, pawn.Position))
                {
                    continue;
                }

                if (GenSight.LineOfSight(caster.Position, pawn.Position, pawn.MapHeld, false, null, 0, 0))
                {
                    potentialTargets.Add(pawn);
                }
            }

            List<jumperPawnGroup> pawnGroups = groupPawns(potentialTargets);

            if (pawnGroups.Count == 0)
            {
                groupSwarmerTracker.groupedPawns.RemoveAll((Pawn x) => allyGroup.Contains(x));
                return null;
            }

            jumperPawnGroup bestGroup = new jumperPawnGroup();
            float bestCoeff = -1f;

            foreach (jumperPawnGroup jumperGroup in pawnGroups)
            {
                if (jumperGroup.members.Count > Math.Max(allyGroup.Count * allySizeMultiplier, soloMaxPawns))
                {
                    continue;
                } 

                float groupCoeff = jumperGroup.groupCenter.DistanceToSquared(caster.Position) * distanceModifier + jumperGroup.members.Count * groupSizeModifier;

                if (bestCoeff == -1 || groupCoeff < bestCoeff)
                {
                    bestGroup = jumperGroup;
                    bestCoeff = groupCoeff;
                }
            }
            
            if(bestCoeff == -1)
            {
                groupSwarmerTracker.groupedPawns.RemoveAll((Pawn x) => allyGroup.Contains(x));
                return null;
            }

            return new LocalTargetInfo(bestGroup.groupCenter);
        }
    }

    public struct jumperPawnGroup
    {
        public List<Pawn> members;
        public IntVec3 groupCenter;

        public jumperPawnGroup(List<Pawn> newMembers, IntVec3 newGroupCenter)
        {
            members = newMembers;
            groupCenter = newGroupCenter;
        }
    }
}
