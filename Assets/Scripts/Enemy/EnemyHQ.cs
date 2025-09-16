using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHQ : BaseHQ
{
    [Header("적 본부 세팅")]
    [SerializeField] float waveTime = 90f; // 웨이브 타임 -> 테스트로 20초
    [SerializeField] float warningBeforeWaveTime = 15f; // 경고 타임 -> 테스트로 웨이브 3초 전에 출력
    [SerializeField] List<PoolType> enemyUnits = new List<PoolType>();
    [SerializeField] float spawnWaveInterval = 0.2f;

    float warningTime = -1f; // 경고 시간
    float timeUntilWave = -1f; // 경고 시간 후 소환까지 걸리는 시간
    UIWaveWarning WarningUI { get; set; } // 일단 프로퍼티로
    EnemyWaveSystem waveSystem;
    Coroutine spawnUnitRoutine; // 웨이브시 스폰은 일시 정지
    protected override void Awake()
    {
        base.Awake();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.enemyHQ = this;
        }
        // HQ 체력바가 제일 위에 표시 될 수 있도록 UI로 표현
        UIManager.Instance.GetUI<UIHpBarContainer>().AddHpBar(this, EUIHpBarType.EnemyUnit, new Vector2(300f, 16.5f));
        // 적 유닛 리스트에 추가
        UnitManager.Instance.AddUnitList(this, false);
        
        //InvokeRepeating("SpawnUnit", 0f, spawnInterval);

        WarningUI = UIManager.Instance.GetUI<UIWaveWarning>(); // 경고의 주체는 적 기지니까, 적 기지에 캐싱해 놓기 
        warningTime = waveTime - warningBeforeWaveTime;
        timeUntilWave = waveTime - warningTime;

        waveSystem = GetComponent<EnemyWaveSystem>();
    }
    protected override void Start()
    {
        base.Start();
        // 계속해서 유닛을 스폰하도록
        spawnUnitRoutine = StartCoroutine(SpawnUnitRoutine());
        // 웨이브 코루틴
        StartCoroutine(WaveTimeRoutine()); 
    }
    protected override void Update()
    {
        base.Update();
        // 워닝 테스트
        /*if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            WarningUI.OpenUI();
        }*/
    }
    public override void Dead()
    {
        base.Dead();
    }
    protected override void SpawnUnit()
    {
        if (enemyUnits.Count == 0) return;
        Vector3 spawnPos = gameObject.transform.position;
        spawnPos.y += UnityEngine.Random.Range(tmpMinY, tmpMaxY) / 100f;
        // 여기서 오브젝트 풀에서 가져오기
        GameObject enemyUnitGO = ObjectPoolManager.Instance.Get(enemyUnits[0]);
        enemyUnitGO.transform.position = spawnPos;
    }
    IEnumerator SpawnUnitRoutine()
    {
        yield return new WaitForSeconds(0.5f); // 잠깐 유예시간 주기

        WaitForSeconds wait = new WaitForSeconds(spawnInterval);
        while (true)
        {
            SpawnUnit();
            yield return wait;
        }

    }
    IEnumerator WaveTimeRoutine()
    {
        int waveIdx = 0;

        while (waveIdx < 5) // 5번째 웨이브까지 실행하기(실제 7분 30초, 현재 40초)
        {
            // warningTime까지 대기
            yield return new WaitForSeconds(warningTime);

            // 경고 표시 (한 번만)
            WarningUI.OpenUI();

            // timeUntilWave 동안 대기
            yield return new WaitForSeconds(timeUntilWave);

            // 기존 적 유닛 스폰 일시 정지
            StopCoroutine(spawnUnitRoutine);
            // 웨이브 시작
            StartCoroutine(WaveRoutine(waveIdx++));
            Debug.Log($"{waveIdx}번째 웨이브 시작");
        }
    }
    IEnumerator WaveRoutine(int waveDataIdx)
    {
        // 데이터 없으면 바로 종료
        if (waveDataIdx >= waveSystem.WaveData.Count) yield break;

        // 캐싱하기
        WaitForSeconds wait = new WaitForSeconds(spawnWaveInterval);
        List<PoolType> unitList = waveSystem.WaveData[waveDataIdx].unitList;
        int unitCnt = unitList.Count;
        for (int i = 0; i < unitCnt; i++)
        {
            Vector3 spawnPos = gameObject.transform.position;
            spawnPos.y += UnityEngine.Random.Range(tmpMinY, tmpMaxY) / 100f;
            // 여기서 오브젝트 풀에서 가져오기
            GameObject enemyUnitGO = ObjectPoolManager.Instance.Get(unitList[i]);
            enemyUnitGO.transform.position = spawnPos;
            yield return wait;
        }
        // 웨이브 끝나면 기존 유닛 스폰 루틴 다시 활성화
        spawnUnitRoutine = StartCoroutine(SpawnUnitRoutine());
    }
}
