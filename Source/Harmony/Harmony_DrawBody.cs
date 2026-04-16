using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using RimWorld;
using Verse;

namespace FixedPawnGenerate
{

    [HarmonyPatch(typeof(PawnRenderNode_Body), "GraphicFor")]
    internal static class PawnRenderNode_Body_GraphicFor
    {
        static bool Prefix(PawnRenderNode_Body __instance, ref Graphic __result, Pawn pawn)
        {
            Comp_OverrideBodyGraphic comp = pawn.GetComp<Comp_OverrideBodyGraphic>();
            if (comp == null || comp.Props.graphicPath == null)
            {
                //不跳过
                return true;
            }

            if ((pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated) ||
                (pawn.IsMutant && !pawn.mutant.Def.bodyTypeGraphicPaths.NullOrEmpty()) ||
                (ModsConfig.AnomalyActive && pawn.IsCreepJoiner && pawn.story.bodyType != null && !pawn.creepjoiner.form.bodyTypeGraphicPaths.NullOrEmpty()))
            {
                return true;
            }

            __result = comp.GetGraphic(pawn, __instance.ShaderFor(pawn), __instance.ColorFor(pawn));

            return false;
        }

    }
}
