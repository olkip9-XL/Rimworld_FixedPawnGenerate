using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate;

public class TransformWorker_Items : TransformWorker
{
    public override bool Update(int elapsed, Pawn pawn)
    {
        if (subTransformDef == null || pawn == null)
        {
            return false;
        }

        bool changed = false;

        ThingOwner<ThingWithComps> storage = pawn.GetComp<Comp_OverrideShader>().GetDirectlyHeldThings() as ThingOwner<ThingWithComps>;
        ItemAddMode addMode = subTransformDef.itemAddMode;
        ItemRemoveMode removeMode = subTransformDef.itemRemoveMode;

        if (storage == null)
        {
            return changed;
        }

        //移除物品
        switch (removeMode)
        {
            case ItemRemoveMode.Destroy:
                pawn.apparel?.DestroyAll();
                pawn.equipment?.DestroyAllEquipment();
                break;
            case ItemRemoveMode.Drop:
                pawn.apparel?.DropAll(pawn.Position);
                pawn.equipment?.DropAllEquipment(pawn.Position);
                break;
            case ItemRemoveMode.Storage:
                pawn.apparel?.GetDirectlyHeldThings().TryTransferAllToContainer(storage);
                pawn.equipment?.GetDirectlyHeldThings().TryTransferAllToContainer(storage);
                break;
            default:
                break;
        }

        //添加物品
        switch (addMode)
        {
            case ItemAddMode.MakeNew:
                AddItemsFromNew(pawn);
                break;
            case ItemAddMode.Storage:
                AddItemsFromStorage(pawn, storage);
                break;
            default:
                break;
        }
        return changed;
    }

    private void AddItemsFromNew(Pawn pawn)
    {
        //衣服
        if (!subTransformDef.apparels.NullOrEmpty() && pawn.apparel != null)
        {
            foreach (ThingDef def in subTransformDef.apparels)
            {
                if (def == null)
                {
                    continue;
                }
                Apparel apparel = ThingMaker.MakeThing(def) as Apparel;
                if (apparel != null)
                {
                    pawn.apparel.Wear(apparel, true, locked: subTransformDef.lockApparel);
                }
            }
        }

        //武器
        if (!subTransformDef.weapons.NullOrEmpty() && pawn.equipment != null)
        {
            foreach (ThingDef def in subTransformDef.weapons)
            {
                if (def == null)
                {
                    continue;
                }
                ThingWithComps weapon = ThingMaker.MakeThing(def) as ThingWithComps;
                if (weapon != null)
                {
                    if (weapon.def.equipmentType == EquipmentType.Primary && pawn.equipment.Primary != null)
                    {
                        pawn.inventory.TryAddAndUnforbid(weapon);
                    }
                    else
                    {
                        pawn.equipment.AddEquipment(weapon);
                    }
                }
            }
        }
    }

    private void AddItemsFromStorage(Pawn pawn, ThingOwner<ThingWithComps> storage)
    {
        while (storage.Any)
        {
            ThingWithComps thing = storage[0];
            storage.RemoveAt(0);

            if (thing.def.IsApparel)
            {
                pawn.apparel.Wear((Apparel)thing);
            }
            else if (thing.def.IsWeapon)
            {
                pawn.equipment.AddEquipment(thing);
            }
            else
            {
                pawn.inventory.innerContainer.TryAdd(thing);
            }
        }
    }

}
