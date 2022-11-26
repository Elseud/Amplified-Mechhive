using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using Verse.Sound;
using UnityEngine;

namespace VanMechanoids
{
    public class Projectile_PestSpit : Bullet
    {
        HediffDef buildupHediffDef = HediffDefOf.ToxicBuildup;
        StatDef scalingDef = StatDefOf.ToxicResistance;
        float buildupAmount = 0.1f;

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            base.Impact(hitThing, blockedByShield);

            if (hitThing != null)
            {
                if (def.projectile.filth != null && def.projectile.filthCount.TrueMax > 0 && hitThing.Position != null && hitThing.MapHeld != null)
                {
                    FilthMaker.TryMakeFilth(hitThing.Position, hitThing.MapHeld, def.projectile.filth, def.projectile.filthCount.RandomInRange, FilthSourceFlags.None, true);
                }

                if (!blockedByShield && (hitThing is Pawn))
                {
                    Pawn hitPawn = hitThing as Pawn;
                    Hediff buildupHediff = hitPawn.health.hediffSet.GetFirstHediffOfDef(buildupHediffDef, false);
                    float hediffMultiplier = 1f;

                    hediffMultiplier *= 1f / hitPawn.BodySize;
                    if (scalingDef != null)
                    {
                        hediffMultiplier *= Mathf.Max(1f - hitPawn.GetStatValue(scalingDef), 0f);
                    }

                    if (hediffMultiplier > 0)
                    {
                        if (buildupHediff != null)
                        {
                            buildupHediff.Severity = Math.Min(buildupHediff.Severity + buildupAmount * hediffMultiplier, 1);
                        }
                        else
                        {
                            buildupHediff = HediffMaker.MakeHediff(buildupHediffDef, hitPawn, null);
                            buildupHediff.Severity = buildupAmount * hediffMultiplier;
                            hitPawn.health.AddHediff(buildupHediff, null, null, null);
                        }
                    }
                }
            }
        }
    }
}
