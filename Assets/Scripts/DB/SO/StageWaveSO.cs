using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExcelAsset(AssetPath = "Resources/DB")]
public class StageWaveSO : MonoSO<StageWaveData>
{
	public List<StageWaveData> stageWaveData;
	public List<StageWaveData> stageWaveData2;
    public override List<StageWaveData> GetList()
    {
        return stageWaveData;
    }

    public List<StageWaveData> GetStageWaveDataList(int mainStageIdx)
    {
        switch (mainStageIdx)
        {
            case 0:
                return stageWaveData;
            case 1:
                return stageWaveData2;
            default:
                Debug.LogWarning("해당 스테이지의 웨이브 데이터가 존재하지 않습니다.");
                break;
        }
        return null;
    }

    public override void SetData(Dictionary<int, StageWaveData> DB)
    {
        for (int i = 0; i < stageWaveData.Count; i++)
        {
            var data = stageWaveData[i];             
            if (data == null) continue;

            DB[data.idNumber] = data;                
        }

        for (int i = 0; i < stageWaveData2.Count; i++)
        {
            var data = stageWaveData2[i];
            if (data == null) continue;

            DB[data.idNumber] = data;
        }
    }
}
