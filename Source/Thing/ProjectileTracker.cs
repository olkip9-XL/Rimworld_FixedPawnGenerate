using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate;

internal class ProjectileTracker : ThingWithComps
{

    private bool ProjectileDestroyed
    {
        get
        {
            return projectile == null || projectile.Destroyed || !projectile.Spawned;
        }

    }
    public override Vector3 DrawPos => projectile?.DrawPos ?? base.DrawPos;


    private Thing projectile;

    private int lifetimeTicksLeft;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);

        if (!respawningAfterLoad)
        {
            Comp_ThingTrail comp = this.TryGetComp<Comp_ThingTrail>();
            if (comp != null)
            {
                this.lifetimeTicksLeft = comp.Props.lifeTimeTicks;
            }
        }
    }

    protected override void Tick()
    {
        base.Tick();

        if (lifetimeTicksLeft <= 0)
        {
            this.Destroy();
            return;
        }

        if (ProjectileDestroyed)
        {
            lifetimeTicksLeft--;
        }
        //track the projectile
        this.Position = projectile.Position;
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
    }
}
