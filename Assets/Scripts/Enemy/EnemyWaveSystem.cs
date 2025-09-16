using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyWave
{
    List<PoolType> meleeUnitList = new List<PoolType>();
    List<PoolType> rangedUnitList = new List<PoolType>();
}
public class EnemyWaveSystem : MonoBehaviour
{
    [Header("적 유닛 웨이브 정보 설정")]
    [SerializeField] List<EnemyWave> waveData = new List<EnemyWave> ();

    public event Action OnWaveFinish; // 웨이브 끝나면 다시 기본 유닛 스폰될 수 있도록

}
