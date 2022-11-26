using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VanMechanoids
{
    public class HediffComp_ToxBomb : HediffComp
    {
        private HediffCompProperties_ToxBomb Props
        {
            get
            {
                return (HediffCompProperties_ToxBomb)this.props;
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            if (!ModsConfig.BiotechActive)
            {
                return;
            }

            if(parent.pawn == null || !parent.pawn.Spawned)
            {
                return;
            }

            GenExplosion.DoExplosion(parent.pawn.Position, parent.pawn.Map, 1.9f, DamageDefOf.ToxGas, parent.pawn, -1, -1f, null, null, null, null, null, 0f, 1, new GasType?(GasType.ToxGas), false, null, 0f, 1, 0f, false, null, null, null, true, 1f, 0f, true, null, 1f);
        }
    }

    public class HediffCompProperties_ToxBomb : HediffCompProperties
    {
        public HediffCompProperties_ToxBomb()
        {
            this.compClass = typeof(HediffComp_ToxBomb);
        }
    }
}
