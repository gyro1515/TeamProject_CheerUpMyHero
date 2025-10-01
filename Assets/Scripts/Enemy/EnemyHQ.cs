using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyHQ : BaseHQ
{
    [Header("적 본부 세팅")]
    [SerializeField] List<PoolType> enemyUnits = new List<PoolType>(); // 기본 스폰 유닛

    public Coroutine spawnUnitRoutine; // 웨이브시 스폰은 일시 정지용

    public EnemyWaveSystem WaveSystem { get; private set; }
    private bool isDefenseWaveSpawned = false;
    // 적 유닛 스폰 쿨타임 실행용
    Dictionary<PoolType, bool> enemyUnitCanSpawn = new Dictionary<PoolType, bool>();
    Dictionary<PoolType, float> enemyUnitCoolTimes = new Dictionary<PoolType, float>();

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

        WaveSystem = GetComponent<EnemyWaveSystem>();
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
        
        // CurHp 오버라이드로 바꿀 듯 합니다***********************
        if (!isDefenseWaveSpawned && CurHp / MaxHp <= 0.7f)
        {
            isDefenseWaveSpawned = true;
            WaveSystem.SpawnDefenseWave();
        }
        // 테스트 키
        if(Input.GetKeyDown(KeyCode.Alpha4))
        {
            WaveSystem.SpawnDefenseWave();
        }
    }
    public override void Dead()
    {
        base.Dead();

        GameManager.Instance.ShowResultUI(true);
        GameManager.Instance.ClearStage();
        Debug.Log("적군 HQ 파괴! 승리!");
    }
    bool SpawnUnit() // 소환했으면 리턴 트루
    {
        if (enemyUnits.Count == 0) return false;
        //Debug.Log($"적 유닛 스폰 가능 수{enemyUnits.Count}");
        // 여기서 오브젝트 풀에서 가져오기
        for (int i = 0; i < enemyUnits.Count; i++)
        {
            //Debug.Log(enemyUnits[i] + " 스폰 시도");
            // 처음 추가하는 거라면 바로 스폰
            if (!enemyUnitCoolTimes.ContainsKey(enemyUnits[i]))
            {
                GameObject enemyUnitGO = ObjectPoolManager.Instance.Get(enemyUnits[i]);
                enemyUnitGO.transform.position = GetRandomSpawnPos();
                float cooltime = enemyUnitGO.GetComponent<BaseUnit>().SpawnCooldown;
                enemyUnitCoolTimes[enemyUnits[i]] = cooltime;
                StartCoroutine(EnemyCoolTimeRoutin(enemyUnits[i], cooltime));
                return true;
            }
            else // 처음이 아니라면 쿨타임 확인 후 스폰
            {
                if (!enemyUnitCanSpawn[enemyUnits[i]]) continue; // 스폰 못하면 다음

                GameObject enemyUnitGO = ObjectPoolManager.Instance.Get(enemyUnits[i]);
                enemyUnitGO.transform.position = GetRandomSpawnPos();
                StartCoroutine(EnemyCoolTimeRoutin(enemyUnits[i], enemyUnitCoolTimes[enemyUnits[i]]));
                return true;
            }
        }
        // 쿨타임 때문에 못 스폰했으면 false 리턴
        return false;
    }
    IEnumerator SpawnUnitRoutine()
    {
        yield return new WaitForSeconds(0.2f); // 잠깐 유예시간 주기

        WaitForSeconds wait = new WaitForSeconds(spawnInterval);
        while (true)
        {
            // 소환이 됐다면 스폰 인터벌만큼 대기
            if(SpawnUnit()) yield return wait;
            else yield return null; // 못했다면 다음 프레임에 다시 시도
        }

    }
    IEnumerator EnemyCoolTimeRoutin(PoolType type, float coolTime)
    {
        //Debug.Log(type + " 쿨타임 시작");
        enemyUnitCanSpawn[type] = false;
        yield return new WaitForSeconds(coolTime);
        enemyUnitCanSpawn[type] = true;
        //Debug.Log(type + " 쿨타임 끝");

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
