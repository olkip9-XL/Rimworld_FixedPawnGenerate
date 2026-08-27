using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate;

internal static class Harmony_OverrideShader
{
    private static void TryEditMaterial(ShaderPart part, PawnRenderNode node, PawnDrawParms parms, ref Material material)
    {
        if (material == null)
        {
            return;
        }

        Pawn pawn = node?.tree?.pawn;
        if(pawn == null)
        {
            return;
        }

        var comp = pawn.TryGetComp<Comp_OverrideShader>();
        if (comp == null)
        {
            return;
        }

        comp.EditMaterial(part, node, parms, ref material);
    }


    //Hair
    [HarmonyPatch(typeof(PawnRenderNodeWorker), "GetMaterial")]
    static class Patch_PawnRenderNodeWorker_GetMaterial
    {
        static void Postfix(PawnRenderNode node, PawnDrawParms parms, ref Material __result)
        {
           TryEditMaterial(ShaderPart.Hair, node, parms, ref __result);
        }
    }

}
