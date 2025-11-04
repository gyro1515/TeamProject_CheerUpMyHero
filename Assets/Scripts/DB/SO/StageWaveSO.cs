using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExcelAsset(AssetPath = "Resources/DB")]
public class StageWaveSO : MonoSO<StageWaveData>
{
	public List<StageWaveData> stageWaveData1;
	public List<StageWaveData> stageWaveData2;
	public List<StageWaveData> stageWaveData3;
    public override List<StageWaveData> GetList()
    {
        return stageWaveData1;
    }

    public List<StageWaveData> GetStageWaveDataList(int mainStageIdx)
    {
        switch (mainStageIdx)
        {
            case 0:
                return stageWaveData1;
            case 1:
                return stageWaveData2;
            case 2:
                return stageWaveData3;
            default:
                Debug.LogWarning("해당 스테이지의 웨이브 데이터가 존재하지 않습니다.");
                break;
        }
        return null;
    }

    public override void SetData(Dictionary<int, StageWaveData> DB)
    {
        for (int i = 0; i < stageWaveData1.Count; i++)
        {
            var data = stageWaveData1[i];             
            if (data == null) continue;

            DB[data.idNumber] = data;                
        }

        for (int i = 0; i < stageWaveData2.Count; i++)
        {
            var data = stageWaveData2[i];
            if (data == null) continue;

            DB[data.idNumber] = data;
        }
        for (int i = 0; i < stageWaveData3.Count; i++)
        {
            var data = stageWaveData3[i];
            if (data == null) continue;

            DB[data.idNumber] = data;
        }
    }
}
