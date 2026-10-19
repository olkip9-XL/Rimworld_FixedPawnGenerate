using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate;

public class CompProperties_ThingTrail : CompProperties
{
    public CompProperties_ThingTrail()
    {
        this.compClass = typeof(Comp_ThingTrail);
    }

    public CompProperties_ThingTrail(ModExtension_ThingTrail modExtension)
    {
        this.compClass = typeof(Comp_ThingTrail);

        //分配ModExtension_ThingTrail中的属性值到CompProperties_ThingTrail中
        this.trailTexturePath = modExtension.trailTexturePath;
        this.shaderType = modExtension.shaderType;
        this.shaderParameters = modExtension.shaderParameters;
        this.widthCurve = modExtension.widthCurve;
        this.width = modExtension.width;
        this.taperByLength = modExtension.taperByLength;
        this.transparencyCurve = modExtension.transparencyCurve;
        this.color = modExtension.color;
        this.altitudeLayer = modExtension.altitudeLayer;
        this.altitudeOffset = modExtension.altitudeOffset;
        this.lifeTimeTicks = modExtension.lifeTimeTicks;
        this.defaultActive = modExtension.defaultActive;
        this.samplesPerTick = modExtension.samplesPerTick;
        this.trailFleck = modExtension.trailFleck;
        this.trailFleckChanceCurve = modExtension.trailFleckChanceCurve;
    }

    [NoTranslate]
    private string trailTexturePath;// 材质路径

    public Texture2D trailTexture;
    public ShaderTypeDef shaderType;

    public List<ShaderParameter> shaderParameters;

    public SimpleCurve widthCurve;

    public float width = 0.6f; // Full width in cells; preserves existing definitions.

    public bool taperByLength = false; // Curve 0=head, 1=tail, even for short trails.

    public SimpleCurve transparencyCurve;

    public Color color = Color.white;

    public AltitudeLayer altitudeLayer = AltitudeLayer.Projectile;

    public float altitudeOffset = 0f;

    public int lifeTimeTicks = 72; //1.2s

    public bool defaultActive = true;

    public int samplesPerTick = 1;

    public FleckDef trailFleck;

    public SimpleCurve trailFleckChanceCurve;

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

        if (lifeTimeTicks <= 0)
        {
            yield return "lifeTimeTicks must be greater than 0.";
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

