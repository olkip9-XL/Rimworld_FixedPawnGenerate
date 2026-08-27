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

    private string trailTexturePath;// 材质路径

    public Texture2D trailTexture;
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

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        foreach (var error in base.ConfigErrors(parentDef))
        {
            yield return error;
        }

        if (samplesPerTick <= 0)
        {
            samplesPerTick = 1;
            yield return "samplesPerTick must be greater than 0.";
        }

        if (trailTexturePath.NullOrEmpty())
        {
            yield return "trailTexturePath must be set.";
        }
    }

    public override void PostLoadSpecial(ThingDef parent)
    {
        base.PostLoadSpecial(parent);

        LongEventHandler.ExecuteWhenFinished(() =>
        {
            if (!trailTexturePath.NullOrEmpty())
            {
                trailTexture = ContentFinder<Texture2D>.Get(trailTexturePath, true);
                if (trailTexture == null)
                {
                    trailTexture = BaseContent.BadTex;
                }
            }
        });
    }
}
