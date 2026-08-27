using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate;

internal class Comp_OverrideShader : ThingComp
{
    public CompProperties_OverrideShader Props => (CompProperties_OverrideShader)this.props;

    private bool testActive = true;

    private Color curColor;

    private Shader hairShader;

    private List<ShaderParameter> hairShaderParameters;

    private Shader bodyShader;

    private List<ShaderParameter> bodyShaderParameters;

    private Shader apparelShader;

    private List<ShaderParameter> apparelShaderParameters;

    private Dictionary<Material, Material> materialOverrides = new Dictionary<Material, Material>();


    List<PawnRenderNodeTagDef> tagDefs = new List<PawnRenderNodeTagDef>();


    public void EditMaterial(ShaderPart part, PawnRenderNode node, PawnDrawParms parms, ref Material material)
    {
        if (testActive == false)
        {
            return;
        }

        if(node.Props.debugLabel != "头发")
        {
            return;
        }

        Shader shader = Props.testShader.Shader;
        List<ShaderParameter> parameters = Props.testShaderParameters;
        //switch (part)
        //{
        //    case ShaderPart.Hair:
        //        shader = hairShader;
        //        parameters = hairShaderParameters;
        //        break;
        //    case ShaderPart.Body:
        //        shader = bodyShader;
        //        parameters = bodyShaderParameters;
        //        break;
        //    case ShaderPart.Apparel:
        //        shader = apparelShader;
        //        parameters = apparelShaderParameters;
        //        break;
        //}
        if (shader == null)
        {
            return;
        }

        //apply
        material.shader = shader;

        foreach (ShaderParameter param in parameters)
        {
            param.Apply(material);
        }
        material.SetColor("_Color", curColor);
    }

    public override void CompTick()
    {
        if (!parent.IsHashIntervalTick(60))
        {
            return;
        }

        List<Color> colors = new List<Color>
        {
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
            Color.cyan,
            Color.magenta,
        };

        curColor = colors[(GenTicks.TicksGame / 60) % colors.Count];

        Pawn pawn = this.parent as Pawn;
        if (pawn != null)
        {
            //PortraitsCache.SetDirty(pawn);
            //GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);

            pawn.Drawer.renderer.SetAllGraphicsDirty();
        }
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        //开关
        yield return new Command_Toggle
        {
            defaultLabel = "Toggle Shader Override",
            defaultDesc = "Toggle the shader override on or off.",
            isActive = () => testActive,
            toggleAction = () =>
            {
                testActive = !testActive;
            }
        };
    }

}
