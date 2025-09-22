using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class StageWaveData : MonoData
{
    public int stage;             // 몇 스테이지인가
    public int wave;              // 몇 번째 웨이브인가
    public PoolType poolType;     // 적 유닛 종류
    public int unitCount;         // 적 유닛 소환 수
    public int spawnProbability;  // 적 유닛 소환될 확률
}
