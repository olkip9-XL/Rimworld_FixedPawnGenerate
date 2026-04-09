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

        yield break;
    }


}
