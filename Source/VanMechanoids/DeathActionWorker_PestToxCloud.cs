using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VanMechanoids
{
    public class DeathActionWorker_PestToxCloud : DeathActionWorker_ToxCloud
    {
        public override void PawnDied(Corpse corpse)
        {
            if (!ModsConfig.BiotechActive)
            {
                return;
            }

            if (corpse.InnerPawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null).FirstOrDefault((BodyPartRecord x) => x.def == VM_DefOf.VM_ToxinContainer) != null)
            {
                GenExplosion.DoExplosion(corpse.Position, corpse.Map, 1.9f, DamageDefOf.ToxGas, corpse.InnerPawn, -1, -1f, null, null, null, null, null, 0f, 1, new GasType?(GasType.ToxGas), false, null, 0f, 1, 0f, false, null, null, null, true, 1f, 0f, true, null, 1f);
            }
        }
    }
}
