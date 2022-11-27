using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

[DefOf]
public static class VM_DefOf
{
    public static AbilityDef VM_SwarmerLongjump;
    public static BodyPartDef VM_ToxinContainer;
    public static BodyPartDef VM_FrontShield;
    public static HediffDef VM_ToxBomb;
    public static HediffDef VM_EMPShield;
    public static DamageDef VM_ShieldExplosion;

    static VM_DefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(VM_DefOf));
    }
}
