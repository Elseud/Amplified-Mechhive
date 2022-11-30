using HotSwap;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace VanMechanoids
{
    [HotSwap.HotSwappable]
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
            if (EquipmentSource == null || currentTarget.Cell == null)
            {
                return false;
            }

            ShotgunExtension extension = EquipmentSource.def.GetModExtension<ShotgunExtension>();

            if (extension == null)
            {
                return false;
            }

            bool flag = base.TryCastShot();
            if (!flag)
            {
                return false;
            }

            float angle = (float)Math.Acos(Vector2.Dot((new Vector2(currentTarget.Cell.x, currentTarget.Cell.z) - new Vector2(CasterPawn.Position.x, CasterPawn.Position.z)).normalized, new Vector2(1, 0)));
            float pelletAngle = (float)(extension.pelletAngle * (Math.PI) / 180);
            for (int i = 0; i < extension.pelletCount - 1; i++)
            {
                if (i == (extension.pelletCount - 1) / 2 + 1)
                {
                    continue;
                }

                float newAngle = angle - pelletAngle * (extension.pelletCount - 1) / 2 + i * pelletAngle;
                IntVec3 newPos = (new IntVec3((int)(Math.Cos(newAngle) * verbProps.range), 0, (int)(Math.Sin(newAngle) * verbProps.range))) + CasterPawn.Position;
                Thing targeting = null;

                foreach (IntVec3 intVec in GenSight.PointsOnLineOfSight(CasterPawn.Position, newPos))
                {
                    if (intVec == CasterPawn.Position)
                    {
                        continue;
                    }

                    Thing targetBuilding = intVec.GetRoofHolderOrImpassable(CasterPawn.Map);
                    if (targetBuilding != null)
                    {
                        targeting = targetBuilding;
                        break;
                    }

                    Thing cover = newPos.GetCover(CasterPawn.Map);
                    if (cover != null)
                    {
                        if (Rand.Chance(cover.BaseBlockChance()))
                        {
                            targeting = targetBuilding;
                            break;
                        }
                    }

                    bool foundBreak = false;
                    foreach (Thing possibleTarget in GridsUtility.GetThingList(intVec, CasterPawn.Map))
                    {
                        if (possibleTarget is Pawn)
                        {
                            Pawn pawn = possibleTarget as Pawn;
                            if (pawn.Downed || pawn.Dead)
                            {
                                if (Rand.Chance(0.25f))
                                {
                                    targeting = pawn;
                                    foundBreak = true;
                                    break;
                                }
                            }
                            else
                            {
                                targeting = pawn;
                                foundBreak = true;
                                break;
                            }
                        }
                    }

                    if (foundBreak)
                    {
                        break;
                    }
                }

                if (targeting != null)
                {
                    LocalTargetInfo newTarget = new LocalTargetInfo(targeting);
                    currentTarget = newTarget;
                    currentDestination = newTarget;
                    base.TryCastShot();
                }
                else
                {
                    if(!base.TryFindShootLineFromTo(this.caster.Position, new LocalTargetInfo(newPos), out ShootLine shootLine))
                    {
                        continue;
                    }

                    Projectile projectile = (Projectile)GenSpawn.Spawn(Projectile, shootLine.Source, CasterPawn.Map, WipeMode.Vanish);
                    projectile.Launch(CasterPawn, CasterPawn.DrawPos, newPos, currentTarget, ProjectileHitFlags.All, this.preventFriendlyFire, EquipmentSource, null);
                }
            }

            return true;
        }

        public Verb_ShootShotgun() { }
    }
}
