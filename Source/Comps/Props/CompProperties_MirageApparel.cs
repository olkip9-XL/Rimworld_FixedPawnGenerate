using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate;

public class CompProperties_MirageApparel : CompProperties
{
    public CompProperties_MirageApparel()
    {
        this.compClass = typeof(Comp_MirageApparel);
    }

    public List<ThingDefCountClass> overrideApparel = null;

}
