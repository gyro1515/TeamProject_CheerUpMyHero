using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExcelAsset(AssetPath = "Resources/DB")]
public class ItemSO : MonoSO<ItemData>
{
	public List<ArmorData> Armor; // Replace 'EntityType' to an actual type that is serializable.
	public List<WeaponData> Weapon; // Replace 'EntityType' to an actual type that is serializable.

    public override List<ItemData> GetList()
    {
        return null;
    }

    public override void SetData(Dictionary<int, ItemData> DB)
    {
        for (int i = 0; i < Armor.Count; i++)
        {
            var data = Armor[i];
            if (data == null) continue;
            
            DB[data.idNumber] = data;
        }

        for (int i = 0; i < Weapon.Count; i++)
        {
            var data = Weapon[i];
            if (data == null) continue;

            DB[data.idNumber] = data;
        }
    }
}