using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate;

public class TransformWorker_Style : TransformWorker
{
    public override bool Update(int elapsed, Pawn pawn)
    {
        if (subTransformDef == null || pawn == null || !Compact_FacialAnimation.IsActive)
        {
            return false;
        }

        bool changed = false;

        //眼睛颜色
        if (subTransformDef.leftEyeColor.HasValue)
        {
            Compact_FacialAnimation.SetFacialAnimationProps(pawn, Compact_FacialAnimation.FacialAnimationPart.LeftEyeColor, subTransformDef.leftEyeColor.Value.ToString());
            changed = true;
        }

        if (subTransformDef.rightEyeColor.HasValue)
        {
            Compact_FacialAnimation.SetFacialAnimationProps(pawn, Compact_FacialAnimation.FacialAnimationPart.RightEyeColor, subTransformDef.rightEyeColor.Value.ToString());
            changed = true;
        }

        //头发
        if (subTransformDef?.hair != null && pawn?.story != null && pawn.story.hairDef != subTransformDef.hair)
        {
            pawn.story.hairDef = subTransformDef.hair;
            changed = true;
        }

        return changed;
    }
}
