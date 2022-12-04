using AthenaFramework;
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
    public class CompAbilityEffect_SwarmerJump : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (GroupSwarmerTracker.groupedPawns.Contains(parent.pawn))
            {
                GroupSwarmerTracker.groupedPawns.Remove(parent.pawn);
            }
        }
    }

    public class CompProperties_AbilitySwarmerJump : CompProperties_AbilityEffect
    {
        public CompProperties_AbilitySwarmerJump()
        {
            this.compClass = typeof(CompAbilityEffect_SwarmerJump);
        }
    }
}
