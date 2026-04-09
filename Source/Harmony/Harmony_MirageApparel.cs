using HarmonyLib;
using KTrie;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
namespace FixedPawnGenerate;

[HarmonyPatch(typeof(DynamicPawnRenderNodeSetup_Apparel), "GetDynamicNodes", MethodType.Enumerator)]
internal static class Patch_DynamicPawnRenderNodeSetup_Apparel_GetDynamicNodes
{
    public static List<Apparel> CustomGetApparel(Pawn pawn)
    {
        if (pawn.TryGetComp<Comp_MirageApparel>() is Comp_MirageApparel comp)
        {
            if (!comp.GetApparel().NullOrEmpty())
            {
                return comp.GetApparel();
            }
        }

        return pawn.apparel.WornApparel;
    }

    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var apparelField = AccessTools.Field(typeof(Pawn), nameof(Pawn.apparel));
        var wornApparelGetter = AccessTools.PropertyGetter(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.WornApparel));
        var customMethod = AccessTools.Method(typeof(Patch_DynamicPawnRenderNodeSetup_Apparel_GetDynamicNodes), nameof(CustomGetApparel));

        var codes = instructions.ToList();
        for (int i = 0; i < codes.Count - 1; i++)
        {
            // 查找 ldfld pawn.apparel 接着调用 get_WornApparel() 
            if (codes[i].LoadsField(apparelField) && codes[i + 1].Calls(wornApparelGetter))
            {
                // 将 ldfld pawn.apparel 替换为 Nop，这样 Pawn 的实例会留在栈上
                codes[i].opcode = System.Reflection.Emit.OpCodes.Nop;
                codes[i].operand = null;
                // 将 get_WornApparel() 的调用替换为对自定义函数的调用
                codes[i + 1].opcode = System.Reflection.Emit.OpCodes.Call;
                codes[i + 1].operand = customMethod;
            }
        }
        return codes;
    }
}
