using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate.Compact.CombatExtended
{
    [StaticConstructorOnStartup]
    public static class Register
    {
        static bool IsCEActive => Compact_CombatExtended.IsActive;

        static IntRange magRange = new IntRange(3, 5);

        static Register()
        {
            if (IsCEActive)
            {
                RegisterMethods();
            }
        }

        static void RegisterMethods()
        {
            Compact_CombatExtended.GenerateGunAmmoFunc = GenerateGunAmmo;
        }

        private static Thing GenerateGunAmmo(Thing gun)
        {
            if(gun == null)
            {
                return null;
            }

            if (gun.TryGetComp<CompAmmoUser>() is CompAmmoUser ammoUser && ammoUser.Props.ammoSet != null)
            {
                CompProperties_AmmoUser props = ammoUser.Props;

                int count = props.AmmoGenPerMagOverride;
                if (count <= 0)
                {
                    count = Mathf.Max(props.magazineSize, 1);
                }
                count *= magRange.RandomInRange;

                ThingDef ammoDef = CE_ThingSetMakerUtility.GetAmmoDef(props, true, true);
                Thing ammo = ThingMaker.MakeThing(ammoDef);
                ammo.stackCount = count;

                return ammo;
            }
            return null;
        }

    }
}
