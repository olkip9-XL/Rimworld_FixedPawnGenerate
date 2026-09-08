using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate;

internal class Comp_OverrideShader : ThingComp, IThingHolder
{
    private class CachedLabelMaterial
    {
        public Rot4 rotation;

        public Material material;
        public ShaderTypeDef shaderType;
    }

    private struct ActiveSubRef
    {
        public SubTransformDef subTransform;
        public int subIndex;
    }

    public CompProperties_OverrideShader Props => (CompProperties_OverrideShader)this.props;

    private readonly object stateLock = new object();
    private readonly Dictionary<string, MaterialRequest> pendingLabelRequests = new Dictionary<string, MaterialRequest>();
    private readonly Dictionary<string, CachedLabelMaterial> labelMaterialCache = new Dictionary<string, CachedLabelMaterial>();
    private readonly HashSet<int> startedSubIndices = new HashSet<int>();

    private bool debugActive = false;
    private TransformDef activeTransform;
    private int transformStartTick = -1;

    private ThingOwner<ThingWithComps> thingOwner = new ThingOwner<ThingWithComps>();

    public void StartTransform(TransformDef transformDef)
    {
        lock (stateLock)
        {
            ClearRuntimeState_NoLock();

            if (transformDef == null)
            {
                debugActive = false;
                return;
            }

            activeTransform = transformDef;
            transformStartTick = GenTicks.TicksGame;
            debugActive = true;
        }

        if (parent is Pawn pawn)
        {
            PortraitsCache.SetDirty(pawn);
            GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
        }

        MarkPawnGraphicsDirty();
    }

    public Material EditMaterial(string nodeLabel, ref Material material)
    {
        if (material == null || nodeLabel == null || nodeLabel.NullOrEmpty())
        {
            return material;
        }

        lock (stateLock)
        {
            if (!debugActive || activeTransform == null)
            {
                return material;
            }

            if (!TryGetCurrentSubForLabel_NoLock(nodeLabel, out _))
            {
                return material;
            }

            if (!pendingLabelRequests.ContainsKey(nodeLabel))
            {
                MaterialRequest request;
                if (MaterialPool.TryGetRequestForMat(material, out request))
                {
                    pendingLabelRequests[nodeLabel] = request;
                }
            }

            if (labelMaterialCache.TryGetValue(nodeLabel, out CachedLabelMaterial cache) && cache.material != null)
            {
                return cache.material;
            }
        }

        return material;
    }

    public override void CompTick()
    {
        bool shouldStop = false;
        bool updated = false;

        lock (stateLock)
        {
            if (!debugActive || activeTransform == null)
            {
                return;
            }

            int elapsed = GenTicks.TicksGame - transformStartTick;
            if (elapsed < 0 || elapsed >= activeTransform.durationTicks)
            {
                shouldStop = true;
            }
            else
            {
                updated = ApplySubPawnChangesByTick_NoLock(elapsed);
                updated = UpdateTransformMaterials_NoLock(elapsed) || updated;
            }
        }

        if (shouldStop)
        {
            StopTransform();
            return;
        }

        if (updated)
        {
            MarkPawnGraphicsDirty();
        }
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (Gizmo gizmo in base.CompGetGizmosExtra())
        {
            yield return gizmo;
        }

        //yield return new Command_Toggle
        //{
        //    defaultLabel = "Toggle Shader Override",
        //    defaultDesc = "Toggle the shader override on or off.",
        //    isActive = () => debugActive,
        //    toggleAction = () =>
        //    {
        //        if (debugActive)
        //        {
        //            StopTransform();
        //        }
        //    }
        //};
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        base.PostDestroy(mode, previousMap);
        StopTransform();
    }

    private void StopTransform()
    {
        lock (stateLock)
        {
            ClearRuntimeState_NoLock();
            debugActive = false;
        }

        MarkPawnGraphicsDirty();
    }

    private bool UpdateTransformMaterials_NoLock(int elapsed)
    {
        // 构建已观察到的实际节点 label 集合（来自 pending request 与已缓存的材质）
        HashSet<string> observedLabels = new HashSet<string>(pendingLabelRequests.Keys);
        foreach (var k in labelMaterialCache.Keys)
        {
            observedLabels.Add(k);
        }

        if (observedLabels.Count == 0)
        {
            return false;
        }

        bool changed = false;
        foreach (string label in observedLabels)
        {
            if (!TryGetCurrentSubForLabel_NoLock(label, out ActiveSubRef active))
            {
                continue;
            }

            SubTransformDef sub = active.subTransform;
            int subIndex = active.subIndex;

            Material mat = EnsureLabelMaterial_NoLock(label, sub);
            if (mat == null)
            {
                continue;
            }

            //ApplySubToMaterial_NoLock(mat, label, sub, subIndex, elapsed);
            sub.Worker?.UpdateMaterial(ref mat, elapsed);
            changed = true;
        }

        return changed;
    }

