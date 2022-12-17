using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using HarmonyLib;
using RimWorld;

namespace AmplifiedMechhive
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony(id: "rimworld.sirvan.amplifiedmechhive.main");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(DamageWorker_AddInjury), "ApplyToPawn")]
        public static class DamageWorker_AddInjury_PreApplyToPawn
        {
            static bool Prefix(DamageWorker_AddInjury __instance, DamageWorker.DamageResult __result, ref DamageInfo dinfo, ref Pawn pawn)
            {
                if (dinfo.Def == AM_DefOf.AM_ShieldExplosion)
                {
                    if (dinfo.Instigator is Pawn)
                    {
                        Pawn instigator = dinfo.Instigator as Pawn;
                        if (!instigator.Faction.HostileTo(pawn.Faction))
                        {
                            dinfo.SetAmount(0);
                            return true;
                        }
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(CompProjectileInterceptor), "ShouldDisplayGizmo", MethodType.Getter)]
        public static class CompProjectileInterceptor_ShouldDisplayGizmo
        {
            static void Postfix(CompProjectileInterceptor __instance, ref bool __result)
            {
                if (__instance.parent is not Pawn)
                {
                    return;
                }

                Pawn pawn = __instance.parent as Pawn;
                Ability ability = pawn.abilities.GetAbility(AM_DefOf.AM_AegisSurge);
                if (ability == null)
                {
                    return;
                }

                CompAbilityEffect_AegisSurge comp = ability.CompOfType<CompAbilityEffect_AegisSurge>();

                if (!comp.activeShield)
                {
                    __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(CompProjectileInterceptor), "Active", MethodType.Getter)]
        public static class CompProjectileInterceptor_Active
        {
            static void Postfix(CompProjectileInterceptor __instance, ref bool __result)
            {
                if (__instance.parent is not Pawn)
                {
                    return;
                }

                Pawn pawn = __instance.parent as Pawn;
                Ability ability = pawn.abilities.GetAbility(AM_DefOf.AM_AegisSurge);
                if (ability == null)
                {
                    return;
                }

                CompAbilityEffect_AegisSurge comp = ability.CompOfType<CompAbilityEffect_AegisSurge>();

                if (!comp.activeShield)
                {
                    __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(CompProjectileInterceptor), "BreakShieldHitpoints")]
        public static class CompProjectileInterceptor_BreakShieldHitpoints
        {
            static void Postfix(CompProjectileInterceptor __instance)
            {
                if (__instance.parent is not Pawn)
                {
                    return;
                }

                Pawn pawn = __instance.parent as Pawn;
                Ability ability = pawn.abilities.GetAbility(AM_DefOf.AM_AegisSurge);
                if (ability == null)
                {
                    return;
                }

                CompAbilityEffect_AegisSurge comp = ability.CompOfType<CompAbilityEffect_AegisSurge>();
                comp.activeShield = false;
            }
        }
    }
}
