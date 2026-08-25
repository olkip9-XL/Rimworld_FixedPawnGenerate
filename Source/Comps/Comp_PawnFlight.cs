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

    private Vector3 lastSpawnPos;

    private int ticksSinceLastSample;

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

        if ((curPos - lastSpawnPos).MagnitudeHorizontal() >= 0.03f)
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

        int i = 1;
        foreach (var sample in samples)
        {
            if (GlobalTextureAtlasManager.TryGetPawnFrameSet(Pawn, out var frameSet, out var _))
            {
                using (new ProfilerBlock("Draw flight blur"))
                {
                    float alpha = Mathf.Lerp(Props.blurAlpha.min, Props.blurAlpha.max, (float)i / (samples.Count + 1)); // Adjust the alpha based on the sample index

                    Rot4 facing = sample.rot;
                    Vector3 bodyPos = sample.pos;
                    float bodyAngle = 0f;
                    PawnDrawMode drawMode = PawnDrawMode.BodyAndHead;
                    Material mat = MaterialPool.MatFrom(new MaterialRequest(frameSet.atlas, ShaderDatabase.Cutout));
                    mat = FadedMaterialPool.FadedVersionOf(mat, alpha); // Set the transparency of the residual image
                    GenDraw.DrawMeshNowOrLater(GetBlitMeshUpdatedFrame(frameSet, facing, drawMode), bodyPos, Quaternion.AngleAxis(bodyAngle, Vector3.up), mat, drawNow: false);

                    //test
                    //Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(sample.pos, Quaternion.identity, Vector3.one), Pawn.Drawer.renderer.SilhouetteGraphic.MatSingle, 0);
                }
            }

            i++;
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
        if (!Props.showButton)
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
            return 9999f; // Set a very high value for max flight time
        }
        return base.GetStatOffset(stat);
    }



}

