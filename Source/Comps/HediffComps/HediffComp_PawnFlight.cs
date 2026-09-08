using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace FixedPawnGenerate;

public class HediffComp_PawnFlight : HediffComp
{
    public bool IsFlying => Pawn.flight?.Flying == true;

    HediffCompProperties_PawnFlight Props => (HediffCompProperties_PawnFlight)this.props;

    Comp_PawnFlight PawnFlight => Pawn.GetComp<Comp_PawnFlight>();

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
    }

    public override IEnumerable<Gizmo> CompGetGizmos()
    {
        if (!Props.showGizmo)
        {
            yield break;
        }

        yield return new Command_Toggle
        {
            defaultLabel = IsFlying ? Props.landLabel : Props.takeOffLabel,
            defaultDesc = IsFlying ? Props.landDesc : Props.takeOffDesc,
            icon = IsFlying ? Props.landIcon : Props.takeOffIcon,
            isActive = () => Pawn.flight.Flying,
            toggleAction = delegate
            {
                if (IsFlying)
                {
                    PawnFlight?.StopFlying();
                }
                else
                {
                    PawnFlight?.StartFlying();
                }
            }
        };

        yield break;
    }

    public override void CompPostPostRemoved()
    {
        base.CompPostPostRemoved();

        if (IsFlying)
        {
            PawnFlight?.StopFlying();
        }
    }

}
