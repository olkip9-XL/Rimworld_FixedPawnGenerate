using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace FixedPawnGenerate
{
    internal class Comp_ThingTrail : ThingComp
    {
        private struct Sample { public Vector3 pos; public float time; }

        public CompProperties_ThingTrail Props => (CompProperties_ThingTrail)props;

        public bool active = false;

        private List<Sample> samples = new List<Sample>();
        private Mesh trailMesh;
        private Material trailMat;
        private const float sampleDistance = 0.35f;
        private const float width = 0.6f;
        private Vector3 lastPos = Vector3.zero;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (!respawningAfterLoad)
                active = Props.defaultActive;
        }

        public override void CompTick()
        {
            base.CompTick();

            //在路径上绘制Fleck
            if (active && !samples.NullOrEmpty() && Props.trailFleck != null && Props.trailFleckChanceCurve != null)
            {
                int sampleCount = samples.Count;
                for (int i = 0; i < sampleCount; i++)
                {
                    Sample sample = samples[i];
                    float t = 1 - (i / (float)(sampleCount - 1));

                    if (Rand.Chance(Props.trailFleckChanceCurve.Evaluate(t)))
                    {
                        float width = Props.widthCurve?.Evaluate(t) ?? 1f;
                        width /= 2f;

                        Vector3 spawnPos = new Vector3(sample.pos.x + Rand.Range(-width, width), sample.pos.y, sample.pos.z + Rand.Range(-width, width));

                        FleckMaker.Static(spawnPos, parent.Map, Props.trailFleck, 1f);
                    }
                }
            }
        }

        private void SampleCurrent()
        {
            if (!active) return;

            Vector3 cur = parent.DrawPos;

            if (lastPos == Vector3.zero) lastPos = cur;

            if ((cur - lastPos).MagnitudeHorizontal() >= sampleDistance)
            {
                for (int i = 0; i < Props.samplesPerTick; i++)
                {
                    Vector3 samplePos = Vector3.Lerp(lastPos, cur, (i + 1) / (float)Props.samplesPerTick);
                    float sampleTime = Mathf.Lerp(GenTicks.TicksGame - 1, GenTicks.TicksGame, (i + 1) / (float)Props.samplesPerTick);


                    Sample sample = new Sample { pos = samplePos, time = sampleTime };
                    sample.pos.y = Props.altitudeLayer.AltitudeFor() + Props.altitudeOffset;
                    samples.Add(sample);
                }

                lastPos = cur;
            }

            // 移除过期样本
            int now = GenTicks.TicksGame;
            samples.RemoveAll(s => now - s.time > Props.lifeTimeTicks);
        }

        public override void PostDraw()
        {
            if (!active) return;

            SampleCurrent();

            if (samples.Count < 2) return;

            if (trailMesh == null)
            {
                trailMesh = new Mesh();
            }
            if (trailMat == null)
            {
                InitializeMaterial();
            }

            // 构建顶点（两顶点/样本），uv, color, idx
            int n = samples.Count;
            int vCount = n * 2;
            int tCount = (n - 1) * 6;
            Vector3[] verts = new Vector3[vCount];
            Vector2[] uvs = new Vector2[vCount];
            Color[] cols = new Color[vCount];
            int[] tris = new int[tCount];

            const float maxMiterMultiplier = 3f; // 限制尖角处的斜接长度，避免出现夸张的尖刺

            int now = GenTicks.TicksGame;
            for (int i = 0; i < n; i++)
            {
                Vector3 p = samples[i].pos;

                // 分别取"进入方向"和"离开方向"，用于计算 miter（斜接）平滑的宽度向量
                Vector3 dirIn = (i > 0) ? (p - samples[i - 1].pos) : (samples[i + 1].pos - p);
                Vector3 dirOut = (i < n - 1) ? (samples[i + 1].pos - p) : (p - samples[i - 1].pos);
                dirIn.y = 0f;
                dirOut.y = 0f;
                dirIn.Normalize();
                dirOut.Normalize();

                Vector3 miterDir = dirIn + dirOut;
                float miterScale = 1f;
                if (miterDir.sqrMagnitude > 0.0001f)
                {
                    miterDir.Normalize();
                    // 斜接长度补偿：夹角越尖锐，需要的宽度越大，但要限制上限避免出现尖刺
                    float cosHalfAngle = Vector3.Dot(miterDir, dirOut);
                    miterScale = Mathf.Clamp(1f / Mathf.Max(cosHalfAngle, 0.01f), 1f, maxMiterMultiplier);
                }
                else
                {
                    // dirIn 和 dirOut 完全相反（180度急转），退化为使用其中一个方向的法线
                    miterDir = Vector3.Cross(dirOut, Vector3.up).normalized;
                }

                float t = Mathf.Clamp01(((float)now - samples[i].time) / (float)Props.lifeTimeTicks); // 0 newest, 1 oldest

                float tWidth = Props.widthCurve?.Evaluate(t) ?? 1f;
                Vector3 perp = Vector3.Cross(miterDir, Vector3.up) * (width * 0.5f * tWidth * miterScale);

                float alpha = Props.transparencyCurve?.Evaluate(t) ?? 1f;
                Color col = new Color(Props.color.r, Props.color.g, Props.color.b, alpha);

                verts[i * 2] = p + perp;
                verts[i * 2 + 1] = p - perp;
                uvs[i * 2] = new Vector2(i / (float)(n - 1), 0);
                uvs[i * 2 + 1] = new Vector2(i / (float)(n - 1), 1);
                cols[i * 2] = col;
                cols[i * 2 + 1] = col;
            }

            int ti = 0;
            for (int i = 0; i < n - 1; i++)
            {
                int i0 = i * 2;
                tris[ti++] = i0 + 2;
                tris[ti++] = i0 + 1;
                tris[ti++] = i0;

                tris[ti++] = i0 + 3;
                tris[ti++] = i0 + 1;
                tris[ti++] = i0 + 2;
            }

            trailMesh.Clear();
            trailMesh.vertices = verts;
            trailMesh.uv = uvs;
            trailMesh.colors = cols;
            trailMesh.triangles = tris;
            trailMesh.RecalculateBounds();

            // 绘制；在 RimWorld 中使用正确的坐标系和渲染通道
            GenDraw.DrawMeshNowOrLater(trailMesh, Vector3.zero, Quaternion.identity, trailMat, drawNow: false);
        }

        private void InitializeMaterial()
        {
            Shader shader = Props.shaderType?.Shader ?? ShaderDatabase.Transparent;
            MaterialRequest request = new MaterialRequest();
            request.mainTex = Props.trailTexture;
            request.shaderParameters = Props.shaderParameters ?? new List<ShaderParameter>();
            request.shader = shader;
            request.color = Props.color;
            trailMat = MaterialPool.MatFrom(request);
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref active, "active", true);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (samples == null)
                    samples = new List<Sample>();
            }
        }

        public Mesh TrailMesh => trailMesh;
        public Material TrailMaterial => trailMat;
    }
}