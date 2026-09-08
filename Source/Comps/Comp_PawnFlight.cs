using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using Verse;
using Verse.Sound;

namespace FixedPawnGenerate;

public class Comp_PawnFlight : ThingComp
{
    private struct Sample
    {
        public Vector3 pos;
        public Rot4 rot;

        public Material mat;
    }
    public CompProperties_PawnFlight Props => (CompProperties_PawnFlight)this.props;

    Pawn Pawn => this.parent as Pawn;

    public bool IsFlying => Pawn.flight?.Flying == true;

    //public bool IsMoving => (Pawn.DrawPos - lastSpawnPos).MagnitudeHorizontal() >= 0.03f;
    public bool IsMoving => Pawn.pather.MovingNow;

    private Vector3 lastSpawnPos;

    private int ticksSinceLastSample;

    private int ticksSinceLastFlyingEffecter;

    private Queue<Sample> samples = new Queue<Sample>();

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (!respawningAfterLoad && Props.defaultFlying)
        {
            StartFlying();
        }
    }

    public override void CompTick()
    {
        SampleTick();

        EffecterTick();
    }

    private void EffecterTick()
    {
        if (!IsFlying || !IsMoving || Props.flyingEffecter == null)
        {
            return;
        }

        if (ticksSinceLastFlyingEffecter < Props.flyingEffecterSpawnInterval)
        {
            ticksSinceLastFlyingEffecter++;
            return;
        }
        else
        {
            ticksSinceLastFlyingEffecter = 0;
            TriggerFlyingEffecter();
        }
    }

    protected virtual void TriggerFlyingEffecter()
    {
        if (Props.flyingEffecter == null)
            return;

        Vector3 offset = new Vector3(Rand.Range(-Props.flyingEffecterSpawnRadius, Props.flyingEffecterSpawnRadius), 0f, Rand.Range(-Props.flyingEffecterSpawnRadius, Props.flyingEffecterSpawnRadius));

        Effecter effecter = Props.flyingEffecter.Spawn();
        effecter.offset = offset;
        effecter.Trigger(Pawn, Pawn);
        effecter.Cleanup();
    }

    private void SampleTick()
    {
        if (!IsFlying)
        {
            if (samples.Count > 0)
                samples.Clear();
            ticksSinceLastSample = 0;
            return;
        }

        ticksSinceLastSample++;
        if (ticksSinceLastSample < Props.blurInterval)
            return;

        Vector3 curPos = Pawn.DrawPos;
        if (lastSpawnPos == Vector3.zero) lastSpawnPos = curPos;

        if (IsMoving)
        {
            if (GlobalTextureAtlasManager.TryGetPawnFrameSet(Pawn, out var frameSet, out var _))
            {
                Material mat = MaterialPool.MatFrom(new MaterialRequest(frameSet.atlas, ShaderDatabase.Cutout));

                samples.Enqueue(new Sample
                {
                    pos = curPos,
                    rot = Pawn.Rotation,
                    mat = mat
                });
            }

            lastSpawnPos = curPos;
            ticksSinceLastSample = 0;

            while (samples.Count > Props.blurCount)
            {
                samples.Dequeue();
            }
        }
        else
        {
            samples.TryDequeue(out _);
        }
    }


    public override void PostDraw()
    {
        if (!IsFlying)
            return;

        if (samples.Count == 0)
            return;

        foreach (var i in Enumerable.Range(0, samples.Count))
        {
            Sample sample = samples.ElementAt(i);

            if (GlobalTextureAtlasManager.TryGetPawnFrameSet(Pawn, out var frameSet, out var _))
            {
                using (new ProfilerBlock("Draw flight blur"))
                {
                    float alpha = Mathf.Lerp(Props.blurAlpha.min, Props.blurAlpha.max, (float)i / (samples.Count + 1)); // Adjust the alpha based on the sample index
                    Material mat = FadedMaterialPool.FadedVersionOf(sample.mat, alpha);
                    GenDraw.DrawMeshNowOrLater(GetBlitMeshUpdatedFrame(frameSet, sample.rot, PawnDrawMode.BodyAndHead), sample.pos, Quaternion.AngleAxis(0f, Vector3.up), mat, drawNow: false);
                }
            }
        }

        Mesh GetBlitMeshUpdatedFrame(PawnTextureAtlasFrameSet frameSet, Rot4 rotation, PawnDrawMode drawMode)
        {
            int index = frameSet.GetIndex(rotation, drawMode);
            if (frameSet.isDirty[index])
            {
                Find.PawnCacheCamera.rect = frameSet.uvRects[index];
                Find.PawnCacheRenderer.RenderPawn(Pawn, frameSet.atlas, Vector3.zero, 1f, 0f, rotation);
                Find.PawnCacheCamera.rect = new Rect(0f, 0f, 1f, 1f);
                frameSet.isDirty[index] = false;
            }

            return frameSet.meshes[index];
        }
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (!Props.showGizmo)
            yield break;

        yield return new Command_Toggle
        {
            defaultLabel = IsFlying ? Props.landLabel : Props.takeOffLabel,
            defaultDesc = IsFlying ? Props.landDesc : Props.takeOffDesc,
            icon = IsFlying ? Props.landIcon : Props.takeOffIcon,
            isActive = () => Pawn.flight.Flying,
            toggleAction = delegate
            {
                if (IsFlying)
                {
                    StopFlying();
                }
                else
                {
                    StartFlying();
                }
            }
        };

        yield break;
    }

    public void StartFlying()
    {
        if (!IsFlying)
        {
            Pawn.flight?.StartFlying();
            Pawn.TryGetComp<Comp_ThingTrail>()?.active = true;

            //hediff
            if (Props.flyingHediff != null && !Pawn.health.hediffSet.HasHediff(Props.flyingHediff))
            {
                Hediff hediff = HediffMaker.MakeHediff(Props.flyingHediff, Pawn);
                hediff.Severity = 1f;
                Pawn.health.AddHediff(hediff);
            }
        }
    }

    public void StopFlying()
    {
        if (IsFlying)
        {
            Pawn.flight?.ForceLand();
            Pawn.TryGetComp<Comp_ThingTrail>()?.active = false;

            //hediff    
            if (Props.flyingHediff != null && Pawn.health.hediffSet.HasHediff(Props.flyingHediff))
            {
                Hediff hediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.flyingHediff);
                if (hediff != null)
                {
                    Pawn.health.RemoveHediff(hediff);
                }
            }
        }
    }

    public override float GetStatOffset(StatDef stat)
    {
        if (stat == StatDefOf.MaxFlightTime)
        {
            return Props.maxFlightTime; // Set a very high value for max flight time
        }
        return base.GetStatOffset(stat);
    }



}

