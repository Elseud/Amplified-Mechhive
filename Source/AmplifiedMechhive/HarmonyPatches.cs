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

        /*
        [HarmonyPatch(typeof(StunHandler), "Notify_DamageApplied")]
        public static class ThingWithComps_PreApplyDamage
        {
            static bool Prefix(StunHandler __instance, ref DamageInfo dinfo)
            {
                if (!(__instance.parent is Pawn))
                    return true;

                Pawn pawn = __instance.parent as Pawn;

                foreach (HediffComp_EMPShield shield in pawn.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany(hediff => hediff.comps).OfType<HediffComp_EMPShield>())
                {
                    if (shield.Notify_DamageApplied(ref dinfo))
                        return false;
                }

                return true;
            }
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

                if (dinfo.Instigator is Pawn)
                {
                    Pawn instigator = dinfo.Instigator as Pawn;
                    Hediff_VicarBuff buff = instigator.health.hediffSet.GetFirstHediffOfDef(AM_DefOf.AM_VicarBuff) as Hediff_VicarBuff;
                    if (buff != null)
                        dinfo.SetAmount(dinfo.Amount * buff.damageMultiplier);
                }
                
                return true;
            }
        }*/
    }
}
