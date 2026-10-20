using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace FixedPawnGenerate;

public class CommsGachaConfigWorker_All : CommsGachaConfigWorker
{
    protected override void SuccessAction(Pawn caller, FixedPawnDef pawnDef)
    {
        Map map = caller.Map;

        List<Pawn> allPawns = new List<Pawn>();

        foreach (var fixedPawnDef in RemainPawns)
        {
            if (def.setPawnToPlayerFaction)
            {
                fixedPawnDef.faction = Find.FactionManager.OfPlayer.def;
            }

            Pawn pawn = FixedPawnUtility.GenerateFixedPawnWithDef(fixedPawnDef);
            if (pawn.Spawned)
                continue;

            if (def.setPawnToPlayerFaction && pawn.Faction != Find.FactionManager.OfPlayer)
            {
                pawn.SetFaction(Find.FactionManager.OfPlayer);
            }

            allPawns.Add(pawn);
        }

        //空降
        PawnsArrivalModeDefOf.CenterDrop.Worker.Arrive(allPawns, new IncidentParms
        {
            target = map,
            spawnCenter = DropCellFinder.TradeDropSpot(map)
        });

        //通知
        Messages.Message(def.successMessage, new LookTargets(allPawns), MessageTypeDefOf.PositiveEvent);

        ChoiceLetter let = LetterMaker.MakeLetter(def.successLetterLabel, def.successLetterText, LetterDefOf.PositiveEvent, allPawns, null, null, null);
        Find.LetterStack.ReceiveLetter(let, null, 0, true);
    }
    protected override void SuccessMessage(Pawn pawn, Map map)
    {
        return;
    }

}
