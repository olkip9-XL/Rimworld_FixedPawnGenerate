using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace FixedPawnGenerate
{
    internal class JobDriver_SwitchStyle : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }


        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (ModLister.CheckIdeology("Styling station"))
            {
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedOrNull(TargetIndex.A);

                Toil warmup = Toils_General.Wait(300); //5s
                warmup.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
                yield return warmup;

                yield return Toils_General.Do(delegate
                {
                    pawn.GetComp<Comp_SwitchStyle>()?.SetStyle(job.count);
                });
            }

            yield break;
        }
    }
}
