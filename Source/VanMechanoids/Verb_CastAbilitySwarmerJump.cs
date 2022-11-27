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
using VanillaWeaponsExpandedLaser;

namespace VanMechanoids
{
    public class Verb_CastAbilitySwarmerJump : Verb_CastAbilityJump
    {
        protected override bool TryCastShot()
        {
            if (groupSwarmerTracker.groupedPawns.Contains(CasterPawn))
            {
                groupSwarmerTracker.groupedPawns.Remove(CasterPawn);
            }

            if (!ability.Activate(currentTarget, currentDestination))
            {
                return false;
            }


            return JumpUtility.DoJump(this.CasterPawn, this.currentTarget, base.ReloadableCompSource, this.verbProps);
        }
    }
}
