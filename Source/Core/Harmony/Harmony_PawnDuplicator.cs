using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate;

internal static class Harmony_PawnDuplicator
{
    //Anomaly Duplicate
    [HarmonyPatch(typeof(GameComponent_PawnDuplicator), "Duplicate")]
    public static class PawnDuplicatePatch
    {
        //copy comps
        public static void Prefix(Pawn pawn)
        {
            if (pawn == null)
            {
                return;
            }

            FixedPawnDef def = pawn.GetFixedPawnDef();
            if (def != null)
            {
#if DEBUG
                Log.Warning($"[Debug]Pawn复制：{pawn.Name}");
#endif
                FixedPawnUtility.CurPawnDef = def;
            }
        }

        public static void Postfix(Pawn pawn, Pawn __result)
        {
            if (pawn == null || __result == null)
            {
                return;
            }

            if (FixedPawnUtility.CurPawnDef != null)
            {
                //处理眼睛的复制
                FixedPawnUtility.CurPawnDef.facialAnimationProps?.SetPawn(__result);

                FixedPawnUtility.Manager.AddPawn(__result, FixedPawnUtility.CurPawnDef);
                FixedPawnUtility.CurPawnDef = null;
            }
        }
     
    }
}
