using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AmplifiedMechhive
{
    public class DeathActionWorker_PestToxCloud : DeathActionWorker
    {
        public override bool DangerousInMelee => true;

        public DeathActionWorker_PestToxCloud() { }

        public override void PawnDied(Corpse corpse)
        {
            if (corpse.InnerPawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null).FirstOrDefault((BodyPartRecord x) => x.def == AM_DefOf.AM_ToxinContainer) != null)
            {
                GenExplosion.DoExplosion(corpse.Position, corpse.Map, 1.9f, DamageDefOf.ToxGas, corpse.InnerPawn, -1, -1f, null, null, null, null, null, 0f, 1, new GasType?(GasType.ToxGas), false, null, 0f, 1, 0f, false, null, null, null, true, 1f, 0f, true, null, 1f);
            }
        }
    }
}
