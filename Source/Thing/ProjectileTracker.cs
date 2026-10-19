using UnityEngine;
using Verse;

namespace FixedPawnGenerate;

internal class ProjectileTracker : ThingWithComps
{
    private Thing projectile;
    private int lifetimeTicksLeft;
    private Vector3 lastDrawPos;
    private bool hasLastDrawPos;
    public bool IsTracking => projectile != null && !projectile.Destroyed && projectile.Spawned;
    public override Vector3 DrawPos => IsTracking ? projectile.DrawPos :
        (hasLastDrawPos ? lastDrawPos : base.DrawPos);

    public void ResetTrail(Vector3 position)
    {
        lastDrawPos = position;
        hasLastDrawPos = true;
        this.TryGetComp<Comp_ThingTrail>()?.ResetTrail(position);
    }

    public void FinishTrail(Vector3 position)
    {
        lastDrawPos = position;
        hasLastDrawPos = true;
        this.TryGetComp<Comp_ThingTrail>()?.RecordPosition(position);
        projectile = null;
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (!respawningAfterLoad)
            lifetimeTicksLeft = Mathf.Max(1, this.TryGetComp<Comp_ThingTrail>()?.Props.lifeTimeTicks ?? 1);
    }

    protected override void Tick()
    {
        if (IsTracking)
        {
            lastDrawPos = projectile.DrawPos;
            hasLastDrawPos = true;
            Position = projectile.Position;
        }
        base.Tick();
        if (IsTracking) return;
        if (--lifetimeTicksLeft <= 0) Destroy();
    }

    public void SetProjectile(Thing projectile)
    {
        this.projectile = projectile;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref projectile, "projectile");
        Scribe_Values.Look(ref lifetimeTicksLeft, "lifetimeTicksLeft", 0);
        Scribe_Values.Look(ref lastDrawPos, "trailLastDrawPos");
        Scribe_Values.Look(ref hasLastDrawPos, "trailHasLastDrawPos", false);
    }
}

