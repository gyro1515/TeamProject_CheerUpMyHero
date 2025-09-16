using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    [Header("적 유닛 웨이브 설정 세팅")]
    [SerializeField] float waveTime = 90f; // 웨이브 타임 -> 테스트로 20초
    [SerializeField] float warningBeforeWaveTime = 15f; // 경고 타임 -> 테스트로 웨이브 3초 전에 출력
    [SerializeField] float spawnWaveInterval = 0.5f; // 웨이브 마다 간격 달라질 수 있음, 현재는 통일

    UIWaveWarning warningUI;
    EnemyHQ enemyHQ;
    float warningTime = -1f; // 경고 시간
    float timeUntilWave = -1f; // 경고 시간 후 소환까지 걸리는 시간

    private void Awake()
    {
        enemyHQ = GetComponent<EnemyHQ>();
        warningUI = UIManager.Instance.GetUI<UIWaveWarning>(); // 경고의 주체는 여기니까, 여기에 캐싱해 놓기 
        warningTime = waveTime - warningBeforeWaveTime;
        timeUntilWave = waveTime - warningTime;
        TestWaveDateInit();
    }
    private void Start()
    {
        // 웨이브 코루틴
        StartCoroutine(WaveTimeRoutine());
    }
    IEnumerator WaveTimeRoutine()
    {
        int waveIdx = 0;

        while (waveIdx < 5) // 5번째 웨이브까지 실행하기(실제 7분 30)
        {
            // warningTime까지 대기
            yield return new WaitForSeconds(warningTime);

            // 경고 표시 (한 번만)
            warningUI.OpenUI();

            // timeUntilWave 동안 대기
            yield return new WaitForSeconds(timeUntilWave);

            // 기존 적 유닛 스폰 일시 정지
            enemyHQ.SetSpawnEnemyActive(false);
            // 웨이브 시작
            StartCoroutine(WaveRoutine(waveIdx++));
            Debug.Log($"{waveIdx}번째 웨이브 시작");
        }
    }
    IEnumerator WaveRoutine(int waveDataIdx)
    {
        // 데이터 없으면 바로 종료
        if (waveDataIdx >= WaveData.Count) yield break;
        // 캐싱하기
        WaitForSeconds wait = new WaitForSeconds(spawnWaveInterval);
        List<PoolType> unitList = WaveData[waveDataIdx].unitList;
        int unitCnt = unitList.Count;
        for (int i = 0; i < unitCnt; i++)
        {
            // 여기서 오브젝트 풀에서 가져오기
            GameObject enemyUnitGO = ObjectPoolManager.Instance.Get(unitList[i]);
            enemyUnitGO.transform.position = enemyHQ.GetRandomSpawnPos();
            yield return wait;
        }
        // 웨이브 끝나면 기존 유닛 스폰 루틴 다시 활성화
        enemyHQ.SetSpawnEnemyActive(true);
    }
    void TestWaveDateInit()
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
