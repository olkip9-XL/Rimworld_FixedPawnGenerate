using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate;

public class CompProperties_OverrideBodyGraphic : CompProperties
{
    public CompProperties_OverrideBodyGraphic()
    {
        this.compClass = typeof(Comp_OverrideBodyGraphic);
    }

    public string graphicPath;

    public Vector2 scale = Vector2.one;

}
