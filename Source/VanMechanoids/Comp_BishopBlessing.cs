using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Verse;
using Verse.AI;
using VFECore;

namespace VanMechanoids
{
    public static class BishopBuffList
    {
        public static List<Pawn> buffedPawns = new List<Pawn>();
    }

    [HotSwap.HotSwappable]
    public class Comp_BishopBlessing : ThingComp
    {
        public CompProperties_BishopBlessing Props => props as CompProperties_BishopBlessing;

        private List<Pawn> buffedAllies = new List<Pawn>();

        public override void CompTick()
        {
            base.CompTick();
            float squaredRange = Props.range * Props.range;
            if (parent.IsHashIntervalTick(Props.updateFrequency))
            {
                Pawn pawn = parent as Pawn;
                foreach (Pawn ally in pawn.Map.mapPawns.PawnsInFaction(pawn.Faction))
                {
                    if (pawn == ally)
                    {
                        continue;
                    }

                    bool buffed = buffedAllies.Contains(ally);
                    bool otherBuffed = BishopBuffList.buffedPawns.Contains(ally);
                    if (otherBuffed && !buffed)
                    {
                        continue;
                    }

                    Log.Message(pawn.Position.DistanceToSquared(ally.Position) + " " + squaredRange);

                    if (pawn.Position.DistanceToSquared(ally.Position) > squaredRange)
                    {
                        if (buffed)
                        {
                            Hediff buffHediff = ally.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
                            if (buffHediff != null)
                            {
                                ally.health.RemoveHediff(buffHediff);
                            }
                            buffedAllies.Remove(ally);
                            BishopBuffList.buffedPawns.Remove(ally);
                        }
                    }
                    else
                    {
                        if (!buffed)
                        {
                            if (otherBuffed)
                            {
                                continue;
                            }

                            Hediff hediff = HediffMaker.MakeHediff(Props.hediffDef, ally);
                            ally.health.AddHediff(hediff);
                            buffedAllies.Add(ally);
                            BishopBuffList.buffedPawns.Add(ally);
                        }
                    }
                }
            }
        }
    }

    public class CompProperties_BishopBlessing : CompProperties
    {
        public CompProperties_BishopBlessing()
        {
            compClass = typeof(Comp_BishopBlessing);
        }

        public HediffDef hediffDef;
        public float range;
        public int updateFrequency = 300;
    }
}
