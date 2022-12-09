using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;
using UnityEngine.UIElements;
using Mono.Unix.Native;
using static UnityEngine.GraphicsBuffer;
using HarmonyLib;
using HotSwap;

namespace AmplifiedMechhive
{
    [HotSwappable]
    public class JobDriver_ProtectBishop : JobDriver
    {
        public Pawn followedPawn => job.GetTarget(TargetIndex.A).Thing as Pawn;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        public JobDriver_ProtectBishop() { }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return new Toil
            {
                tickAction = delegate ()
                {
                    bool moving = pawn.pather.Moving;
                    if (moving && !pawn.IsHashIntervalTick(30))
                    {
                        return;
                    }

                    IntVec3 targetCell = followedPawn.Position;

                    if (followedPawn.pather.Moving && followedPawn.pather.curPath != null)
                    {
                        List<IntVec3> nodes = followedPawn.pather.curPath.NodesReversed;
                        if (nodes.Count > 0)
                        {
                            targetCell = nodes[Math.Min(nodes.Count - 1, 5)];
                        }
                    }

                    if (moving && !pawn.pather.Destination.Cell.InHorDistOf(targetCell, job.followRadius))
                    {
                        return;
                    }

                    job.locomotionUrgency = (pawn.CurJob != null) ? pawn.CurJob.locomotionUrgency : LocomotionUrgency.Walk;

                    IntVec3 goal = CellFinder.RandomClosewalkCellNear(targetCell, pawn.Map, Mathf.FloorToInt(job.followRadius), null);

                    if (goal == pawn.Position)
                    {
                        EndJobWith(JobCondition.Succeeded);
                        return;
                    }

                    if (!pawn.CanReach(goal, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.ByPawn))
                    {
                        EndJobWith(JobCondition.Incompletable);
                        return;
                    }

                    pawn.pather.StartPath(goal, PathEndMode.OnCell);
                },

                defaultCompleteMode = ToilCompleteMode.Never
            };
            yield break;
        }

        public override bool IsContinuation(Job j)
        {
            return this.job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
        }
    }
}
