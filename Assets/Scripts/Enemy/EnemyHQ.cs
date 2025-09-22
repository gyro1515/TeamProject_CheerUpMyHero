using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHQ : BaseHQ
{
    [Header("적 본부 세팅")]
    [SerializeField] List<PoolType> enemyUnits = new List<PoolType>(); // 기본 스폰 유닛

    public Coroutine spawnUnitRoutine; // 웨이브시 스폰은 일시 정지용

    private EnemyWaveSystem waveSystem;
    private bool isDefenseWaveSpawned = false;
    
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

        waveSystem = GetComponent<EnemyWaveSystem>();
    }
    protected override void Start()
    {
        base.Start();
        // 계속해서 유닛을 스폰하도록
        SetSpawnEnemyActive(true);
    }
    protected override void Update()
    {
        base.Update();
        
        if (!isDefenseWaveSpawned && CurHp / MaxHp <= 0.7f)
        {
            isDefenseWaveSpawned = true;
            waveSystem.SpawnDefenseWave();
        }
    }
    public override void Dead()
    {
        base.Dead();
    }
    protected override void SpawnUnit()
    {
        if (enemyUnits.Count == 0) return;
        // 여기서 오브젝트 풀에서 가져오기
        GameObject enemyUnitGO = ObjectPoolManager.Instance.Get(enemyUnits[0]);
        enemyUnitGO.transform.position = GetRandomSpawnPos();
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
    public void SetSpawnEnemyActive(bool active)
    {
        // 활성화 시, 혹시라도 이미 실행 중인 게 있다면 리턴
        // (웨이브 끝나기 전에 웨이브 실행시 이렇게 됨)
        if (active && spawnUnitRoutine != null) return;

        if(active) spawnUnitRoutine = StartCoroutine(SpawnUnitRoutine());
        else if(spawnUnitRoutine != null)
        {
            StopCoroutine(spawnUnitRoutine);
            spawnUnitRoutine = null;
        }
    }
}
