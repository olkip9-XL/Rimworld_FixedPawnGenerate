using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate
{
    public class PawnStyleData
    {
        public string label;

        public HairDef hairDef;

        public Dictionary<ThingDef, ThingStyleDef> apparelStyleDefs = new Dictionary<ThingDef, ThingStyleDef>();

        public int tachieId = -1;

        public BackstoryDef childBackStoryDef;

        public BackstoryDef adultBackStoryDef;

        public string pawnVoiceClipPath;
    }

    internal class CompProperties_SwitchStyle : CompProperties
    {
        public CompProperties_SwitchStyle()
        {
            this.compClass = typeof(Comp_SwitchStyle);
        }

        public List<PawnStyleData> altStyles = new List<PawnStyleData>();
    }
}
