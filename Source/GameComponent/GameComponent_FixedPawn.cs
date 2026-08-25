using HarmonyLib;
using RimWorld;
using RuntimeAudioClipLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate
{
    public class GameComponent_FixedPawn : GameComponent
    {
        public Game game;

        private List<Pawn> workingPawnList;
        private List<FixedPawnDef> workingDefList;


        //old
        //public List<FixedPawnDef> uniqePawns = new List<FixedPawnDef>();

        //加载存档时使用
        private Dictionary<string, FixedPawnDef> pawnDics = new Dictionary<string, FixedPawnDef>();

        //old
        //private Dictionary<Pawn, FixedPawnDef> cachedPawns = new Dictionary<Pawn, FixedPawnDef>();

        private Dictionary<Pawn, FixedPawnDef> spawnedPawns = new Dictionary<Pawn, FixedPawnDef>();

        internal List<FixedPawnDef> spawnedUniquePawns = new List<FixedPawnDef>();

        public GameComponent_FixedPawn(Game game) : base()
        {
            this.game = game;
        }
        public override void LoadedGame()
        {
            base.LoadedGame();

            //兼容旧存档
            if (pawnDics != null && pawnDics.Count > 0 && (!this.spawnedPawns.Any() || !this.spawnedUniquePawns.Any()))
            {
                //Log.Warning("Compatibility with old saves in progress");

                foreach (Map map in Find.Maps)
                {
                    foreach (Pawn pawn in map.mapPawns.AllPawns)
                    {
                        if (pawnDics.TryGetValue(pawn.ThingID, out FixedPawnDef def))
                        {
                            spawnedPawns.AddDistinct(pawn, def);
                            if (def.isUnique)
                                spawnedUniquePawns.AddDistinct(def);
                        }
                    }
                }

                foreach (Pawn pawn in Find.WorldPawns.AllPawnsAliveOrDead)
                {
                    if (pawnDics.TryGetValue(pawn.ThingID, out FixedPawnDef def))
                    {
                        spawnedPawns.AddDistinct(pawn, def);
                        if (def.isUnique && pawn.relations.everSeenByPlayer == true)
                            spawnedUniquePawns.AddDistinct(def);
                    }
                }
            }


        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();

            this.spawnedUniquePawns.AddRange(Find.GameInitData.startingAndOptionalPawns
                    .Select(pawn => pawn.GetFixedPawnDef())
                    .Where(def => def != null && def.isUnique));


            //remove non-unique pawns;
            spawnedPawns.RemoveAll(x => !Find.GameInitData.startingAndOptionalPawns.Contains(x.Key) && !x.Value.isUnique);

            //pass to world
            foreach (var pair in spawnedPawns)
            {
                if (pair.Key.GetPawnPositionState() == PawnPositionState.OTHER)
                {
                    Find.WorldPawns.PassToWorld(pair.Key, RimWorld.Planet.PawnDiscardDecideMode.KeepForever);

                    Faction faction = null;
                    if (pair.Value.faction != null)
                        faction = Find.FactionManager.FirstFactionOfDef(pair.Value.faction);

                    pair.Key.SetFaction(faction);
                }
            }
        }

        /*LoadingVars -> ResolvingCrossRefs -> PostLoadInit*/
        public override void ExposeData()
        {
            base.ExposeData();

            //构建pawnDics
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                pawnDics.Clear();
                try
                {
                    foreach (var pair in spawnedPawns)
                    {
                        if (pair.Key == null)
                        {
                            Log.Error($"[FixedPawnGenerate] ExposeData: spawnedPawns contains null key. Value: {pair.Value?.defName ?? "null"}");
                            continue;
                        }

                        if (pair.Value == null)
                        {
                            Log.Error($"[FixedPawnGenerate] ExposeData: spawnedPawns contains null value. Key: {pair.Key?.LabelShort ?? "null"}");
                            continue;
                        }

                        pawnDics.Add(pair.Key.ThingID, pair.Value);
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"[FixedPawnGenerate] ExposeData: Error while constructing pawnDics from spawnedPawns. spawnedPawns count: {spawnedPawns.Count}, pawnDics count: {pawnDics.Count},\n {e}");
                }
            }

            //pawnDics为加载存档时构建comps时使用
            Scribe_Collections.Look(ref pawnDics, "pawnDics", LookMode.Value, LookMode.Def);
            Scribe_Collections.Look<Pawn, FixedPawnDef>(ref spawnedPawns, "spawnedPawns", LookMode.Reference, LookMode.Def, ref workingPawnList, ref workingDefList, true, true, true);

            Scribe_Collections.Look(ref spawnedUniquePawns, "spawnedUniquePawns", LookMode.Def);

            //null list check
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                spawnedPawns ??= new Dictionary<Pawn, FixedPawnDef>();

                pawnDics ??= new Dictionary<string, FixedPawnDef>();

                spawnedUniquePawns ??= new List<FixedPawnDef>();

                spawnedPawns.RemoveAll(x => x.Key == null && x.Value == null);
            }
        }

        internal FixedPawnDef GetDef(Pawn pawn)
        {
            if (pawn == null)
            {
                return null;
            }

            FixedPawnDef def = null;

            if (pawnDics.TryGetValue(pawn.ThingID, out def))
            {
                return def;
            }

            if (spawnedPawns.TryGetValue(pawn, out def))
            {
                return def;
            }

            return null;
        }
        internal Pawn GetPawn(FixedPawnDef def)
        {
            return spawnedPawns.FirstOrDefault(x => x.Value == def).Key;
        }

        internal void AddPawn(Pawn pawn, FixedPawnDef def, bool forceAdd = false)
        {
            if (pawn == null || def == null)
            {
                Log.Error($"[FixedPawnGenerate] Attempted to add pawn {pawn?.LabelShort ?? "null"} with fixedPawnDef {def?.defName ?? "null"}, forceAdd: {forceAdd}");
                return;
            }

            if (!spawnedPawns.ContainsKey(pawn))
            {
                spawnedPawns.Add(pawn, def);

                if (forceAdd)
                {
                    string defLabel = def.isUnique ? $"{def.defName}★" : def.defName;

                    Log.Warning($"[FixedPawnGenerate] Assigned {defLabel} to {pawn.LabelShort}");
                }
            }
            else if (forceAdd)
            {
                FixedPawnDef originalDef = spawnedPawns[pawn];
                string originalDefLabel = originalDef.isUnique ? $"{originalDef.defName}★" : originalDef.defName;
                string defLabel = def.isUnique ? $"{def.defName}★" : def.defName;

                spawnedPawns[pawn] = def;

                if (def.isUnique)
                {
                    spawnedUniquePawns.AddDistinct(def);
                }

                Log.Warning($"[FixedPawnGenerate] Reassigned {defLabel} to {pawn.LabelShort}, original def was {originalDefLabel}");
            }
        }

        internal void RemovePawn(Pawn pawn)
        {
            spawnedPawns.Remove(pawn);
        }

        public void LogPawnDics()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("============Spawned Pawns============");

            int count = 0;

            TablePrinter tp = new TablePrinter(new List<string> { "Name", "Def", "Location", "ThingID" });

            foreach (var pair in spawnedPawns)
            {
                Pawn pawn = pair.Key;
                FixedPawnDef def = pair.Value;

                string location = "Error";

                switch (pawn.GetPawnPositionState())
                {
                    case PawnPositionState.IN_MAP:
                        location = $"Map[{pawn.Map.uniqueID}]";
                        break;
                    case PawnPositionState.WORLD_PAWN:
                        location = "World Pawn";
                        break;
                    case PawnPositionState.IN_CONTAINER:
                        location = "In Container Enclosed";
                        break;
                    case PawnPositionState.IN_CORPSE:
                        location = "In Corpse/Unnatural";
                        break;
                    case PawnPositionState.IN_OTHER_HOLDER:
                        location = "In Unknown Holder";
                        break;
                    case PawnPositionState.IN_CARAVAN:
                        location = "In Caravan";
                        break;
                    case PawnPositionState.OTHER:
                        location = "None";
                        break;
                    case PawnPositionState.ERROR:
                        break;
                }

                // 将序号与名称合并后整体格式化，避免前缀导致的偏移
                string defField = def.defName + (def.isUnique ? "★" : "");

                tp.AddRow(new List<string> { pawn.Name.ToString(), defField, location, pawn.ThingID });
            }

            sb.AppendLine(tp.ToString());

            sb.AppendLine();
            sb.AppendLine("============Unique Pawns============");
            count = 0;

            foreach (var item in spawnedUniquePawns)
            {
                string tagsStr = item.tags != null ? string.Join(", ", item.tags) : "None";
                sb.AppendLine($"[{count++}] {item.defName}: {item.name}, tags: {tagsStr}");
            }

            Log.Warning(sb.ToString());
        }

        internal bool IsSpawned(FixedPawnDef def)
        {
            return this.spawnedUniquePawns.Contains(def);
        }

    }

}
