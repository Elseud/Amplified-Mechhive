using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VanMechanoids
{
    public class CompShotgun : ThingComp
    {
        public CompProperties_Shotgun Props
        {
            get
            {
                return (CompProperties_Shotgun)this.props;
            }
        }
    }

    public class CompProperties_Shotgun : CompProperties
    {
        public CompProperties_Shotgun()
        {
            this.compClass = typeof(CompShotgun);
        }

        public int pelletCount;
        public float missRadius;
        public float missRange;
        public float pawnPriority;
        public float interestTilesPriority;
        public float downedPawnPriority;
    }
}
