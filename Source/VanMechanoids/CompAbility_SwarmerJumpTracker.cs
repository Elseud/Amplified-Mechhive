using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VanMechanoids
{
    public class CompAbility_SwarmerJumpTracker : AbilityComp
    {
        public bool secondaryCall = false;

        public CompAbility_SwarmerJumpTracker() 
        {
            secondaryCall = false;
        }
    }
    public class CompProperties_SwarmerJumpTracker : AbilityCompProperties
    {
        public CompProperties_SwarmerJumpTracker()
        {
            this.compClass = typeof(CompAbility_SwarmerJumpTracker);
        }
    }
}
