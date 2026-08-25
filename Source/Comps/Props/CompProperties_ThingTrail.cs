using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate;

internal class CompProperties_ThingTrail : CompProperties
{
    public CompProperties_ThingTrail()
    {
        this.compClass = typeof(Comp_ThingTrail);
    }

    public string trailTexturePath;// 材质路径

    public ShaderTypeDef shaderType;

    public List<ShaderParameter> shaderParameters;

    public SimpleCurve widthCurve;

    public SimpleCurve transparencyCurve;

    public Color color = Color.white;

    public AltitudeLayer altitudeLayer = AltitudeLayer.Projectile;

    public float altitudeOffset = 0f;

    public int lifeTimeTicks = 72; //1.2s

    public bool defaultActive = true;

    public int samplesPerTick = 1;
}
