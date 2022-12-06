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
    }
}
