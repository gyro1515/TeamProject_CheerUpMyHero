using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExcelAsset(AssetPath = "Resources/DB")]
public class SynergyEffectSO : MonoSO<SynergyData>
{
	public List<SynergyData> Sheet1;

    public override List<SynergyData> GetList()
    {
        throw new NotImplementedException();
    }

    public override void SetData(Dictionary<int, SynergyData> DB)
    {
        for (int i = 0; i < Sheet1.Count; i++)
        {
            var data = Sheet1[i];
            if (data == null) continue;

            // key는 시너지 타입과 등급을 조합하여 생성
            // key = synergyType * 1000 + synergyGrade
            DB[(int)data.synergyType * 1000 + (int)data.synergyGrade] = data;
        }
    }
}
