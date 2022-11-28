using HotSwap;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VanMechanoids
{
    [HotSwappable]
    public class Verb_ShootShotgun : Verb_LaunchProjectile
    {
        public Dictionary<IntVec3, List<Thing>> validCells;
        public Dictionary<IntVec3, float> validCellsChances;

        public IntVec3 cachedTargetingTile;
        public float cachedTargetingDistance;
        public int cachedTargetingRadius;
        public List<IntVec3> cachedTargetingTiles;

        protected override int ShotsPerBurst
        {
            get
            {
                return this.verbProps.burstShotCount;
            }
        }

        public override void WarmupComplete()
        {
            base.WarmupComplete();
            Pawn pawn = currentTarget.Thing as Pawn;
            if (pawn != null && !pawn.Downed && !pawn.IsColonyMech && CasterIsPawn && CasterPawn.skills != null)
            {
                float num = (pawn.HostileTo(caster) ? 170f : 20f);
                float num2 = verbProps.AdjustedFullCycleTime(this, CasterPawn);
                CasterPawn.skills.Learn(SkillDefOf.Shooting, num * num2, false);
            }
        }

        protected override bool TryCastShot()
        {
            if (EquipmentSource == null || !currentTarget.HasThing)
            {
                return false;
            }

            CompShotgun comp = EquipmentSource.AllComps.Where((ThingComp x) => x is CompShotgun).FirstOrDefault() as CompShotgun;

            if (comp == null)
            {
                return false;
            }

            float distance = caster.Position.DistanceTo(currentTarget.Thing.Position);
            float radius = (float)(comp.Props.missRadius * ((distance < comp.Props.missRange) ? (distance / comp.Props.missRange) : Math.Log(distance, comp.Props.missRange) * 1.75f - 0.75f));
            int radialCells = GenRadial.NumCellsInRadius(radius);
            validCells = new Dictionary<IntVec3, List<Thing>>();
            validCellsChances = new Dictionary<IntVec3, float>();
            for (int i = 0; i < radialCells; i++)
            {
                IntVec3 cell = currentTarget.Thing.Position + GenRadial.RadialPattern[i];
                if (cell.InBounds(currentTarget.Thing.Map))
                {
                    validCells[cell] = new List<Thing>();
                    validCellsChances[cell] = 1;
                    List<Thing> trueTargets = GridsUtility.GetThingList(cell, currentTarget.Thing.Map).Where((Thing x) => (x.def.passability != Traversability.Standable || (x is Pawn))).ToList();
                    if (trueTargets.Count > 0)
                    {
                        validCellsChances[cell] *= comp.Props.interestTilesPriority;
                        Thing possiblePawn = trueTargets.Where((Thing x) => (x is Pawn)).ToList().FirstOrDefault();
                        if (possiblePawn != null)
                        {
                            validCellsChances[cell] *= comp.Props.pawnPriority * ((possiblePawn as Pawn).Downed ? comp.Props.downedPawnPriority : 1);
                        }
                    }
                }
            }

            bool flag = attemptShot(comp);
            if (flag && CasterIsPawn)
            {
                CasterPawn.records.Increment(RecordDefOf.ShotsFired);
            }

            if (flag)
            {
                if (comp != null)
                {
                    for (int i = 1; i < comp.Props.pelletCount; i++)
                    {
                        attemptShot(comp);
                    }
                }
            }
            return flag;
        }

        public override void DrawHighlight(LocalTargetInfo target)
        {
            if (target != null && target.IsValid && EquipmentSource == null)
            {
                return;
            }

            base.DrawHighlight(target);

            if (cachedTargetingTile == target.Cell)
            {
                GenDraw.DrawFieldEdges(cachedTargetingTiles);
            }
            else
            {
                cachedTargetingTile = target.Cell;
                float distance = caster.Position.DistanceToSquared(target.Cell);

                if (distance > verbProps.range * verbProps.range)
                {
                    distance = 1;
                }

                if (cachedTargetingDistance != distance)
                {
                    CompShotgun comp = EquipmentSource.AllComps.Where((ThingComp x) => x is CompShotgun).FirstOrDefault() as CompShotgun;
                    cachedTargetingDistance = distance;
                    float sqrtDistance = (float)Math.Sqrt(distance);
                    float radius = (float)(comp.Props.missRadius * ((sqrtDistance < comp.Props.missRange) ? (sqrtDistance / comp.Props.missRange) : Math.Log(sqrtDistance, comp.Props.missRange) * 1.75f - 0.75f));
                    cachedTargetingRadius = GenRadial.NumCellsInRadius(radius);
                }

                cachedTargetingTiles = new List<IntVec3>();
                for (int i = 0; i < cachedTargetingRadius; i++)
                {
                    IntVec3 cell = cachedTargetingTile + GenRadial.RadialPattern[i];
                    if (cell.InBounds(caster.MapHeld))
                    {
                        cachedTargetingTiles.Add(cell);
                    }
                }
            }

            GenDraw.DrawFieldEdges(cachedTargetingTiles);
        }

        public bool attemptShot(CompShotgun comp)
        {
            if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
            {
                return false;
            }

            ThingDef projectile = Projectile;
            if (projectile == null)
            {
                return false;
            }

            ShootLine shootLine;
            if (!base.TryFindShootLineFromTo(caster.Position, currentTarget, out shootLine))
            {
                return false;
            }

            lastShotTick = Find.TickManager.TicksGame;
            LocalTargetInfo backupTarget = currentTarget;

            ShotReport shotReport = ShotReport.HitReportFor(caster, this, currentTarget);
            if (!Rand.Chance(shotReport.AimOnTargetChance_IgnoringPosture))
            {
                ProjectileHitFlags projectileHitFlags2 = ProjectileHitFlags.NonTargetWorld;
                if (canHitNonTargetPawnsNow)
                {
                    projectileHitFlags2 |= ProjectileHitFlags.NonTargetPawns;
                }
                
                if (validCells.Count == 0)
                {
                    return false;
                }

                IntVec3 randomCell;
                validCells.Keys.ToList().TryRandomElementByWeight((IntVec3 x) => validCellsChances[x], out randomCell);
                List<Thing> validTargets = validCells[randomCell];

                if (validTargets.Count == 0)
                {
                    ShootLine shootLine2;
                    LocalTargetInfo newRandomTarget = new LocalTargetInfo(randomCell);
                    if (!base.TryFindShootLineFromTo(caster.Position, newRandomTarget, out shootLine2))
                    {
                        return false;
                    }
                    currentTarget = newRandomTarget;
                    base.TryCastShot();
                    currentTarget = backupTarget;
                    return true;
                }

                Thing randomTarget;
                validTargets.TryRandomElementByWeight((Thing x) => x.def.passability == Traversability.Impassable ? 3 : 1, out randomTarget);

                ShootLine shootLine3;
                LocalTargetInfo newRandomTarget2 = new LocalTargetInfo(randomTarget);
                if (!base.TryFindShootLineFromTo(caster.Position, newRandomTarget2, out shootLine3))
                {
                    return false;
                }

                currentTarget = newRandomTarget2;
                base.TryCastShot();
                currentTarget = backupTarget;
                return true;
            }

            base.TryCastShot();

            return true;
        }

        public Verb_ShootShotgun() { }
    }
}
