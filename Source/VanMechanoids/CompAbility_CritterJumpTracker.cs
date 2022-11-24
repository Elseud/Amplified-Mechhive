using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VanMechanoids
{
    public class CompAbility_CritterJumpTracker : AbilityComp
    {
        public bool secondaryCall = false;

        public CompAbility_CritterJumpTracker() 
        {
            secondaryCall = false;
        }
    }
    public class CompProperties_CritterJumpTracker : AbilityCompProperties
    {
        public CompProperties_CritterJumpTracker()
        {
            this.compClass = typeof(CompAbility_CritterJumpTracker);
        }
    }
}
