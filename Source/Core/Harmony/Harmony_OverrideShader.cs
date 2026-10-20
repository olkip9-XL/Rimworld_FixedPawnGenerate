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
    private static void TryEditMaterial(PawnRenderNode node, ref Material material)
    {
        TryEditMaterial(node?.Props?.debugLabel, node?.tree?.pawn, ref material);
    }

    private static void TryEditMaterial(string nodeLabel, Pawn pawn, ref Material material)
    {
        if (material == null || nodeLabel == null || pawn == null)
        {
            return;
        }

        var comp = pawn.TryGetComp<Comp_OverrideShader>();
        if (comp == null)
        {
            return;
        }

        Material material2 = comp.EditMaterial(nodeLabel, ref material);
        if (material2 != null)
        {
            material = material2;
        }
    }


    //Base
    [HarmonyPatch(typeof(PawnRenderNodeWorker), "GetMaterial")]
    static class Patch_PawnRenderNodeWorker_GetMaterial
    {
        static void Postfix(PawnRenderNode node, PawnDrawParms parms, ref Material __result)
        {
            TryEditMaterial(node, ref __result);
        }
    }

}
