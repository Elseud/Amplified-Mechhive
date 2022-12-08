using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

[DefOf]
public static class AM_DefOf
{
    public static AbilityDef AM_SwarmerLongjump;
    public static BodyPartDef AM_ToxinContainer;
    public static BodyPartDef AM_FrontShield;
    public static HediffDef AM_ToxBomb;
    public static HediffDef AM_EMPShield;
    public static DamageDef AM_ShieldExplosion;
    public static HediffDef AM_VicarBuff;
    public static HediffDef AM_VicarTracker;
    public static HediffDef AM_Apotheosis;
    public static EffecterDef AM_ApotheosisCast;
    public static ThingDef Mote_MechApotheosisWarmupOnTarget;
    public static ThingDef Mote_VicarBeamBase;
    public static JobDef ProtectBishop;
    public static PawnKindDef AM_Bishop;

    static AM_DefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(AM_DefOf));
    }
}
