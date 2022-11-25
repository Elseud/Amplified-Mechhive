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

namespace VanMechanoids
{
    public class Verb_CastAbilitySwarmerJump : Verb_CastAbilityJump
    {
        public float groupRange = 3f; //Cast the ability of all allies in this range on the same target

        protected override bool TryCastShot()
        {
            if (!this.ability.Activate(this.currentTarget, this.currentDestination))
            {
                return false;
            }

            CompAbility_SwarmerJumpTracker jumpTracker = ability.comps.OfType<CompAbility_SwarmerJumpTracker>().First();
            if (jumpTracker.secondaryCall || CasterPawn.IsColonyMech)
            {
                jumpTracker.secondaryCall = false;
                return JumpUtility.DoJump(this.CasterPawn, this.currentTarget, base.ReloadableCompSource, this.verbProps);
            }

            foreach (Pawn ally in this.CasterPawn.Map.mapPawns.PawnsInFaction(this.CasterPawn.Faction))
            {
                float allyDist = ally.Position.DistanceToSquared(this.CasterPawn.Position);
                if (this.CasterPawn != ally && ally.Spawned && !ally.Downed && !ally.Dead && allyDist <= groupRange * groupRange && ally.abilities.abilities.Any((Ability a) => a.def == VM_DefOf.VM_SwarmerLongjump))
                {
                    Ability allyAbility = ally.abilities.abilities.FirstOrDefault((Ability a) => a.def == VM_DefOf.VM_SwarmerLongjump);
                    if (allyAbility != null && allyAbility.CanCast && allyAbility.AICanTargetNow(this.currentTarget))
                    {
                        CompAbility_SwarmerJumpTracker allyJumpTracker = ability.comps.OfType<CompAbility_SwarmerJumpTracker>().First();
                        allyJumpTracker.secondaryCall = true;
                        ally.jobs.StartJob(allyAbility.GetJob(currentTarget, currentDestination), JobCondition.InterruptForced, null, false, true, null, null, false, false, new bool?(false), false, true);
                    }
                }
            }

            return JumpUtility.DoJump(this.CasterPawn, this.currentTarget, base.ReloadableCompSource, this.verbProps);
        }
    }
}
