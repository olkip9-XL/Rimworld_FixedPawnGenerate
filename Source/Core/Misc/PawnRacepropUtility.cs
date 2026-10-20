using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate
{
    internal static class PawnRacepropUtility
    {
        public static HashSet<Pawn> simplePawns = new HashSet<Pawn>();

        public static Dictionary<ThingDef, RaceProperties> alterRaceProps = new Dictionary<ThingDef, RaceProperties>();

        public static void AddAlteredRaceProp(Pawn pawn)
        {
            simplePawns.Add(pawn);

            ThingDef def = pawn.def;
            if (!alterRaceProps.ContainsKey(def))
            {
                RaceProperties props = new RaceProperties();
                Type type = props.GetType();

                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var f in fields)
                {
                    f.SetValue(props, f.GetValue(pawn.def.race));
                }

                props.renderTree = FPG_RenderTreeDefOf.SimplePawn;

                alterRaceProps[def] = props;
            }
        }

        public static RaceProperties TryGetAlteredRaceProp(this Pawn pawn)
        {
            if (simplePawns.Contains(pawn))
            {
                return alterRaceProps[pawn.def];
            }
            else
            {
                return null;
            }
        }

    }
}
