using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate
{
    internal class Comp_SwitchStyle : ThingComp
    {
        public int currentStyleIndex = -1;
        public CompProperties_SwitchStyle Props => (CompProperties_SwitchStyle)this.props;

        Pawn pawn => parent as Pawn;

        PawnStyleData baseDataInt;
        PawnStyleData BaseData
        {
            get
            {
                if (baseDataInt == null)
                {
                    baseDataInt = new PawnStyleData();

                    FixedPawnDef fixedPawnDef = pawn.GetFixedPawnDef();
                    if (fixedPawnDef == null)
                    {
                        Log.Error($"Failed to find FixedPawnDef for {pawn} when initializing base PawnStyleData.");
                        return null;
                    }
                    baseDataInt.label = "base";
                    baseDataInt.hairDef = fixedPawnDef.hair;
                    baseDataInt.apparelStyleDefs = new Dictionary<ThingDef, ThingStyleDef>();
                    foreach (var apparel in fixedPawnDef.apparel)
                    {
                        baseDataInt.apparelStyleDefs[apparel.thing] = null;
                    }
                    baseDataInt.tachieId = -1;
                    baseDataInt.childBackStoryDef = fixedPawnDef.childHood;
                    baseDataInt.adultBackStoryDef = fixedPawnDef.adultHood;

                    CompProperties_PawnVoice compPropsPawnVoice = fixedPawnDef.comps.FirstOrDefault(c => c is CompProperties_PawnVoice) as CompProperties_PawnVoice;
                    if (compPropsPawnVoice != null)
                    {
                        baseDataInt.pawnVoiceClipPath = compPropsPawnVoice.clipsPath;
                    }
                }
                return baseDataInt;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            //reset to current style after load
            if (respawningAfterLoad)
            {
                this.SetStyle(currentStyleIndex);
            }
        }

        public void SetStyle(int index)
        {
            PawnStyleData data;

            if (index < 0 || index >= Props.altStyles.Count)
                data = BaseData;
            else
                data = Props.altStyles[index];

            if (data == null)
                return;

            //hair
            if (data.hairDef != null)
                pawn.story.hairDef = data.hairDef;

            //apparel
            if (data.apparelStyleDefs != null)
            {
                foreach (var apparel in pawn.apparel.WornApparel)
                {
                    if (data.apparelStyleDefs.TryGetValue(apparel.def, out ThingStyleDef styleDef))
                    {
                        apparel.StyleDef = styleDef;
                    }
                }
            }
            Comp_MirageApparel compMirageApparel = pawn.GetComp<Comp_MirageApparel>();
            if (compMirageApparel != null)
            {
                compMirageApparel.SetApparelStyle(data.apparelStyleDefs);
            }

            //tachie
            CompTachie compTachie = pawn.GetComp<CompTachie>();
            if (compTachie != null)
            {
                compTachie.SetAlterTachie(data.tachieId);
            }

            //voice
            CompPawnVoice compPawnVoice = pawn.GetComp<CompPawnVoice>();
            if (compPawnVoice != null && !data.pawnVoiceClipPath.NullOrEmpty())
            {
                compPawnVoice.Props.SetOverrideClipsPath(data.pawnVoiceClipPath);
            }

            //backstory
            if (data.childBackStoryDef != null)
                pawn.story.Childhood = data.childBackStoryDef;
            if (data.adultBackStoryDef != null)
                pawn.story.Adulthood = data.adultBackStoryDef;

            //effecter
            DebugActionsUtility.DustPuffFrom(pawn);
            pawn.Drawer.renderer.SetAllGraphicsDirty();
            currentStyleIndex = index;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref currentStyleIndex, "currentStyleIndex", 0);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    action = () =>
                    {
                        currentStyleIndex = (currentStyleIndex + 1) % (Props.altStyles.Count + 1);
                        SetStyle(currentStyleIndex);
                    },
                    defaultLabel = "DEV: Switch Style",
                    defaultDesc = "Switch to the next style.",
                };
            }

            yield break;
        }

    }
}
