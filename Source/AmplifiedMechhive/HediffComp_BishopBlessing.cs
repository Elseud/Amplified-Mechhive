using AthenaFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AmplifiedMechhive
{
    public class HediffComp_BishopBlessing : HediffComp
    {
        private HediffCompProperties_BishopBlessing Props => props as HediffCompProperties_BishopBlessing;

        public Comp_BishopBlessing bishopComp;
        public Pawn bishop;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (bishop == null || !bishop.Spawned || bishop.Destroyed)
            {
                if (bishopComp != null && bishopComp.followingPawns.Contains(Pawn))
                {
                    bishopComp.followingPawns.Remove(Pawn);
                }

                Pawn.health.RemoveHediff(parent);
                bishopComp = null;
                bishop = null;
                return;
            }

            if (bishopComp == null)
            {
                bishopComp = bishop.AllComps.OfType<Comp_BishopBlessing>().ToList()[0];
            }

            if (Pawn.IsHashIntervalTick(bishopComp.Props.updateFrequency))
            {
                if (Pawn.Position.DistanceToSquared(bishopComp.parent.Position) > bishopComp.Props.range * bishopComp.Props.range)
                {
                    if (bishopComp != null && bishopComp.followingPawns.Contains(Pawn))
                    {
                        bishopComp.followingPawns.Remove(Pawn);
                    }

                    Pawn.health.RemoveHediff(parent);
                }
            }
        }

        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();

            if (bishopComp != null && bishopComp.followingPawns.Contains(Pawn))
            {
                bishopComp.followingPawns.Remove(Pawn);
            }

            Pawn.health.RemoveHediff(parent);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref bishop, "bishop");
        }
    }

    public class HediffCompProperties_BishopBlessing : HediffCompProperties
    {
        public HediffCompProperties_BishopBlessing()
        {
            this.compClass = typeof(HediffComp_BishopBlessing);
        }
    }
}
