using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using Verse;

namespace FixedPawnGenerate
{
    [StaticConstructorOnStartup]
    public static class FixedPawnHarmonyPatchStartup
    {
        static FixedPawnHarmonyPatchStartup()
        {
            new Harmony("Lotus.FixedPawnGenerate").PatchAll();

            //PrintSth();
            //ExportTexture();
        }

        //test
        static void PrintSth()
        {
            //打印所有shader的属性信息
            StringBuilder sb = new StringBuilder();

            List<string> loggedShader = new List<string>();

            foreach (var shaderType in DefDatabase<ShaderTypeDef>.AllDefsListForReading)
            {
                Shader shader = shaderType.Shader;

                if (loggedShader.Contains(shader.name))
                {
                    continue;
                }
                else
                {
                    loggedShader.Add(shader.name);
                }

                sb.AppendLine(shader.name);
                sb.AppendLine("| 名字 | ID | 类型 | 描述 | 标志 | 默认值 |");
                sb.AppendLine("| --- | --- | --- | --- | --- | --- |");

                for (int i = 0; i < shader.GetPropertyCount(); i++)
                {
                    string name = shader.GetPropertyName(i);
                    int id = shader.GetPropertyNameId(i);
                    ShaderPropertyType type= shader.GetPropertyType(i);
                    string description = shader.GetPropertyDescription(i);
                    ShaderPropertyFlags flags = shader.GetPropertyFlags(i);

                    switch (type)
                    {
                        case ShaderPropertyType.Color:
                            sb.AppendLine($"| {name} | {id} | {type} | {description} | {flags} | Color |");
                            break;
                        case ShaderPropertyType.Vector:
                            sb.AppendLine($"| {name} | {id} | {type} | {description} | {flags} | {shader.GetPropertyDefaultVectorValue(i)} |");
                            break;
                        case ShaderPropertyType.Float:
                            sb.AppendLine($"| {name} | {id} | {type} | {description} | {flags} | {shader.GetPropertyDefaultFloatValue(i)} |");   
                            break;
                        case ShaderPropertyType.Range:
                            sb.AppendLine($"| {name} | {id} | {type} | {description} | {flags} | {shader.GetPropertyRangeLimits(i)} |");
                            break;
                        case ShaderPropertyType.Texture:
                            sb.AppendLine($"| {name} | {id} | {type} | {description} | {flags} | {shader.GetPropertyTextureDefaultName(i)} |");
                            break;
                        case ShaderPropertyType.Int:
                            sb.AppendLine($"| {name} | {id} | {type} | {description} | {flags} | {shader.GetPropertyDefaultIntValue(i)} |");
                            break;
                        default:
                            break;
                    }
                }
                sb.AppendLine();
            }

            Log.Warning( sb.ToString() );
        }

        private static void ExportTexture()
        {
            //测试
            List<string> paths = new List<string>()
            {
               "Things/Mote/SparkThrownBlue",
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
}