    private bool ApplySubPawnChangesByTick_NoLock(int elapsed)
    {
        if (activeTransform?.subTransforms.NullOrEmpty() != false)
        {
            return false;
        }

        bool changed = false;
        for (int i = 0; i < activeTransform.subTransforms.Count; i++)
        {
            if (startedSubIndices.Contains(i))
            {
                continue;
            }

            SubTransformDef sub = activeTransform.subTransforms[i];
            if (!IsSubActive(sub, elapsed))
            {
                continue;
            }

            TransformWorker worker = sub.Worker;
            if (worker == null)
            {
                continue;
            }

            startedSubIndices.Add(i);
            changed = worker.Update(elapsed, parent as Pawn) || changed;
        }

        return changed;
    }

    private bool TryGetCurrentSubForLabel_NoLock(string label, out ActiveSubRef activeSub)
    {
        activeSub = default;
        if (activeTransform == null)
        {
            return false;
        }

        int elapsed = GenTicks.TicksGame - transformStartTick;
        if (elapsed < 0 || elapsed >= activeTransform.durationTicks)
        {
            return false;
        }

        Pawn pawn = parent as Pawn;

        // 遍历所有激活的 sub，按声明顺序覆盖，后声明覆盖前声明
        ActiveSubRef found = default;
        bool any = false;
        for (int i = 0; i < activeTransform.subTransforms.Count; i++)
        {
            SubTransformDef sub = activeTransform.subTransforms[i];
            if (!IsSubActive(sub, elapsed))
                continue;

            TransformWorker worker = sub.Worker;
            if (worker == null)
            {
                continue;
            }

            if (worker.Match(label, pawn))
            {
                found.subTransform = sub;
                found.subIndex = i;
                any = true;
            }
        }

        if (!any)
            return false;

        activeSub = found;
        return true;
    }

    private static bool IsSubActive(SubTransformDef sub, int elapsed)
    {
        //if (sub == null || sub.durationTicks <= 0 || sub.debugLabels.NullOrEmpty())
        if (sub == null || sub.durationTicks <= 0)
        {
            return false;
        }

        if (elapsed < sub.startTick)
        {
            return false;
        }

        return elapsed < sub.startTick + sub.durationTicks;
    }

    private Material EnsureLabelMaterial_NoLock(string label, SubTransformDef sub)
    {
        labelMaterialCache.TryGetValue(label, out CachedLabelMaterial cache);

        Rot4 pawnRotation = this.parent is Pawn pawn ? pawn.Rotation : Rot4.South;

        bool shouldRebuild = cache == null || cache.material == null || cache.shaderType != sub.shaderType || cache.rotation != pawnRotation;
        if (!shouldRebuild)
        {
            return cache.material;
        }

        if (!pendingLabelRequests.TryGetValue(label, out MaterialRequest request))
        {
            return cache?.material;
        }

        if (sub.shaderType?.Shader != null)
        {
            request.shader = sub.shaderType.Shader;
        }

        Material baseMaterial = MaterialPool.MatFrom(request);
        Material instance = new Material(baseMaterial);

        if (cache?.material != null)
        {
            UnityEngine.Object.Destroy(cache.material);
        }

        labelMaterialCache[label] = new CachedLabelMaterial
        {
            rotation = pawnRotation,
            material = instance,
            shaderType = sub.shaderType
        };

        pendingLabelRequests.Remove(label);
        return instance;
    }

    private void ClearRuntimeState_NoLock()
    {
        foreach (CachedLabelMaterial cache in labelMaterialCache.Values)
        {
            if (cache?.material != null)
            {
                UnityEngine.Object.Destroy(cache.material);
            }
        }

        labelMaterialCache.Clear();
        pendingLabelRequests.Clear();
        startedSubIndices.Clear();

        activeTransform = null;
        transformStartTick = -1;
    }

    private void MarkPawnGraphicsDirty()
    {
        if (parent is Pawn pawn)
        {
            pawn.Drawer?.renderer?.SetAllGraphicsDirty();
        }
    }

    public void GetChildHolders(List<IThingHolder> outChildren)
    {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }

    public ThingOwner GetDirectlyHeldThings()
    {
        return thingOwner;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_Deep.Look(ref thingOwner, "thingOwner", this);
    }
}

