using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate;

internal class Comp_ShootEffect : ThingComp
{
    public CompProperties_ShootEffect Props => (CompProperties_ShootEffect)this.props;

    Pawn Holder
    {
        get
        {
            return (this.parent.ParentHolder as Pawn_EquipmentTracker)?.pawn;
        }
    }

    public override void Notify_UsedWeapon(Pawn pawn)
    {
        base.Notify_UsedWeapon(pawn);

        TriggerEffect();
    }

    private void TriggerEffect()
    {
        if (Holder == null || Props.effecterDef == null)
        {
            return;
        }

        float aimingAngle = 0f;

        //检查Stance
        Stance_Busy stance_Busy = Holder.stances.curStance as Stance_Busy;
        if (stance_Busy != null)
        {
            Vector3 targetPos = stance_Busy.focusTarg.HasThing ? stance_Busy.focusTarg.Thing.DrawPos : stance_Busy.focusTarg.Cell.ToVector3Shifted();
            if ((targetPos - Holder.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
            {
                aimingAngle = (targetPos - Holder.DrawPos).AngleFlat();
            }
        }

        //检查Verb
        Verb curVerb = Holder.CurrentEffectiveVerb;
        if (curVerb != null && curVerb.AimAngleOverride.HasValue)
        {
            aimingAngle = curVerb.AimAngleOverride.Value;
        }

        Vector3 offset = new Vector3(0, 0, 1f).RotatedBy(aimingAngle) * Props.effecterSpawnDistance;

        Vector3 spawnPos = Holder.DrawPos + new Vector3(0, 0, 1f).RotatedBy(aimingAngle) * Props.effecterSpawnDistance;

        Effecter effecter = Props.effecterDef.Spawn();
        effecter.offset = offset;
        effecter.Trigger(Holder, null);
        effecter.Cleanup();
    }

}
