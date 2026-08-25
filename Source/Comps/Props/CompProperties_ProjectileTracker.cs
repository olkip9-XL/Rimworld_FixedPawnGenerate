using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate;

internal class CompProperties_ProjectileTracker : CompProperties
{
    public CompProperties_ProjectileTracker()
    {
        this.compClass = typeof(Comp_ProjectileTracker);
    }

    public ThingDef tracker;

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        foreach (var error in base.ConfigErrors(parentDef))
        {
            yield return error;
        }
        if (tracker == null || tracker.thingClass != typeof(ProjectileTracker))
        {
            yield return "tracker is null or not a ProjectileTracker";
        }
    }
}
