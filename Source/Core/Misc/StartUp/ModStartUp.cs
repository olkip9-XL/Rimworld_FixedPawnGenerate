using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Rendering;

using Verse;

namespace FixedPawnGenerate;

[StaticConstructorOnStartup]
internal static class ModStartUp
{
    static ModStartUp()
    {
        foreach (var def in DefDatabase<FixedPawnDef>.AllDefs)
        {
            try
            {
                PatchForcedGenes(def);
            }
            catch (Exception e)
            {
                Log.Error($"Error patching forced genes for FixedPawnDef {def.defName}: {e}");
            }
        }
    }

    private static void PatchForcedGenes(FixedPawnDef def)
    {
        //为体型添加ForcedGenes
        if (def.bodyType != null)
        {
            if (def.forcedGenes == null)

            {
                def.forcedGenes = new List<GeneDef>();
            }

            if (def.bodyType == BodyTypeDefOf.Female || def.bodyType == BodyTypeDefOf.Male)
            {
                GeneDef geneDef = DefDatabase<GeneDef>.GetNamed("Body_Standard");
                if (geneDef != null && !def.forcedGenes.Contains(geneDef))
                {
                    def.forcedGenes.Add(geneDef);
                }
            }
            else if (def.bodyType == BodyTypeDefOf.Thin)
            {
                GeneDef geneDef = DefDatabase<GeneDef>.GetNamed("Body_Thin");
                if (geneDef != null && !def.forcedGenes.Contains(geneDef))
                {
                    def.forcedGenes.Add(geneDef);
                }
            }
            else if (def.bodyType == BodyTypeDefOf.Hulk)
            {
                GeneDef geneDef = DefDatabase<GeneDef>.GetNamed("Body_Hulk");
                if (geneDef != null && !def.forcedGenes.Contains(geneDef))
                {
                    def.forcedGenes.Add(geneDef);
                }
            }
            else if (def.bodyType == BodyTypeDefOf.Fat)
            {
                GeneDef geneDef = DefDatabase<GeneDef>.GetNamed("Body_Fat");
                if (geneDef != null && !def.forcedGenes.Contains(geneDef))
                {
                    def.forcedGenes.Add(geneDef);
                }
            }
        }
    }


    //test
    static void PrintSth()
    {
    }

    static void ExportTexture()
    {
        //测试
        List<string> paths = new List<string>()
            {
               "Things/Building/Childcare/ToyBox",
               "Things/Building/Childcare/ToyBox_m",
            };

        List<string> folderPaths = new List<string>()
        {
            //"Things/Building/Misc/Lightball/Lightball_Overlay"
        };

        foreach (string path in paths)
        {
            ExportTexture(path);
        }

        foreach (string folderPath in folderPaths)
        {
            ExportTextureFolder(folderPath);
        }

        static void ExportTexture(string path)
        {
            Texture2D tex = ContentFinder<Texture2D>.Get(path);
            if (tex == null)
            {
                Log.Error($"Texture not found: {path}");
                return;
            }
            int width = tex.width;
            int height = tex.height;
            RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            RenderTexture.active = rt;
            Graphics.Blit(tex, rt);
            Texture2D tempTex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
            tempTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tempTex.Apply();
            RenderTexture.active = null;
            //create dir
            string folderPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "RimworldTexture");
            System.IO.Directory.CreateDirectory(folderPath);
            byte[] bytes = tempTex.EncodeToPNG();
            System.IO.File.WriteAllBytes(System.IO.Path.Combine(folderPath, $"{path.Replace("/", "_")}.png"), bytes);
        }

        static void ExportTextureFolder(string folderPath)
        {
            foreach (var tex in ContentFinder<Texture2D>.GetAllInFolder(folderPath))
            {
                if (tex == null)
                    continue;

                int width = tex.width;
                int height = tex.height;

                RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);

                RenderTexture.active = rt;
                Graphics.Blit(tex, rt);

                Texture2D tempTex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
                tempTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                tempTex.Apply();
                RenderTexture.active = null;

                //create dir
                string exportDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "RimworldTexture");
                System.IO.Directory.CreateDirectory(exportDirectory);
                byte[] bytes = tempTex.EncodeToPNG();
                System.IO.File.WriteAllBytes(System.IO.Path.Combine(exportDirectory, $"{tex.name}.png"), bytes);
            }
        }
    }
}
