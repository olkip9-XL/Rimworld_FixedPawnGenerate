using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace FixedPawnGenerate;

[HarmonyPatch(typeof(Building_StylingStation), "GetFloatMenuOptions")]
internal static class Patch_Building_StylingStation_GetFloatMenuOptions
{
    private static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> __result, Building_StylingStation __instance, Pawn selPawn)
    {
        //original options
        List<FloatMenuOption> list = Enumerable.ToList<FloatMenuOption>(__result);
        foreach (FloatMenuOption floatMenuOption in __result)
        {
            yield return floatMenuOption;
        }
        if (Enumerable.Count<FloatMenuOption>(list) == 1 && list[0].action == null)
        {
            yield break;
        }

        //toggle mirage apparel
        if (selPawn != null && selPawn.GetComp<Comp_MirageApparel>() != null)
        {
            FloatMenuOption option = new FloatMenuOption("FPG_ToggleMirageApparel".Translate(), delegate ()
            {
                Job job = JobMaker.MakeJob(FPG_JobDefOf.FPG_ToggleMirageApparel, __instance);

                selPawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
            }, MenuOptionPriority.InitiateSocial, null, null, 0f, null, null, true, 0);
            yield return FloatMenuUtility.DecoratePrioritizedTask(option, selPawn, __instance, "ReservedBy", null);
        }

        //switch apparel style
        if (selPawn != null && selPawn.apparel != null && selPawn.TryGetComp<Comp_SwitchStyle>() is Comp_SwitchStyle comp)
        {
            //base style
            FloatMenuOption baseOption = new FloatMenuOption("FPG_SwitchStyle".Translate("FPG_BaseStyle".Translate()), delegate ()
            {
                Job job = JobMaker.MakeJob(FPG_JobDefOf.FPG_SwitchStyle, __instance);
                //借用这个count字段传递样式索引，-1代表基础样式
                job.count = -1;
                selPawn.jobs.TryTakeOrderedJob(job);
            }, MenuOptionPriority.InitiateSocial);
            yield return FloatMenuUtility.DecoratePrioritizedTask(baseOption, selPawn, __instance, "ReservedBy", null);

            //alt styles
            for (int i = 0; i < comp.Props.altStyles.Count; i++)
            {
                var style = comp.Props.altStyles[i];
                int styleIndex = i; // local copy for closure

                FloatMenuOption option = new FloatMenuOption("FPG_SwitchStyle".Translate(style.label), delegate ()
                {
                    Job job = JobMaker.MakeJob(FPG_JobDefOf.FPG_SwitchStyle, __instance);
                    job.count = styleIndex;
                    selPawn.jobs.TryTakeOrderedJob(job);
                }, MenuOptionPriority.InitiateSocial);
                yield return FloatMenuUtility.DecoratePrioritizedTask(option, selPawn, __instance, "ReservedBy", null);
            }
        }

        yield break;
    }


}
