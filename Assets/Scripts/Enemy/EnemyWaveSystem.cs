using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyWave
{
    [SerializeField] public List<PoolType> unitList = new List<PoolType>();
}
public class EnemyWaveSystem : MonoBehaviour
{
    [field: Header("적 유닛 웨이브 정보 확인용")]
    [field: SerializeField] public List<EnemyWave> WaveData { get; private set; } = new List<EnemyWave>();

    public event Action OnWaveFinish; // 웨이브 끝나면 다시 기본 유닛 스폰될 수 있도록

    private void Awake()
    {
        TestwaveDateInit();
    }
    void TestwaveDateInit()
    {
        WaveData.Clear();
        EnemyWave wave1 = new EnemyWave();
        int totalMonCnt = 8;
        int monBundleCnt = 2;
        int monBundle = totalMonCnt / monBundleCnt;
        for (int i = 0; i < totalMonCnt; i++)
        {
            if(i % monBundle == 3)
            {
                wave1.unitList.Add(PoolType.EnemyUnit3);
                continue;
            }
            wave1.unitList.Add(PoolType.EnemyUnit2);
        }
        WaveData.Add(wave1);
        EnemyWave wave2 = new EnemyWave();
        totalMonCnt = 16;
        monBundleCnt = 2;
        monBundle = totalMonCnt / monBundleCnt;
        for (int i = 0; i < totalMonCnt; i++)
        {
            if (i % monBundle == 5 || i % monBundle == 6)
            {
                wave2.unitList.Add(PoolType.EnemyUnit3);
                continue;
            }
            else if (i % monBundle == 7)
            {
                wave2.unitList.Add(PoolType.EnemyUnit4);
                continue;
            }
            wave2.unitList.Add(PoolType.EnemyUnit2);
        }
        WaveData.Add(wave2);
        EnemyWave wave3 = new EnemyWave();
        totalMonCnt = 32;
        monBundleCnt = 2;
        monBundle = totalMonCnt / monBundleCnt;
        for (int i = 0; i < totalMonCnt; i++)
        {
            if (i % monBundle >= 10 && i % monBundle < 13)
            {
                wave3.unitList.Add(PoolType.EnemyUnit3);
                continue;
            }
            else if (i % monBundle >= 13 && i % monBundle < 15)
            {
                wave3.unitList.Add(PoolType.EnemyUnit4);
                continue;
            }
            else if (i % monBundle >= 15 && i % monBundle < monBundle)
            {
                wave3.unitList.Add(PoolType.EnemyUnit5);
                continue;
            }
            wave3.unitList.Add(PoolType.EnemyUnit2);
        }
        WaveData.Add(wave3);
        EnemyWave wave4 = new EnemyWave();
        totalMonCnt = 64;
        monBundleCnt = 4;
        monBundle = totalMonCnt / monBundleCnt;
        for (int i = 0; i < totalMonCnt; i++)
        {
            if (i % monBundle >= 10 && i % monBundle < 13)
            {
                wave4.unitList.Add(PoolType.EnemyUnit3);
                continue;
            }
            else if (i % monBundle >= 13 && i % monBundle < 15)
            {
                wave4.unitList.Add(PoolType.EnemyUnit4);
                continue;
            }
            else if (i % monBundle >= 15 && i % monBundle < monBundle)
            {
                wave4.unitList.Add(PoolType.EnemyUnit5);
                continue;
            }
            wave4.unitList.Add(PoolType.EnemyUnit2);
        }
        WaveData.Add(wave4);
        EnemyWave wave5 = new EnemyWave();
        totalMonCnt = 128;
        monBundleCnt = 8;
        monBundle = totalMonCnt / monBundleCnt;
        for (int i = 0; i < totalMonCnt; i++)
        {
            if (i % monBundle >= 10 && i % monBundle < 13)
            {
                wave5.unitList.Add(PoolType.EnemyUnit3);
                continue;
            }
            else if (i % monBundle >= 13 && i % monBundle < 15)
            {
                wave5.unitList.Add(PoolType.EnemyUnit4);
                continue;
            }
            else if (i % monBundle >= 15 && i % monBundle < monBundle)
            {
                wave5.unitList.Add(PoolType.EnemyUnit5);
                continue;
            }
            wave5.unitList.Add(PoolType.EnemyUnit2);
        }
        WaveData.Add(wave5);
    }
}
