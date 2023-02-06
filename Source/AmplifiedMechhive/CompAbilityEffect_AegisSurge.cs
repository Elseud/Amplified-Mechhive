using AthenaFramework;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using static UnityEngine.GraphicsBuffer;

namespace AmplifiedMechhive
{
    public class CompAbilityEffect_AegisSurge : CompAbilityEffect
    {
        public new CompProperties_AegisSurge Props => this.props as CompProperties_AegisSurge;

        public bool activeShield = false;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            activeShield = true;
            CompProjectileInterceptor comp = parent.pawn.GetComp<CompProjectileInterceptor>();
            if (comp != null)
            {
                comp.currentHitPoints = comp.Props.hitPoints;
                comp.nextChargeTick = -1;
            }
        }
    }

    public class CompProperties_AegisSurge : CompProperties_AbilityEffect
    {
        public CompProperties_AegisSurge()
        {
            this.compClass = typeof(CompAbilityEffect_AegisSurge);
        }
    }
}
