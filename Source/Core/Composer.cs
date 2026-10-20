using System;
using UnityEngine;
using Verse;

public static class TachieTextureComposer
{
    public static RenderTexture MergeTextures(
        Texture2D topTex,
        Texture2D bottomTex,
        Vector2Int offset)
    {
        if (topTex == null)
            throw new ArgumentNullException(nameof(topTex));

        if (bottomTex == null)
            throw new ArgumentNullException(nameof(bottomTex));

        //左下角原点
        offset = new Vector2Int(offset.x, bottomTex.height - offset.y - topTex.height);

        int width = bottomTex.width;
        int height = bottomTex.height;

        RenderTexture previous = RenderTexture.active;
        RenderTexture result = null;
        Material material = null;

        try
        {
            Shader shader = DefDatabase<ShaderDef>
                .GetNamed("FPG_TachieComposite").Shader;

            material = new Material(shader);

            material.SetTexture("_OverlayTex", topTex);
            material.SetVector(
                "_OverlayRect",
                new Vector4(
                    offset.x / (float)width,
                    offset.y / (float)height,
                    topTex.width / (float)width,
                    topTex.height / (float)height));

            result = new RenderTexture(
                width, height, 0, RenderTextureFormat.ARGB32);

            result.name = "FPG_TachieComposite";
            result.filterMode = FilterMode.Trilinear;
            //result.wrapMode = TextureWrapMode.Clamp;
            result.useMipMap = true;
            //result.autoGenerateMips = false;

            if (!result.Create())
                throw new InvalidOperationException(
                    "无法创建立绘合成 RenderTexture。");

            // Blit 会将 bottomTex 设置为材质的 _MainTex。
            Graphics.Blit(bottomTex, result, material, 0);

            return result;
        }
        catch
        {
            if (result != null)
            {
                result.Release();
                UnityEngine.Object.Destroy(result);
            }

            throw;
        }
        finally
        {
            RenderTexture.active = previous;

            if (material != null)
                UnityEngine.Object.Destroy(material);
        }
    }

    public static Texture2D MergeTexturesToTexture2D(
        Texture2D topTex,
        Texture2D bottomTex,
        Vector2Int offset)
    {

        Texture2D result = null;
        RenderTexture previous = RenderTexture.active;
        RenderTexture merged = null;

        try
        {
            merged = MergeTextures(topTex, bottomTex, offset);

            result = new Texture2D(merged.width, merged.height, TextureFormat.ARGB32, true);
            result.anisoLevel = 2;
            result.filterMode = FilterMode.Trilinear;

            RenderTexture.active = merged;
            result.ReadPixels(new Rect(0, 0, merged.width, merged.height), 0, 0);
            result.Apply(updateMipmaps: true);
        }
        catch (Exception e)
        {


        }
        finally
        {
            RenderTexture.active = previous;
            if (merged != null)
            {
                merged.Release();
                UnityEngine.Object.Destroy(merged);
            }
        }

        return result;
    }
}