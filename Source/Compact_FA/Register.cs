using FacialAnimation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Verse;
using static FixedPawnGenerate.Compact_FacialAnimation;

namespace FixedPawnGenerate.Compact.FA;

[StaticConstructorOnStartup]
public static class Register
{

    static Register()
    {
        //注册函数
        if (Compact_FacialAnimation.IsActive)
        {
            Compact_FacialAnimation.SetFacialAnimationPropsFunc = SetFacialAnimationProps;
        }
    }

    private static void SetFacialAnimationProps(Pawn pawn, FacialAnimationPart part, string defName)
    {
        if (pawn == null || defName.NullOrEmpty())
            return;

        switch (part)
        {
            case FacialAnimationPart.Head:
                FacialAnimation.HeadTypeDef headTypeDef = DefDatabase<FacialAnimation.HeadTypeDef>.GetNamed(defName);
                if (headTypeDef != null)
                    pawn.GetComp<HeadControllerComp>().FaceType = headTypeDef;
                break;

            case FacialAnimationPart.Brow:
                FacialAnimation.BrowTypeDef browTypeDef = DefDatabase<FacialAnimation.BrowTypeDef>.GetNamed(defName);
                if (browTypeDef != null)
                    pawn.GetComp<BrowControllerComp>().FaceType = browTypeDef;
                break;

            case FacialAnimationPart.Lid:
                FacialAnimation.LidTypeDef lidTypeDef = DefDatabase<FacialAnimation.LidTypeDef>.GetNamed(defName);
                if (lidTypeDef != null)
                    pawn.GetComp<LidControllerComp>().FaceType = lidTypeDef;
                break;

            case FacialAnimationPart.Eye:
                FacialAnimation.EyeballTypeDef eyeTypeDef = DefDatabase<FacialAnimation.EyeballTypeDef>.GetNamed(defName);
                if (eyeTypeDef != null)
                    pawn.GetComp<EyeballControllerComp>().FaceType = eyeTypeDef;
                break;

            case FacialAnimationPart.LeftEyeColor:
                Color leftColor = ParseHelper.FromString<Color>(defName);
                pawn.GetComp<EyeballControllerComp>().FaceSecondColor = leftColor;
                break;

            case FacialAnimationPart.RightEyeColor:
                Color rightColor = ParseHelper.FromString<Color>(defName);
                pawn.GetComp<EyeballControllerComp>().FaceColor = rightColor;
                break;

            case FacialAnimationPart.Mouth:
                FacialAnimation.MouthTypeDef mouthTypeDef = DefDatabase<FacialAnimation.MouthTypeDef>.GetNamed(defName);
                if (mouthTypeDef != null)
                    pawn.GetComp<MouthControllerComp>().FaceType = mouthTypeDef;
                break;

            case FacialAnimationPart.Skin:
                FacialAnimation.SkinTypeDef skinTypeDef = DefDatabase<FacialAnimation.SkinTypeDef>.GetNamed(defName);
                if (skinTypeDef != null)
                    pawn.GetComp<SkinControllerComp>().FaceType = skinTypeDef;
                break;

            case FacialAnimationPart.Emotion:
                FacialAnimation.EmotionTypeDef emotionDef = DefDatabase<FacialAnimation.EmotionTypeDef>.GetNamed(defName);
                if (emotionDef != null)
                    pawn.GetComp<EmotionControllerComp>().FaceType = emotionDef;
                break;

            default:
                break;
        }
    }
}
