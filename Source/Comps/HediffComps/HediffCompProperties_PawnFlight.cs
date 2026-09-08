using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate;

public class HediffCompProperties_PawnFlight : HediffCompProperties
{
    public HediffCompProperties_PawnFlight()
    {
        this.compClass = typeof(HediffComp_PawnFlight);
    }

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

    public bool showGizmo = true;

    public override void PostLoad()
    {
        base.PostLoad();

        //加载按钮贴图
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
