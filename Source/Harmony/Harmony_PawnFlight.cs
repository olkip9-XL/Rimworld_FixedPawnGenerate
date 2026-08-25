using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using RimWorld;
using Verse;

namespace FixedPawnGenerate;

[HarmonyPatch(typeof(Pawn_FlightTracker), "Notify_JobStarted")]
internal static class Harmony_PawnFlight
{
    static bool Prefix(Pawn_FlightTracker __instance)
    {
        Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
        if(pawn != null && pawn.HasComp<Comp_PawnFlight>())
        {
            return false; 
        }

        return true;
    }

}
