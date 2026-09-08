using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate;
public class TransformWorker_Shader : TransformWorker
{
    public override bool Match(string label, Pawn pawn)
    {
        if (subTransformDef == null || subTransformDef.debugLabels.NullOrEmpty())
        {
            return false;
        }

        foreach (string debugLabel in subTransformDef.debugLabels)
        {
            if (debugLabel.NullOrEmpty())
            {
                continue;
            }

            if (debugLabel == "All")
            {
                return true;
            }

            if (debugLabel == "Apparel" && IsApparelNode(label, pawn))
            {
                return true;
            }

            if (debugLabel == label)
            {
                return true;
            }
        }

        return false;
    }

    public override bool Update(int elapsed, Pawn pawn)
    {
        return false;
    }

    public override bool UpdateMaterial(ref Material material, int elapsed)
    {
        if (material == null || subTransformDef == null)
        {
            return false;
        }

        bool changed = false;
        if (!subTransformDef.shaderParameters.NullOrEmpty())
        {
            foreach (ShaderParameter parameter in subTransformDef.shaderParameters)
            {
                parameter?.Apply(material);
                changed = true;
            }
        }

        if (subTransformDef.floatParamTransforms.NullOrEmpty())
        {
            return changed;
        }

        float localProgress = Mathf.Clamp01((elapsed - subTransformDef.startTick) / (float)subTransformDef.durationTicks);
        foreach (FloatShaderParamTransform floatTransform in subTransformDef.floatParamTransforms)
        {
            if (floatTransform == null || floatTransform.paramName.NullOrEmpty())
            {
                continue;
            }

            if (!material.HasProperty(floatTransform.paramName))
            {
                continue;
            }

            float curveValue = floatTransform.curve?.Evaluate(localProgress) ?? localProgress;
            material.SetFloat(floatTransform.paramName, curveValue);
            changed = true;
        }

        return changed;
    }

    private static bool IsApparelNode(string nodeLabel, Pawn pawn)
    {
        if (pawn == null || pawn.apparel == null || pawn.apparel.WornApparel == null)
        {
            return false;
        }

        return pawn.apparel.WornApparel.Any(x => x.def.defName == nodeLabel);
    }

}
