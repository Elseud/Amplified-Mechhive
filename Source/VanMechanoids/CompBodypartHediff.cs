using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VanMechanoids
{
    public class CompBodypartHediff : ThingComp
    {
        private CompProperties_BodypartHediff Props => props as CompProperties_BodypartHediff;

        public override void CompTick()
        {
            base.CompTick();
            if (parent.Spawned)
            {
                if (parent is Pawn)
                {
                    Pawn pawn = parent as Pawn;
                    List<BodyPartRecord> partRecords = pawn.RaceProps.body.GetPartsWithDef(Props.bodyPartDef);
                    foreach (BodyPartRecord partRecord in partRecords)
                    {
                        if (!pawn.health.hediffSet.HasHediff(Props.hediffDef, partRecord))
                        {
                            Hediff hediff = HediffMaker.MakeHediff(Props.hediffDef, pawn, partRecord);
                            pawn.health.AddHediff(hediff, partRecord, null, null);
                        }
                    }
                }

                parent.AllComps.Remove(this);
            }
        }
    }

    public class CompProperties_BodypartHediff : CompProperties
    {
        public CompProperties_BodypartHediff()
        {
            compClass = typeof(CompBodypartHediff);
        }

        public BodyPartDef bodyPartDef;
        public HediffDef hediffDef;
    }
}
