using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate;

public static class Compact_FacialAnimation
{
    public enum FacialAnimationPart
    {
        Head,
        Brow,
        Lid,
        Eye,
        LeftEyeColor,
        RightEyeColor,
        Mouth,
        Skin,
        Emotion
    }

    public static bool IsActive => ModLister.HasActiveModWithName("[NL] Facial Animation - WIP");

    public static Action<Pawn, FacialAnimationPart, string> SetFacialAnimationPropsFunc;

    public static void SetFacialAnimationProps(Pawn pawn, FacialAnimationPart part, string defName)
    {
        SetFacialAnimationPropsFunc?.Invoke(pawn, part, defName);
    }
}
