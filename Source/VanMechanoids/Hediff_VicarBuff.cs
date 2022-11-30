using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VanMechanoids
{
    public class Hediff_VicarBuff : HediffWithComps
    {
        public Pawn beamOwner;
        public float damageMultiplier = 1.2f;

        public override void Notify_PawnKilled()
        {
            if (beamOwner != null && pawn != null && pawn.MapHeld != null)
            {
                VicarBeamHolder holder = pawn.MapHeld.GetComponent<VicarBeamHolder>();
                holder.DespawnBeam(beamOwner);
            }
            
            base.Notify_PawnKilled();
        }

        public override void Notify_KilledPawn(Pawn victim, DamageInfo? dinfo)
        {
            base.Notify_KilledPawn(victim, dinfo);
        }
    }

    public class Hediff_Vicar : HediffWithComps
    {
        public override void Tick()
        {
            base.Tick();

            if (pawn != null && pawn.MapHeld != null && pawn.health.Downed)
            {
                VicarBeamHolder holder = pawn.MapHeld.GetComponent<VicarBeamHolder>();
                holder.DespawnBeam(pawn);
            }
        }

        public override void Notify_PawnKilled()
        {
            if (pawn != null && pawn.MapHeld != null)
            {
                VicarBeamHolder holder = pawn.MapHeld.GetComponent<VicarBeamHolder>();
                holder.DespawnBeam(pawn);
            }

            base.Notify_PawnKilled();
        }

        public override void Notify_KilledPawn(Pawn victim, DamageInfo? dinfo)
        {
            base.Notify_KilledPawn(victim, dinfo);
        }
    }
}
