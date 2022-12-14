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
                bishop = null;
                parent.pawn.health.RemoveHediff(parent);
                return;
            }

            if (bishopComp == null)
            {
                bishopComp = bishop.AllComps.OfType<Comp_BishopBlessing>().ToList()[0];
            }

            if (parent.pawn.IsHashIntervalTick(60))
            {
                if (parent.pawn.Position.DistanceToSquared(bishopComp.parent.Position) > bishopComp.Props.range * bishopComp.Props.range)
                {
                    parent.pawn.health.RemoveHediff(parent);
                }
            }
        }

        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();
            parent.pawn.health.RemoveHediff(parent);
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
