using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using Verse.AI;

namespace FixedPawnGenerate;

[StaticConstructorOnStartup]
internal static class ThingTrailMaker
{
    static ThingTrailMaker()
    {
        GenerateTrailDefsForThingDef();
    }
    private static void GenerateTrailDefsForThingDef()
    {
        IEnumerable<ThingDef> allThingDefs = DefDatabase<ThingDef>.AllDefsListForReading.Where(def => def.HasModExtension<ModExtension_ThingTrail>());

        List<ThingDef> trailDefsReadyToAdd = new List<ThingDef>();

        foreach (var thingDef in allThingDefs)
        {
            if (typeof(ThingWithComps).IsAssignableFrom(thingDef.thingClass))
            {
                ModExtension_ThingTrail modExtension_ThingTrail = thingDef.GetModExtension<ModExtension_ThingTrail>();
                if (modExtension_ThingTrail == null)
                {
                    Log.Error($"ThingDef {thingDef.defName} has no ModExtension_ThingTrail, skipping trail generation.");
                    continue;
                }

                try
                {
                    //Create一个新的ThingDef，defName为原ThingDef的defName + "_Trail"
                    ThingDef trailDef = new ThingDef
                    {
                        category = ThingCategory.Ethereal,
                        useHitPoints = false,
                        defName = thingDef.defName + "_Trail",
                        selectable = false,
                        tickerType = TickerType.Normal,
                        thingClass = typeof(ProjectileTracker),
                        drawOffscreen = true,
                        drawerType = DrawerType.RealtimeOnly,
                        graphicData = new GraphicData
                        {
                            texPath = "Empty",
                            graphicClass = typeof(Graphic_Single)
                        }
                    };

                    //为TrailDef添加CompProperties_ThingTrail组件
                    CompProperties_ThingTrail compProperties_ThingTrail = new CompProperties_ThingTrail(modExtension_ThingTrail);
                    compProperties_ThingTrail.PostLoadSpecial(trailDef);
                    trailDef.comps.Add(compProperties_ThingTrail);

                    //为原始Def添加CompProperties_ProjectileTracker组件
                    CompProperties_ProjectileTracker compProperties_ProjectileTracker = new CompProperties_ProjectileTracker();
                    compProperties_ProjectileTracker.tracker = trailDef;
                    thingDef.comps.Add(compProperties_ProjectileTracker);

                    //将TrailDef添加到DefDatabase中
                    trailDefsReadyToAdd.Add(trailDef);
                }
                catch (Exception e)
                {
                    Log.Error($"Error generating trail for ThingDef {thingDef.defName}: {e.Message}");
                }
            }
        }

        foreach (var trailDef in trailDefsReadyToAdd)
        {
            DefDatabase<ThingDef>.Add(trailDef);
        }
    }

}
