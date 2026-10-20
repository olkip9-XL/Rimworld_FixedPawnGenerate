using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate
{
    internal class Comp_ThingTrail : ThingComp
    {
        private struct Sample { public Vector3 pos; public float time; }
        public CompProperties_ThingTrail Props => (CompProperties_ThingTrail)props;
        public bool active;
        private readonly List<Sample> samples = new List<Sample>();
        private readonly List<Sample> drawingSamples = new List<Sample>();
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<Vector2> uvs = new List<Vector2>();
        private readonly List<Color> colors = new List<Color>();
        private readonly List<int> triangles = new List<int>();
        private readonly List<float> distances = new List<float>();
        private Mesh trailMesh;
        private Material trailMat;
        private const int MaxSamples = 8192;
        private int Lifetime => Mathf.Max(1, Props.lifeTimeTicks);

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad) active = Props.defaultActive;
        }

        public void ResetTrail(Vector3 position)
        {
            samples.Clear();
            RecordPosition(position);
        }

        public void RecordPosition(Vector3 position)
        {
            if (!active) return;
            position.y = Props.altitudeLayer.AltitudeFor() + Props.altitudeOffset;
            float now = GenTicks.TicksGame;
            // Retain one expired point for interpolation at the lifetime boundary.
            while (samples.Count > 1 && samples[1].time <= now - Lifetime)
                samples.RemoveAt(0);
            if (samples.Count == 0 || now - samples[samples.Count - 1].time > Lifetime)
            {
                samples.Clear();
                samples.Add(new Sample { pos = position, time = now });
                return;
            }
            Sample last = samples[samples.Count - 1];
            if ((position - last.pos).sqrMagnitude < 0.000001f) return;
            int count = Mathf.Clamp(Props.samplesPerTick, 1, 128);
            for (int i = 1; i <= count; i++)
            {
                float t = i / (float)count;
                samples.Add(new Sample
                {
                    pos = Vector3.Lerp(last.pos, position, t),
                    time = Mathf.Lerp(last.time, now, t)
                });
            }
            if (samples.Count > MaxSamples) samples.RemoveRange(0, samples.Count - MaxSamples);
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!active) { samples.Clear(); return; }
            // Simulation records the history, including offscreen and accelerated flight.
            if (!(parent is ProjectileTracker tracker) || tracker.IsTracking)
                RecordPosition(parent.DrawPos);
            if (Props.trailFleck == null || Props.trailFleckChanceCurve == null || parent.Map == null)
                return;
            foreach (Sample sample in samples)
            {
                float age = (GenTicks.TicksGame - sample.time) / Lifetime;
                if (age < 0f || age >= 1f) continue;
                if (!Rand.Chance(Props.trailFleckChanceCurve.Evaluate(age))) continue;
                float halfWidth = Mathf.Max(0f, Props.width * (Props.widthCurve?.Evaluate(age) ?? 1f)) * 0.5f;
                Vector3 position = sample.pos + new Vector3(Rand.Range(-halfWidth, halfWidth), 0f,
                    Rand.Range(-halfWidth, halfWidth));
                if (position.ToIntVec3().InBounds(parent.Map))
                    FleckMaker.Static(position, parent.Map, Props.trailFleck, 1f);
            }
        }

        public override void PostDraw()
        {
            if (!active || samples.Count == 0) return;
            drawingSamples.Clear();
            float cutoff = GenTicks.TicksGame - Lifetime;
            for (int i = 0; i < samples.Count; i++)
            {
                Sample sample = samples[i];
                if (sample.time < cutoff)
                {
                    if (i + 1 < samples.Count && samples[i + 1].time > cutoff)
                    {
                        Sample next = samples[i + 1];
                        drawingSamples.Add(new Sample
                        {
                            pos = Vector3.Lerp(sample.pos, next.pos, (cutoff - sample.time) / (next.time - sample.time)),
                            time = cutoff
                        });
                    }
                    continue;
                }
                drawingSamples.Add(sample);
            }
            // Attach to the current head without recording render-dependent history.
            if (!(parent is ProjectileTracker tracker) || tracker.IsTracking)
            {
                Vector3 head = parent.DrawPos;
                head.y = Props.altitudeLayer.AltitudeFor() + Props.altitudeOffset;
                if (drawingSamples.Count > 0 && (head - drawingSamples[drawingSamples.Count - 1].pos).sqrMagnitude > 0.000001f)
                    drawingSamples.Add(new Sample { pos = head, time = GenTicks.TicksGame });
            }
            if (drawingSamples.Count < 2) return;

            if (trailMesh == null) { trailMesh = new Mesh(); trailMesh.MarkDynamic(); }
            if (trailMat == null) InitializeMaterial();
            vertices.Clear(); uvs.Clear(); colors.Clear(); triangles.Clear(); distances.Clear();
            float length = 0f;
            for (int i = 0; i < drawingSamples.Count; i++)
            {
                if (i > 0) length += (drawingSamples[i].pos - drawingSamples[i - 1].pos).magnitude;
                distances.Add(length);
            }
            if (length < 0.0001f) return;

            for (int i = 0; i < drawingSamples.Count; i++)
            {
                Vector3 p = drawingSamples[i].pos;
                Vector3 incoming = (i > 0 ? p - drawingSamples[i - 1].pos : drawingSamples[i + 1].pos - p).normalized;
                Vector3 outgoing = (i + 1 < drawingSamples.Count ? drawingSamples[i + 1].pos - p : incoming).normalized;
                Vector3 tangent = incoming + outgoing;
                // At a reversal use the outgoing tangent, not its normal.
                tangent = tangent.sqrMagnitude > 0.0001f ? tangent.normalized : outgoing;
                float miter = Mathf.Clamp(1f / Mathf.Max(Vector3.Dot(tangent, outgoing), 0.01f), 1f, 3f);
                float age = Mathf.Clamp01((GenTicks.TicksGame - drawingSamples[i].time) / Lifetime);
                float t = Props.taperByLength ? Mathf.Max(age, 1f - distances[i] / length) : age;
                float width = Mathf.Max(0f, Props.width * (Props.widthCurve?.Evaluate(t) ?? 1f));
                Vector3 perpendicular = Vector3.Cross(tangent, Vector3.up) * (width * 0.5f * miter);
                float alpha = Mathf.Clamp01(Props.transparencyCurve?.Evaluate(t) ?? 1f);
                // Material supplies tint; vertices supply per-point alpha, avoiding squared tint.
                Color color = new Color(1f, 1f, 1f, alpha);
                vertices.Add(p + perpendicular); vertices.Add(p - perpendicular);
                uvs.Add(new Vector2(distances[i] / length, 0f));
                uvs.Add(new Vector2(distances[i] / length, 1f));
                colors.Add(color); colors.Add(color);
                if (i == 0) continue;
                int previous = (i - 1) * 2;
                triangles.Add(previous + 2); triangles.Add(previous + 1); triangles.Add(previous);
                triangles.Add(previous + 3); triangles.Add(previous + 1); triangles.Add(previous + 2);
            }
            trailMesh.Clear();
            trailMesh.SetVertices(vertices); trailMesh.SetUVs(0, uvs); trailMesh.SetColors(colors);
            trailMesh.SetTriangles(triangles, 0);
            trailMesh.RecalculateBounds();
            GenDraw.DrawMeshNowOrLater(trailMesh, Vector3.zero, Quaternion.identity, trailMat, false);
        }

        private void InitializeMaterial()
        {
            trailMat = MaterialPool.MatFrom(new MaterialRequest
            {
                mainTex = Props.trailTexture ?? BaseContent.WhiteTex,
                shader = Props.shaderType?.Shader ?? ShaderDatabase.Transparent,
                shaderParameters = Props.shaderParameters,
                color = Props.color
            });
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            base.PostDeSpawn(map, mode);
            if (trailMesh != null) UnityEngine.Object.Destroy(trailMesh);
            trailMesh = null;
            samples.Clear();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref active, "active", Props.defaultActive);
            // Cosmetic history is rebuilt after loading.
        }

        public Mesh TrailMesh => trailMesh;
        public Material TrailMaterial => trailMat;
    }
}

