using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;

namespace FixedPawnGenerate;

public enum ItemAddMode
{
    //Make新的，默认普通质量
    MakeNew,

    //忽略参数，从storage中全部拿取并装备
    Storage
}

public enum ItemRemoveMode
{
    //直接Destroy
    Destroy,
    //扔在地上
    Drop,
    //转移至storage
    Storage,
}

public class TransformDef : Def
{
    public int durationTicks = 60;

    public List<SubTransformDef> subTransforms = new List<SubTransformDef>();

    public void Apply(Pawn pawn)
    {
        if (pawn == null)
        {
            return;
        }
        var comp = pawn.TryGetComp<Comp_OverrideShader>();
        if (comp == null)
        {
            Log.Error($"Pawn {pawn} does not have Comp_OverrideShader, cannot apply TransformDef {defName}.");
            return;
        }
        comp.StartTransform(this);
    }

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (string error in base.ConfigErrors())
        {
            yield return error;
        }

        if (durationTicks <= 0)
        {
            yield return "durationTicks must be greater than 0.";
        }

        if (subTransforms.NullOrEmpty())
        {
            yield return "subTransforms is null or empty.";
            yield break;
        }

        for (int i = 0; i < subTransforms.Count; i++)
        {
            SubTransformDef sub = subTransforms[i];
            if (sub == null)
            {
                yield return $"subTransforms[{i}] is null.";
                continue;
            }

            foreach (string error in sub.ConfigErrors(i))
            {
                yield return error;
            }
        }
    }
}

public class SubTransformDef
{
    /*
    特殊Labels说明

    1、 Apparel: 衣物节点。
    2、 All: 所有节点。
     */

    public List<string> debugLabels = new List<string>();

    public int startTick = 0;

    public int durationTicks = 60;

    public ShaderTypeDef shaderType;

    public List<ShaderParameter> shaderParameters = new List<ShaderParameter>();

    public List<FloatShaderParamTransform> floatParamTransforms = new List<FloatShaderParamTransform>();

    public Color? leftEyeColor;

    public Color? rightEyeColor;

    public List<ThingDef> apparels = new List<ThingDef>();

    public List<ThingDef> weapons = new List<ThingDef>();

    public HairDef hair;

    public TransformWorker workerInt;
    public TransformWorker Worker {
        get
        {
            if (transformWorker == null)
            {
                return null;
            }

            if (workerInt == null)
            {
                workerInt = Activator.CreateInstance(transformWorker) as TransformWorker;
                if (workerInt != null)
                {
                    workerInt.subTransformDef = this;
                }
            }

            return workerInt;
        }
    }

    public Type transformWorker;

    public ItemAddMode itemAddMode = ItemAddMode.MakeNew;

    public ItemRemoveMode itemRemoveMode = ItemRemoveMode.Drop;

    public bool lockApparel = false;


    //为其他mod提供扩展点
    public List<DefModExtension> modExtensions;


    public IEnumerable<string> ConfigErrors(int index)
    {
        if (transformWorker != null && !typeof(TransformWorker).IsAssignableFrom(transformWorker))
        {
            yield return $"subTransforms[{index}].transformWorker must inherit TransformWorker.";
        }

        if (startTick < 0)
        {
            yield return $"subTransforms[{index}].startTick must be >= 0.";
        }

        if (durationTicks <= 0)
        {
            yield return $"subTransforms[{index}].durationTicks must be > 0.";
        }

        if (!floatParamTransforms.NullOrEmpty())
        {
            for (int i = 0; i < floatParamTransforms.Count; i++)
            {
                FloatShaderParamTransform floatTransform = floatParamTransforms[i];
                if (floatTransform == null)
                {
                    yield return $"subTransforms[{index}].floatParamTransforms[{i}] is null.";
                    continue;
                }

                foreach (string error in floatTransform.ConfigErrors(index, i))
                {
                    yield return error;
                }
            }
        }

        if (!apparels.NullOrEmpty())
        {
            for (int i = 0; i < apparels.Count; i++)
            {
                ThingDef apparelDef = apparels[i];
                if (apparelDef == null)
                {
                    yield return $"subTransforms[{index}].apparels[{i}] is null.";
                    continue;
                }

                if (!apparelDef.IsApparel)
                {
                    yield return $"subTransforms[{index}].apparels[{i}] must be apparel ThingDef.";
                }
            }
        }

        if (!weapons.NullOrEmpty())
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                ThingDef weaponDef = weapons[i];
                if (weaponDef == null)
                {
                    yield return $"subTransforms[{index}].weapons[{i}] is null.";
                    continue;
                }

                if (!weaponDef.IsWeapon)
                {
                    yield return $"subTransforms[{index}].weapons[{i}] must be weapon ThingDef.";
                }
            }
        }
    }
}

public class FloatShaderParamTransform
{
    public string paramName;

    public float targetValue;

    public SimpleCurve curve;

    public IEnumerable<string> ConfigErrors(int subIndex, int index)
    {
        if (paramName.NullOrEmpty())
        {
            yield return $"subTransforms[{subIndex}].floatParamTransforms[{index}].paramName is null or empty.";
        }
    }
}
