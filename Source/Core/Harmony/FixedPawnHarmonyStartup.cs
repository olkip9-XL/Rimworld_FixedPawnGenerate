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
        }
    }
}
