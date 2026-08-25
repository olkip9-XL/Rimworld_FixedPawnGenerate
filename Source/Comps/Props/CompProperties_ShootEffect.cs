using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;

namespace FixedPawnGenerate;

internal class CompProperties_ShootEffect : CompProperties
{
    public CompProperties_ShootEffect()
    {
        this.compClass = typeof(Comp_ShootEffect);
    }

    public EffecterDef effecterDef;

    public float effecterSpawnDistance = 1f;
}
