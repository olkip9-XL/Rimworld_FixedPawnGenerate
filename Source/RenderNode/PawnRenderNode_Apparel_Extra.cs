using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate;

internal class PawnRenderNode_Apparel_Extra : PawnRenderNode_Apparel
{
    public PawnRenderNode_Apparel_Extra(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
    {
    }

    public PawnRenderNode_Apparel_Extra(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree, Apparel apparel) : base(pawn, props, tree, apparel)
    {
    }

    public PawnRenderNode_Apparel_Extra(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree, Apparel apparel, bool useHeadMesh) : base(pawn, props, tree, apparel, useHeadMesh)
    {
    }

    protected override IEnumerable<Graphic> GraphicsFor(Pawn pawn)
    {
        Graphic graphic = GetGraphicApparel(apparel, tree.pawn.story.bodyType, pawn.Drawer.renderer.StatueColor.HasValue);
        if (graphic != null)
        {
            yield return graphic;
        }

        yield break;
    }

    private Graphic GetGraphicApparel(Apparel apparel, BodyTypeDef bodyType, bool forStatue)
    {
        if (bodyType == null)
        {
            Log.Error("Getting apparel graphic with undefined body type.");
            bodyType = BodyTypeDefOf.Male;
        }

        if (props.texPath.NullOrEmpty())
        {
            return null;
        }

        string path = ((apparel.def.apparel.LastLayer != ApparelLayerDefOf.Overhead && apparel.def.apparel.LastLayer != ApparelLayerDefOf.EyeCover && !apparel.RenderAsPack() && !(props.texPath == BaseContent.PlaceholderImagePath) && !(props.texPath == BaseContent.PlaceholderGearImagePath)) ? (props.texPath + "_" + bodyType.defName) : props.texPath);
        Shader shader = ShaderDatabase.Cutout;
        if (!forStatue)
        {
            if (apparel.StyleDef?.graphicData.shaderType != null)
            {
                shader = apparel.StyleDef.graphicData.shaderType.Shader;
            }
            else if ((apparel.StyleDef == null && apparel.def.apparel.useWornGraphicMask) || (apparel.StyleDef != null && apparel.StyleDef.UseWornGraphicMask))
            {
                shader = ShaderDatabase.CutoutComplex;
            }
        }

        Color color = Props.color.HasValue ? Props.color.Value : apparel.DrawColor;

        return GraphicDatabase.Get<Graphic_Multi>(path, shader, apparel.def.graphicData.drawSize, color);
    }




}
