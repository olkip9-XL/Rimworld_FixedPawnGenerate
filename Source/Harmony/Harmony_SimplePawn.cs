using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate
{

    [HarmonyPatch(typeof(PawnRenderNode), "AddChildren")]
    internal static class Harmony_SimplePawn
    {
        //remove facial animation nodes
        static void Postfix(PawnRenderNode __instance, ref PawnRenderNode[] ___children, ref PawnRenderTree ___tree)
        {
            if (___children.Any(x => x is PawnRenderNode_SimplePawn))
            {
                List<PawnRenderNode> list = ___children.ToList();
                list.RemoveAll(x => x.GetType().Name.Contains("NLFacialAnimation"));
                ___children = list.ToArray();
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderTree), "TrySetupGraphIfNeeded")]
    public static class PawnRenderTree_TrySetupGraphIfNeeded_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            FieldInfo rootField = AccessTools.Field(typeof(PawnRenderTreeDef), nameof(PawnRenderTreeDef.root));
            FieldInfo pawnField = AccessTools.Field(typeof(PawnRenderTree), nameof(PawnRenderTree.pawn));
            MethodInfo modifyMethod = AccessTools.Method(typeof(PawnRenderTree_TrySetupGraphIfNeeded_Patch), nameof(ModifyNodeProperties));

            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];

                if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand is FieldInfo fInfo && fInfo == rootField)
                {
                    int stlocIndex = i + 1;
                    yield return codes[stlocIndex];

                    // ---- 插入开始 ----
                    yield return new CodeInstruction(OpCodes.Ldloc_0);

                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, pawnField);

                    yield return new CodeInstruction(OpCodes.Call, modifyMethod);

                    yield return new CodeInstruction(OpCodes.Stloc_0);
                    // ---- 插入结束 ----
                    i++;
                }
            }
        }

        public static PawnRenderNodeProperties ModifyNodeProperties(PawnRenderNodeProperties originalProps, Pawn pawn)
        {
            if (pawn.HasComp<Comp_SimplePawn>())
            {
                return FPG_RenderTreeDefOf.SimplePawn.root;
            }

            return originalProps;
        }
    }
}
