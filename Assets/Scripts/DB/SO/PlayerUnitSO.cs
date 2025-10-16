using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExcelAsset(AssetPath = "Resources/DB")]
public class PlayerUnitSO : MonoSO<BaseUnitData>
{
	public List<BaseUnitData> hero_unit; 
	public List<BaseUnitData> hiller_unit; 
	public List<BaseUnitData> allianceCommon; 
	public List<BaseUnitData> allianceRare;
	public List<BaseUnitData> allianceEpic;

    public override List<BaseUnitData> GetList()
    {
        throw new NotImplementedException();
    }

    public override void SetData(Dictionary<int, BaseUnitData> DB)
    {
        for (int i = 0; i < hero_unit.Count; i++)
        {
            var data = hero_unit[i];
            if (data == null) continue;

            DB[(int)data.poolType] = data;
        }
        // 힐러는 일단 제외
        /*for (int i = 0; i < hiller_unit.Count; i++)
        {
            var data = hiller_unit[i];
            if (data == null) continue;

            DB[(int)data.poolType] = data;
        }*/
        for (int i = 0; i < allianceCommon.Count; i++)
        {
            var data = allianceCommon[i];
            if (data == null) continue;

            DB[(int)data.poolType] = data;
        }
        for (int i = 0; i < allianceRare.Count; i++)
        {
            var data = allianceRare[i];
            if (data == null) continue;

            DB[(int)data.poolType] = data;
        }
        for (int i = 0; i < allianceEpic.Count; i++)
        {
            var data = allianceEpic[i];
            if (data == null) continue;

            DB[(int)data.poolType] = data;
        }
    }
}
