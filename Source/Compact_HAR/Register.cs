using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using UnityEngine;
using AlienRace;

namespace FixedPawnGenerate.Compact.HAR
{
    [StaticConstructorOnStartup]
    internal static class Register
    {
        public static bool IsHARActive => Compact_HAR.IsActive;

        public static Dictionary<ThingDef_AlienRace, List<AlienPartGenerator.BodyAddon>> bodyAddonDict = new Dictionary<ThingDef_AlienRace, List<AlienPartGenerator.BodyAddon>>();


        static Register()
        {
            if (IsHARActive)
            {
                RegisteMethod();
                Initialize();
            }

        }

        static void RegisteMethod()
        {
            Compact_HAR.SetAlienChannelColorFunc = SetColorChanel;
            Compact_HAR.SetPawnAddonFunc = SetBodyAddonVariant;
            Compact_HAR.SetAlienAddonColorFunc = SetBodyAddonColor;
        }

        static void Initialize()
        {
            //init bodyAddonDict
            bodyAddonDict = new Dictionary<ThingDef_AlienRace, List<AlienPartGenerator.BodyAddon>>();

            foreach (var alienDef in DefDatabase<ThingDef_AlienRace>.AllDefs)
            {
                List<AlienPartGenerator.BodyAddon> bodyAddons = alienDef?.alienRace?.generalSettings?.alienPartGenerator?.bodyAddons;

                if (!bodyAddons.NullOrEmpty())
                {
                    bodyAddonDict[alienDef] = bodyAddons;
                }
            }
        }

        private static AlienPartGenerator.AlienComp GetAlienComp(Pawn pawn)
        {
            return pawn.TryGetComp<AlienPartGenerator.AlienComp>();
        }

        private static int GetAddonIndex(string addonName, List<AlienPartGenerator.BodyAddon> list)
        {
            return list.FindIndex(b => b.Name == addonName);
        }


        //registered method
        private static void SetColorChanel(Pawn pawn, string colorChanel, Color? color, Color? colorTwo)
        {
            GetAlienComp(pawn)?.OverwriteColorChannel(colorChanel, color, colorTwo);
        }

        private static void SetBodyAddonVariant(Pawn pawn, string bodyAddonName, int variantIndex)
        {
            var alienComp = GetAlienComp(pawn);
            if (alienComp == null)
            {
                return;
            }

            ThingDef_AlienRace alienDef = pawn.def as ThingDef_AlienRace;

            if (!bodyAddonDict.ContainsKey(alienDef))
            {
                return;
            }

            var bodyAddon = bodyAddonDict[alienDef].FirstOrDefault(b => b.Name == bodyAddonName);
            if (bodyAddon == null)
            {
                return;
            }

            if (alienComp.addonVariants == null)
            {
                alienComp.CompRenderNodes();
            }

            int addonIndex = GetAddonIndex(bodyAddonName, bodyAddonDict[alienDef]);

            alienComp.addonVariants[addonIndex] = variantIndex;
        }

        private static void SetBodyAddonColor(Pawn pawn, string bodyAddonName, Color? color, Color? colorTwo)
        {
            var alienComp = GetAlienComp(pawn);
            if (alienComp == null)
            {
                return;
            }

            ThingDef_AlienRace alienDef = pawn.def as ThingDef_AlienRace;

            if (!bodyAddonDict.ContainsKey(alienDef))
            {
                return;
            }

            var bodyAddon = bodyAddonDict[alienDef].FirstOrDefault(b => b.Name == bodyAddonName);
            if (bodyAddon == null)
            {
                return;
            }

            if (alienComp.addonVariants == null)
            {
                alienComp.CompRenderNodes();
            }

            alienComp.OverwriteColorChannel(bodyAddon.ColorChannel, color, colorTwo);
        }

    }
}
