using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using RimWorld;

namespace EL_MechAI
{
    public class JobGiver_AICastAbilityOnMechFactionMembers : JobGiver_AICastAbility
    {
        Random rand = new Random();

        public JobGiver_AICastAbilityOnMechFactionMembers() { }

        static JobGiver_AICastAbilityOnMechFactionMembers() { }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.CurJobDef == this.ability.jobDef)
            {
                return (Job)null;
            }
                
            Ability ability = pawn.abilities?.GetAbility(this.ability);
            if (ability == null || !ability.CanCast)
            {
                return (Job)null;
            }
                
            LocalTargetInfo target = this.GetTarget(pawn, ability);
            if (target.Thing.Faction != null)
            {
                return (Job)null;
            }
                
            if (target.Thing.Faction.AllyOrNeutralTo(Faction.OfMechanoids) == true)
            {
                return (Job)null;
            }
                
            if (target.Thing.Faction.HostileTo(Faction.OfMechanoids) == true)
            {
                return (Job)null;
            }

            if (!target.IsValid)
            {
                return (Job)null;
            }

            return ability.GetJob(target, target);
        }

        protected override LocalTargetInfo GetTarget(Pawn caster, Ability ability)
        {
            float range = ability.def.verbProperties.range;
            List<Pawn> friendlyMechs = caster.Map.mapPawns.SpawnedPawnsInFaction(caster.Faction).Where((Pawn x) => x.Position.DistanceTo(caster.Position) <= range && x.RaceProps.IsMechanoid).ToList();
            return new LocalTargetInfo(friendlyMechs[rand.Next(friendlyMechs.Count())]);
        }
    }
}
