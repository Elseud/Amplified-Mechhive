using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace VanMechanoids
{
    public class ThinkNodeConditional_UnderGroupCombatPressure : ThinkNode_Conditional
    {
        public float maxThreatDistance = 3f;
        public float maxAllyDistance = 3f;
        public int soloMinPawns = 3; //Minimal amount of hostile pawns which is required the condition would be fulfilled
        public int groupPawnMultiplier = 2; //How much hostile pawns are added to the minimum per ally nearby. With default settings, it will be 3 with 0 allies, 4 with 1, 6 with 2 and etc

        public ThinkNodeConditional_UnderGroupCombatPressure() { }

        protected override bool Satisfied(Pawn pawn)
        {
            int allies = 1;

            foreach (Pawn ally in pawn.Map.mapPawns.PawnsInFaction(pawn.Faction))
            {
                if (pawn != ally && ally.Spawned && !ally.Downed && !ally.Dead && ally.Position.DistanceToSquared(pawn.Position) <= maxAllyDistance * maxAllyDistance)
                {
                    allies++;
                }
            }

            TraverseParms tp = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false, false, false);
            int hostileCount = 0;
            RegionTraverser.BreadthFirstTraverse(pawn.Position, pawn.Map, (Region from, Region to) => to.Allows(tp, false), delegate (Region r)
            {
                List<Thing> possibleHostiles = r.ListerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
                for (int i = 0; i < possibleHostiles.Count; i++)
                {
                    if (possibleHostiles[i].HostileTo(pawn) && (maxThreatDistance <= 0f || possibleHostiles[i].Position.InHorDistOf(pawn.Position, maxThreatDistance)))
                    {
                        hostileCount += 1;
                    }
                }
                return true;
            }, (int)((maxThreatDistance * 2 + 1) * (maxThreatDistance * 2 + 1)), RegionType.Set_Passable);

            return pawn.Spawned && !pawn.Downed && (hostileCount >= Math.Max(soloMinPawns, allies * groupPawnMultiplier));
        }

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNodeConditional_UnderGroupCombatPressure thinkNodeConditional_UnderGroupCombatPressure = (ThinkNodeConditional_UnderGroupCombatPressure)base.DeepCopy(resolve);
            thinkNodeConditional_UnderGroupCombatPressure.maxThreatDistance = this.maxThreatDistance;
            thinkNodeConditional_UnderGroupCombatPressure.maxAllyDistance = this.maxAllyDistance;
            thinkNodeConditional_UnderGroupCombatPressure.soloMinPawns = this.soloMinPawns;
            thinkNodeConditional_UnderGroupCombatPressure.groupPawnMultiplier = this.groupPawnMultiplier;
            return thinkNodeConditional_UnderGroupCombatPressure;
        }
    }
}
