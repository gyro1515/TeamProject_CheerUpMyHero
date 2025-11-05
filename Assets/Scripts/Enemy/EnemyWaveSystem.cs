using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class EnemyWave
{
    [SerializeField] public List<(PoolType poolType, float statMultiplier)> unitList = new List<(PoolType poolType, float statMultiplier)>();
}
public class EnemyWaveSystem : MonoBehaviour
{
    [field: Header("적 유닛 웨이브 정보 확인용")]
    [field: SerializeField] public List<EnemyWave> WaveData { get; private set; } = new List<EnemyWave>();
    [Header("적 유닛 웨이브 설정 세팅")]
    [SerializeField] float waveTime = 90f; // 웨이브 타임 -> 테스트로 20초
    [SerializeField] float warningBeforeWaveTime = 15f; // 경고 타임 -> 테스트로 웨이브 3초 전에 출력
    [SerializeField] float spawnWaveInterval = 0.5f; // 웨이브 마다 간격 달라질 수 있음, 현재는 통일
    WaitForSeconds waitForSpawnInterval;

    UIWaveWarning warningUI;
    EnemyHQ enemyHQ;
    float warningTime = -1f; // 경고 시간
    float timeUntilWave = -1f; // 경고 시간 후 소환까지 걸리는 시간
    int waveIdx = -1;
    const int maxWaveIdx = 5; // 최대 웨이브 인덱스(0~4)
    //public int WaveIdx { get { return waveIdx; } }

    public event Action OnWarningDisplayed; // 웨이브 경고가 화면에 표시될 때 발생하는 이벤트

    IEventPublisher<TimeSyncEvent> onTimeSyncEvent;
    IEventPublisher<StartWaveEvent> onStartWave;

    // 대기 시간
    private void Awake()
    {
        enemyHQ = GetComponent<EnemyHQ>();
        warningUI = UIManager.Instance.GetUI<UIWaveWarning>(); // 경고의 주체는 여기니까, 여기에 캐싱해 놓기 
        warningTime = waveTime - warningBeforeWaveTime;
        timeUntilWave = waveTime - warningTime;
        //TestWaveDateInit();
        SetWaveData();
        waitForSpawnInterval = new WaitForSeconds(spawnWaveInterval);
        onTimeSyncEvent = EventManager.GetPublisher<TimeSyncEvent>();
        onStartWave = EventManager.GetPublisher<StartWaveEvent>();
    }
    private void Start()
    {
        onTimeSyncEvent.Publish(new TimeSyncEvent { waveTime = waveTime, maxWaveCount = maxWaveIdx });
        // 웨이브 코루틴
        StartCoroutine(WaveTimeRoutine());
    }
    /*private void Update()
    {
        // 워닝 테스트
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            warningUI.OpenUI();
            OnWarningDisplayed?.Invoke();
        }
    }*/

    IEnumerator WaveTimeRoutine()
    {
        waveIdx = 0;

        while (waveIdx < maxWaveIdx) // 5번째 웨이브까지 실행하기(실제 7분 30)
        {
            // warningTime까지 대기
            yield return new WaitForSeconds(warningTime);

            // 경고 표시 (한 번만)
            warningUI.OpenUI();
            //OnWarningDisplayed?.Invoke();
            // timeUntilWave 동안 대기
            yield return new WaitForSeconds(timeUntilWave);

            // 기존 적 유닛 스폰 일시 정지
            enemyHQ.SetSpawnEnemyActive(false);
            // 웨이브 시작
            AudioManager.PlayRandomOneShotByCameraDistance(DataManager.AudioData.monsterWaveSE_oak, gameObject.transform, 0.5f);
            StartCoroutine(WaveRoutine(waveIdx++));
            Debug.Log($"{waveIdx}번째 웨이브 시작");
            onStartWave.Publish(new StartWaveEvent { waveIdx = waveIdx });
        }
    }

