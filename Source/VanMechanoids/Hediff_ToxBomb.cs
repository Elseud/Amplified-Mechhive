using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VanMechanoids
{
    public class Hediff_ToxBomb : HediffWithComps
    {
        public override void PostRemoved()
        {
            base.PostRemoved();

            if (!ModsConfig.BiotechActive)
            {
                return;
            }

            if(pawn == null || !pawn.Spawned)
            {
                return;
            }

            GenExplosion.DoExplosion(pawn.Position, pawn.Map, 1.9f, DamageDefOf.ToxGas, pawn, -1, -1f, null, null, null, null, null, 0f, 1, new GasType?(GasType.ToxGas), false, null, 0f, 1, 0f, false, null, null, null, true, 1f, 0f, true, null, 1f);
        }
    }
}
