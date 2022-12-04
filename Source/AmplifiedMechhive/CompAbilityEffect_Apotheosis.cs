using HotSwap;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using static UnityEngine.GraphicsBuffer;

namespace AmplifiedMechhive
{
    public class CompProperties_AbilityApotheosis : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityApotheosis()
        {
            this.compClass = typeof(CompAbilityEffect_Apotheosis);
        }

        public HediffDef hediffDef;
    }

    public class CompAbilityEffect_Apotheosis : CompAbilityEffect
    {
        public new CompProperties_AbilityApotheosis Props => this.props as CompProperties_AbilityApotheosis;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = target.Pawn;

            if (pawn == null)
            {
                return;
            }

            pawn.health.AddHediff(HediffMaker.MakeHediff(Props.hediffDef, pawn));
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return this.Valid(target, false);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn pawn = target.Pawn;

            if (pawn == null)
            {
                return false;
            }

            if (pawn.Faction == null || pawn.def.race == null)
            {
                return false;
            }

            if (pawn.Faction != parent.pawn.Faction)
            {
                return false;
            }

            if (!pawn.def.race.IsMechanoid)
            {
                return false;
            }

            if (pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef) != null)
            {
                return false;
            }

            return true;
        }

        public CompAbilityEffect_Apotheosis()
        {
        }
    }
}
