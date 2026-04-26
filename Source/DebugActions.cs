using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate
{

    public static class DebugActions
    {
        private static List<RenderTexture> pawnTextures = new List<RenderTexture>();

        [DebugAction("FixedPawnGenerate", "FPG: Log spawned pawns", false, false, false, false, allowedGameStates = AllowedGameStates.Playing)]
        private static void LogSpawnedPawns()
        {
            FixedPawnUtility.Manager.LogPawnDics();
        }
        [DebugAction("FixedPawnGenerate", "FPG: Log world pawns", false, false, false, false, allowedGameStates = AllowedGameStates.Playing)]
        private static void LogWorldPawns()
        {
            Find.WorldPawns.LogWorldPawns();
        }

        [DebugAction("FixedPawnGenerate", "FPG: Spawn fixed pawn", false, false, false, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static List<DebugActionNode> SpawnFixedPawn()
        {
            List<DebugActionNode> list = new List<DebugActionNode>();

            foreach (var def in DefDatabase<FixedPawnDef>.AllDefs)
            {
                DebugActionNode node = new DebugActionNode(def.isUnique ? $"{def.defName} ★" : def.defName, DebugActionType.ToolMap, delegate
                {
                    Pawn pawn = FixedPawnUtility.GenerateFixedPawnWithDef(def);
                    if (pawn != null)
                    {
                        GenPlace.TryPlaceThing(pawn, UI.MouseCell(), Find.CurrentMap, ThingPlaceMode.Near);
                    }
                });

                node.category = def.isUnique ? "Unique Pawns" : "Regular Pawns";

                list.Add(node);
            }

            list = list.OrderBy((DebugActionNode n) => n.label).ToList();

            return list;
        }



        [DebugAction("FixedPawnGenerate", "FPG: Export Pawn texture", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]

        private static void ExportPawnTex()
        {
            Pawn pawn = Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).OfType<Pawn>().FirstOrDefault();
            if (pawn != null)
            {
                if (pawnTextures.NullOrEmpty() || pawnTextures.Count < 4)
                {
                    pawnTextures = new List<RenderTexture>();

                    for (int i = 0; i < 4; i++)
                    {
                        RenderTexture rt = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);
                        pawnTextures.Add(rt);
                    }
                }

                List<Rot4> rot4s = new List<Rot4>() { Rot4.North, Rot4.East, Rot4.South, Rot4.West };

                foreach (var rot in rot4s)
                {
                    ExportPawnToImage(pawn, rot);
                }

                Messages.Message($"Exported {pawn.LabelShort}'s portrait to Desktop", MessageTypeDefOf.NeutralEvent);
            }
        }

        public static void ExportPawnToImage(Pawn pawn, Rot4 rot, int width = 512, int height = 512)
        {
            if (pawn == null) return;

            string pawnName = System.Text.RegularExpressions.Regex.Replace(pawn.LabelShort, "<[^>]+>", "");

            string exportDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "PawnTex", pawnName);

            RenderTexture rt = pawnTextures[(int)rot.AsInt];
            RenderTexture.active = rt;

            pawn.Drawer.renderer.SetAllGraphicsDirty();
            PortraitsCache.SetDirty(pawn);

            Find.PawnCacheRenderer.RenderPawn(pawn, rt, Vector3.zero, 1f, 0f, rot);

            Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            byte[] bytes = tex.EncodeToPNG();

            Directory.CreateDirectory(exportDirectory);
            string filePath = Path.Combine(exportDirectory, $"{rot.ToString()}.png");
            File.WriteAllBytes(filePath, bytes);

            UnityEngine.Object.Destroy(tex);
        }

        [DebugAction("FixedPawnGenerate", "FPG: Force assign fixedPawnDef", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static List<DebugActionNode> ForceAssignDef()
        {
            List<DebugActionNode> list = new List<DebugActionNode>();
            foreach (var def in DefDatabase<FixedPawnDef>.AllDefs)
            {
                DebugActionNode node = new DebugActionNode(def.defName, DebugActionType.ToolMap, delegate
                {
                    Pawn pawn = Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).OfType<Pawn>().FirstOrDefault();
                    if (pawn != null)
                    {
                        GameComponent_FixedPawn game = Current.Game?.GetComponent<GameComponent_FixedPawn>();
                        if (game != null)
                        {
                            game.AddPawn(pawn, def, true);

                            Messages.Message($"Assigned {def.defName} to {pawn.LabelShort}", MessageTypeDefOf.NeutralEvent);
                        }
                    }
                });

                if (def.isUnique)
                {
                    node.label += " ★";
                    node.category = "Unique Pawns";
                }
                else
                {
                    node.category = "Regular Pawns";
                }

                list.Add(node);
            }

            return list;
        }


        // Export local texture, for debug use
        //[DebugAction("FixedPawnGenerate", "FPG: Export Local texture", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ExportTexture()
        {
            List<string> paths = new List<string>()
            {
                "Things/Mote/PsychicLinkLine"
            };

            foreach (string path in paths)
            {
                ExportTexture(path);
            }
        }

        private static void ExportTexture(string path)
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

        private static void ExportTextureFolder(string folderPath)
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