    public void SpawnDefenseWave()
    {
        if (WaveData.Count <= 2 ) return;

        AudioManager.PlayRandomOneShotByCameraDistance(DataManager.AudioData.monsterWaveSE_oak, gameObject.transform, 0.5f);
        GameObject fxGO = ObjectPoolManager.Instance.Get(PoolType.FXEnemyHQDefense);
        Vector3 fxSpawnPos = gameObject.transform.position;
        fxSpawnPos.y += 7.25f;
        fxGO.transform.position = fxSpawnPos;
        StartCoroutine(WaveRoutine(2));
        Debug.Log("체력 70퍼 이하라서 방어 웨이브 스폰함");
        // 모든 적에게 히트백
        List<BaseCharacter> unitList = UnitManager.PlayerUnitList;
        for (int i = 0; i < unitList.Count; i++)
        {
            if (unitList[i] is BaseUnit unit)
            {
                unit.StartHitBack();
            }
        }
    }

    IEnumerator WaveRoutine(int waveDataIdx)
    {
        // 데이터 없으면 바로 종료
        if (waveDataIdx >= WaveData.Count) yield break;
        // 캐싱하기
        List<(PoolType poolType, float statMultiplier)> unitList = WaveData[waveDataIdx].unitList;
        int unitCnt = unitList.Count;
        for (int i = 0; i < unitCnt; i++)
        {
            // 여기서 오브젝트 풀에서 가져오기
            GameObject enemyUnitGO = ObjectPoolManager.Instance.Get(unitList[i].poolType);
            enemyUnitGO.transform.position = enemyHQ.GetRandomSpawnPos();
            enemyUnitGO.GetComponent<EnemyUnit>().SetStatMultiplier(unitList[i].statMultiplier);
            yield return waitForSpawnInterval;
        }
        // 웨이브 끝나면 기존 유닛 스폰 루틴 다시 활성화
        enemyHQ.SetSpawnEnemyActive(true);
    }

    void SetWaveData()
    {
        // 빈 리스트로 시작
        WaveData.Clear();
        // 가져올 스테이지 정보 세팅
        (int selectedMainStageIdx, int selectedSubStageIdx ) = PlayerDataManager.Instance.SelectedStageIdx;
        // 웨이브 데이터SO 가져오기
        StageWaveSO waveSO = DataManager.Instance.StageWaveData.SO;
        List<StageWaveData> waveDataList = waveSO.GetStageWaveDataList(selectedMainStageIdx);
        int waveIdx = -1;
        // 데이터가 없다면 스테이지 1로 판단하기, 메인에서 시작하면 이럴 일 없음
        if (waveDataList == null) 
        { 
            selectedMainStageIdx = 0;
            selectedSubStageIdx = 0;
            waveDataList = waveSO.GetStageWaveDataList(selectedMainStageIdx);
            Debug.LogWarning("웨이브 정보 없어 -> 스테이지1 데이터로 세팅");
            PlayerDataManager.Instance.SelectedStageIdx = (selectedMainStageIdx, selectedSubStageIdx);
            //return; 나중에는 그냥 리턴하기
        }

        foreach (StageWaveData waveData in waveDataList)
        {
            // 선택한 스테이지가 아니라면 다음
            if (waveData.stage - 1 != selectedSubStageIdx) continue;
            // 웨이브Idx마다 WaveData.Add
            if(waveIdx < waveData.wave - 1)
            {
                waveIdx++;
                WaveData.Add(new EnemyWave());
            }
            // 해당 유닛 수만큼 wave.unitList에 추가
            for (int j = 0; j < waveData.unitCount; j++)
            {
                WaveData[waveIdx].unitList.Add((waveData.poolType, waveData.spawnProbability / (float)100));
            }
        }
    }
    public void SetOnWarningEnd(Action onWarningEndEvent)
    {
        warningUI.OnWarningEnd += onWarningEndEvent;
    }
    
    #region 테스트용
    // 데이터 테이블에 따라 아래 형식 사용할 수 있어서 일단 주석처리
    /*void TestWaveDateInit()
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
    }*/
    #endregion
}
#region 웨이브 이벤트
public struct StartWaveEvent
{
    public int waveIdx;
}
#endregion
