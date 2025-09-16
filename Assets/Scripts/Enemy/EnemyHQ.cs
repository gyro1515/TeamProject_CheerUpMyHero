using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHQ : BaseHQ
{
    [Header("적 본부 세팅")]
    [SerializeField] float waveTime = 90f; // 웨이브 타임 -> 테스트로 15초
    [SerializeField] float warningBeforeWaveTime = 15f; // 경고 타임 -> 테스트로 웨이브 3초 전에 출력
    [SerializeField] List<PoolType> enemyUnits = new List<PoolType>();
    
    float warningTime = -1f; // 경고 시간
    float timeUntilWave = -1f; // 경고 시간 후 소환까지 걸리는 시간
    UIWaveWarning WarningUI { get; set; } // 일단 프로퍼티로
    EnemyWaveSystem waveSystem;
    Coroutine spawnUnitRoutine; // 추후 웨이브시 스폰은 일시 정지 할 수 도 있어서

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
        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            WarningUI.OpenUI();
        }
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
        //enemyUnitGO.transform.SetParent(gameObject.transform);
        //EnemyUnit enemyUnit = enemyUnitGO.GetComponent<EnemyUnit>();
        //UnitManager.Instance.EnemyUnitList.Add(enemyUnit);
    }
    IEnumerator SpawnUnitRoutine()
    {
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

        while (waveIdx++ != 5) // 5번째 웨이브까지 실행하기(실제 7분 30초, 현재 40초)
        {
            // warningTime까지 대기
            yield return new WaitForSeconds(warningTime);

            // 경고 표시 (한 번만)
            WarningUI.OpenUI();

            // timeUntilWave 동안 대기
            yield return new WaitForSeconds(timeUntilWave);

            // 웨이브 시작
            Debug.Log($"{waveIdx}번째 웨이브 시작");
        }
    }
}
