using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate;

public class CompProperties_PawnFlight : CompProperties
{
    public CompProperties_PawnFlight()
    {
        this.compClass = typeof(Comp_PawnFlight);
    }

    //残影
    public int blurCount = 3;

    public int blurInterval = 10;

    public bool defaultFlying = false;

    public FloatRange blurAlpha = new FloatRange(0.5f, 1f);

    [NoTranslate]
    public string takeOffIconPath = "UI/Icon/TakeOff";

    [NoTranslate]
    public string landIconPath = "UI/Icon/Land";

    public Texture2D takeOffIcon;

    public Texture2D landIcon;

    public string takeOffLabel = "TakeOff";

    public string landLabel = "Land";

    public string takeOffDesc = "TakeOffDesc";

    public string landDesc = "LandDesc";

    public bool showButton = true;

    public HediffDef flyingHediff;

    public override void PostLoadSpecial(ThingDef parent)
    {
        base.PostLoadSpecial(parent);

        if (takeOffIcon == null && !takeOffIconPath.NullOrEmpty())
        {
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                takeOffIcon = ContentFinder<Texture2D>.Get(takeOffIconPath, true);
            });
        }

        if (landIcon == null && !landIconPath.NullOrEmpty())
        {
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                landIcon = ContentFinder<Texture2D>.Get(landIconPath, true);
            });
        }
    }
}
