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
    public static AbilityDef VM_CritterLongjump;

    static VM_DefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(VM_DefOf));
    }
}
