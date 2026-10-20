using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;



namespace FixedPawnGenerate;

public class Comp_OverrideBodyGraphic : ThingComp
{
    public CompProperties_OverrideBodyGraphic Props => (CompProperties_OverrideBodyGraphic)this.props;

    public Graphic GetGraphic(Pawn pawn, Shader shader, Color color)
    {
        return GraphicDatabase.Get<Graphic_Multi>(Props.graphicPath, shader, Props.scale, color);
    }

}
