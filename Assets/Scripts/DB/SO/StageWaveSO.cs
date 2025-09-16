using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExcelAsset(AssetPath = "Resources/DB")]
public class StageWaveSO : MonoSO<StageWaveData>
{
	public List<StageWaveData> stageWaveData;
    public override List<StageWaveData> GetList()
    {
        return stageWaveData;
    }
    
}
