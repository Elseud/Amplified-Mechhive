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

        public List<Pawn> groupedPawns = new List<Pawn>(); //Prevents pawns from the same group from running this code.
                                                           //That would be quite pointless considering the fact that if they're in the same group then all of them will be dragged along with the first group pawn

        public JobGiver_JumpToLoneNonMelee() { }

        static JobGiver_JumpToLoneNonMelee() { }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.CurJobDef == this.ability.jobDef)
            {
                return (Job)null;
            }

            if (groupedPawns.Contains(pawn))
            {
                groupedPawns.Remove(pawn);
            }

            Ability pawnAbility = pawn.abilities?.GetAbility(this.ability);
            if (pawnAbility == null || !pawnAbility.CanCast)
            {
                return (Job)null;
            }

            LocalTargetInfo target = this.GetTarget(pawn, pawnAbility);

            if (target == null || !target.IsValid)
            {
                return (Job)null;
            }

            return pawnAbility.GetJob(target, target);
        }

        private (List<Pawn>, float) LocateGroup(Pawn pawn)
        {
            List<Pawn> group = new List<Pawn>();
            float groupDist = 0f;

            foreach (Pawn ally in pawn.Map.mapPawns.PawnsInFaction(pawn.Faction))
            {
                if (groupedPawns.Contains(ally)) //Already reserved by another jumper
                {
                    continue;
                }

                float allyDist = ally.Position.DistanceToSquared(pawn.Position);
                if (pawn != ally && ally.Spawned && !ally.Downed && !ally.Dead && allyDist <= maxAllyDistance * maxAllyDistance)
                {
                    group.Add(ally);
                    groupDist = Math.Max(groupDist, allyDist);
                }
            }

            return (group, groupDist);
        }

        public List<jumperPawnGroup> groupPawns(List<Pawn> potentialTargets)
        {
            List<jumperPawnGroup> pawnGroups = new List<jumperPawnGroup>();
            foreach (Pawn target in potentialTargets)
            {
                if (target.health.Downed || target.health.Dead)
                {
                    continue;
                }

                List<jumperPawnGroup> includedGroups = pawnGroups.Where((jumperPawnGroup x) => x.groupCenter.DistanceToSquared(target.Position) <= groupingDistance * groupingDistance).ToList();
                if (includedGroups.Count > 1)
                {
                    for (int i = 1; i < includedGroups.Count; i++)
                    {
                        jumperPawnGroup jumperGroup = includedGroups[i];
                        jumperPawnGroup groupZero = includedGroups[0];
                        groupZero.groupCenter = ((groupZero.groupCenter * groupZero.members.Count) + (jumperGroup.groupCenter * jumperGroup.members.Count)) * (1 / (groupZero.members.Count + jumperGroup.members.Count));
                        groupZero.members = groupZero.members.Concat(jumperGroup.members).ToList();
                    }
                }

                if (includedGroups.Count > 0)
                {
                    jumperPawnGroup groupZero = includedGroups[0];
                    groupZero.members.Add(target);
                    groupZero.groupCenter = ((includedGroups[0].groupCenter * includedGroups[0].members.Count) + target.Position) * (1 / (includedGroups[0].members.Count + 1));
                    pawnGroups.Add(includedGroups[0]);
                }
                else
                {
                    jumperPawnGroup newGroup = new jumperPawnGroup(new List<Pawn>() { target }, new IntVec3(target.Position.x, target.Position.y, target.Position.z));
                    pawnGroups.Add(newGroup);
                }
            }

            return pawnGroups;
        }

        protected override LocalTargetInfo GetTarget(Pawn caster, Ability ability)
        {
            var (allyGroup, groupDist) = this.LocateGroup(caster);

            List<Pawn> potentialTargets = caster.Map.mapPawns.AllPawnsSpawned.Where((Pawn x) => x.Faction.HostileTo(caster.Faction) && (x.Position.DistanceToSquared(caster.Position) <= (maxDistance - groupDist) * (maxDistance - groupDist)) && JumpUtility.ValidJumpTarget(x.MapHeld, x.Position)).ToList();
            List<jumperPawnGroup> pawnGroups = groupPawns(potentialTargets);

            if (pawnGroups.Count == 0)
            {
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
                return null;
            }

            groupedPawns = groupedPawns.Concat(allyGroup).ToList();

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
