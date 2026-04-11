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
    public static List<Apparel> CustomGetApparel(Pawn pawn, List<Apparel> originalApparels)
    {
        if (pawn.TryGetComp<Comp_MirageApparel>() is Comp_MirageApparel comp)
        {
            if (!comp.GetApparel().NullOrEmpty())
            {
                return comp.GetApparel();
            }
        }

        return originalApparels;
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
                var dup = new CodeInstruction(System.Reflection.Emit.OpCodes.Dup);
                dup.MoveLabelsFrom(codes[i]); // 保护可能存在的跳转标签
                codes.Insert(i, dup);

                // 在 get_WornApparel() 之后插入我们的修改方法，其将会消耗栈上的 Pawn 和原 List<Apparel>
                codes.Insert(i + 3, new CodeInstruction(System.Reflection.Emit.OpCodes.Call, customMethod));
                i += 3; // 跳过我们插入的指令
            }
        }
        return codes;
    }
}
