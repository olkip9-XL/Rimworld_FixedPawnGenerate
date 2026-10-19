using UnityEngine;
using Verse;

namespace FixedPawnGenerate;

public class Comp_ProjectileTracker : ThingComp
{
    private CompProperties_ProjectileTracker Props => (CompProperties_ProjectileTracker)props;
    private ProjectileTracker tracker;

    public void ResetTrail(Vector3 position)
    {
        if (tracker != null && !tracker.Destroyed)
            tracker.ResetTrail(position);
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (respawningAfterLoad) return;
        if (Props.tracker == null)
        {
            Log.Error($"Comp_ProjectileTracker on {parent} has no valid tracker set.");
            return;
        }
        tracker = (ProjectileTracker)ThingMaker.MakeThing(Props.tracker);
        tracker.SetProjectile(parent);
        GenSpawn.Spawn(tracker, parent.Position, parent.Map);
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        // Preserve the final segment; the independent tracker continues fading.
        if (tracker != null && !tracker.Destroyed)
            tracker.FinishTrail(parent.DrawPos);
        base.PostDeSpawn(map, mode);
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_References.Look(ref tracker, "trailTracker");
    }
}
