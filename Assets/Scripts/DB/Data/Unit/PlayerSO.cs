using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExcelAsset(AssetPath = "Resources/DB")]
public class PlayerSO : MonoSO<PlayerData>
{
	public List<PlayerData> Player;

    public override List<PlayerData> GetList()
    {
        throw new NotImplementedException();
    }

    public override void SetData(Dictionary<int, PlayerData> DB)
    {
        for (int i = 0; i < Player.Count; i++)
        {
            var data = Player[i];
            if (data == null) continue;

            DB[(int)data.level] = data;
        }
    }
}
