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
    [HarmonyPatch(typeof(Pawn_StyleTracker), "get_HasAnyUnwantedStyleItem")]
    internal static class Patch_PawnStyleTracker_HasAnyUnwantedStyleItem
    {
        static void Postfix(Pawn_StyleTracker __instance, ref bool __result)
        {
            Pawn pawn = __instance.pawn;
            if (pawn != null && pawn.HasComp<Comp_SwitchStyle>())
            {
                __result = false;
            }
        }
    }
}
