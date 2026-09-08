using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate;

internal class CompProperties_OverrideShader : CompProperties
{
    public CompProperties_OverrideShader()
    {
        this.compClass = typeof(Comp_OverrideShader);
    }

}
