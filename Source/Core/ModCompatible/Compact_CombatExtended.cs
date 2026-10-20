using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate;

public static class Compact_CombatExtended
{
    public static bool IsActive => ModLister.HasActiveModWithName("Combat Extended");

    public static Func<Thing, Thing> GenerateGunAmmoFunc;

    public static Thing GenerateGunAmmo(Thing gun)
    {
        return GenerateGunAmmoFunc?.Invoke(gun);
    }

}
