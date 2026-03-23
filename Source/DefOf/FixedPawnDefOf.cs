using System;
using RimWorld;
using Verse;

namespace FixedPawnGenerate
{
    [DefOf]
    public static class FixedPawnDefOf
    {
        static FixedPawnDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(FixedPawnDefOf));
        }

    }
}