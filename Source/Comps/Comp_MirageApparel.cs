using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate;

public class Comp_MirageApparel : ThingComp
{
    private static List<Apparel> cachedApparel = null;

    public CompProperties_MirageApparel Props => (CompProperties_MirageApparel)this.props;

    Pawn pawn => this.parent as Pawn;

    private bool active = false;

    public bool Active => active;

    public List<Apparel> GetApparel()
    {
        if (cachedApparel == null)
        {
            InitializeApparel();
        }

        return active ? cachedApparel : null;
    }

    public void ToggleActive()
    {
        active = !active;

        pawn.Drawer.renderer.SetAllGraphicsDirty();
    }

    private void InitializeApparel()
    {
        if (pawn.apparel == null)
        {
            Log.Error("Pawn " + pawn.Name + " has null apparel.");
            return;
        }

        cachedApparel = new List<Apparel>();

        if (!Props.overrideApparel.NullOrEmpty())
        {
            foreach (var apparelData in Props.overrideApparel)
            {
                cachedApparel.Add((Apparel)ThingMaker.MakeThing(apparelData.thingDef, apparelData.stuff));
            }
        }
        else
        {
            FixedPawnDef fixedPawnDef = pawn.GetFixedPawnDef();

            if (fixedPawnDef != null)
            {
                foreach (var apparelData in fixedPawnDef.apparel)
                {
                    cachedApparel.Add((Apparel)ThingMaker.MakeThing(apparelData.thing, apparelData.stuff));
                }
            }
        }
    }


    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_Values.Look(ref active, "active", false);
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (DebugSettings.ShowDevGizmos)
        {
            Command_Toggle toggle = new Command_Toggle
            {
                defaultLabel = "Dev: Toggle mirage apparel",
                isActive = () => active,
                toggleAction = () => ToggleActive()
            };
            yield return toggle;
        }
        yield break;
    }
}
