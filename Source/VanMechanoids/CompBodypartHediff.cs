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
        private CompProperties_BodypartHediff Props
        {
            get
            {
                return (CompProperties_BodypartHediff)this.props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (parent.Spawned)
            {
                if (parent is Pawn)
                {
                    Pawn pawn = parent as Pawn;
                    if (pawn.RaceProps.body.AllParts.Where((BodyPartRecord x) => x.def == Props.bodyPartDef).ToList().Count() > 0)
                    {
                        BodyPartRecord partRecord = pawn.RaceProps.body.AllParts.Where((BodyPartRecord x) => x.def == Props.bodyPartDef).ToList()[0];
                        Hediff hediff = HediffMaker.MakeHediff(Props.hediffDef, pawn, partRecord);
                        pawn.health.AddHediff(hediff, partRecord, null, null);
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
            this.compClass = typeof(CompBodypartHediff);
        }

        public BodyPartDef bodyPartDef;
        public HediffDef hediffDef;
    }
}
