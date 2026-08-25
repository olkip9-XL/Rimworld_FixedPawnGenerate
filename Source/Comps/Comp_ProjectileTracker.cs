using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate;

internal class Comp_ProjectileTracker : ThingComp
{
    public CompProperties_ProjectileTracker Props => (CompProperties_ProjectileTracker)props;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);

        if (!respawningAfterLoad)
        {
            if (Props.tracker != null)
            {
                ProjectileTracker tracker = (ProjectileTracker)ThingMaker.MakeThing(Props.tracker);
                if (tracker != null)
                {
                    tracker.SetProjectile(parent);
                    GenSpawn.Spawn(tracker, parent.Position, parent.Map);
                }
            }
            else
            {
                Log.Error($"Comp_ProjectileTracker on {parent} has no valid tracker set.");
                return;
            }
          
        }
    }

}
