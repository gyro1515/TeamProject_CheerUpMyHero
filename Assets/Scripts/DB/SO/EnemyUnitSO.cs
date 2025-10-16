using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExcelAsset(AssetPath = "Resources/DB")]
public class EnemyUnitSO : MonoSO<BaseUnitData>
{
	public List<BaseUnitData> enemyCommon; 
	public List<BaseUnitData> enemyRare; 
	public List<BaseUnitData> enemyEpic; 

    public override List<BaseUnitData> GetList()
    {
        throw new NotImplementedException();
    }

    public override void SetData(Dictionary<int, BaseUnitData> DB)
    {
        for (int i = 0; i < enemyCommon.Count; i++)
        {
            var data = enemyCommon[i];
            if (data == null) continue;

            DB[(int)data.poolType] = data;
        }

        for (int i = 0; i < enemyRare.Count; i++)
        {
            var data = enemyRare[i];
            if (data == null) continue;

            DB[(int)data.poolType] = data;
        }
        for (int i = 0; i < enemyEpic.Count; i++)
        {
            var data = enemyEpic[i];
            if (data == null) continue;

            DB[(int)data.poolType] = data;
        }
    }
}
