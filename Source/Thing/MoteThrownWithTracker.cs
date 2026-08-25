using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate;

internal class MoteThrownWithTracker : MoteThrown
{
    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);

        if (!respawningAfterLoad && this.def.HasModExtension<ModExtension_ThingTracker>())
        {
            ThingDef trackerDef = this.def.GetModExtension<ModExtension_ThingTracker>().tracker;
            if (trackerDef != null)
            {
                Thing tracker = ThingMaker.MakeThing(trackerDef);
                if (tracker is ProjectileTracker projectileTracker)
                {
                    projectileTracker.SetProjectile(this);
                    GenSpawn.Spawn(projectileTracker, this.Position, this.Map);
                }
            }
            else
            {
                Log.Error($"MoteThrownWithTracker on {this} has no valid tracker set.");
                return;
            }
        }
    }

}
