using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyHQ : BaseHQ
{
    [Header("적 본부 세팅")]
    [SerializeField] List<PoolType> enemyUnits = new List<PoolType>(); // 기본 스폰 유닛

    private const string POOLPATH = "Prefabs/ObjPooling/";

    public Coroutine spawnUnitRoutine; // 웨이브시 스폰은 일시 정지용

    public EnemyWaveSystem WaveSystem { get; private set; }
    private bool isDefenseWaveSpawned = false;
    // 적 유닛 스폰 쿨타임 실행용
    Dictionary<PoolType, bool> enemyUnitCanSpawn = new Dictionary<PoolType, bool>();
    Dictionary<PoolType, float> enemyUnitCoolTimes = new Dictionary<PoolType, float>();

    public override float CurHp
    {
        get => base.CurHp; 
        set
        {
            base.CurHp = value;
            if (curHp != 0 && !isDefenseWaveSpawned && curHp / MaxHp <= 0.7f)
            {
                isDefenseWaveSpawned = true;
                WaveSystem.SpawnDefenseWave();
            }
        }
    }
    protected override void Awake()
    {
        base.Awake();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.enemyHQ = this;
        }
        // HQ 체력바가 제일 위에 표시 될 수 있도록 UI로 표현
        //UIManager.Instance.GetUI<UIHpBarContainer>().AddHpBar(this, EUIHpBarType.EnemyUnit, new Vector2(300f, 16.5f));
        // 적 유닛 리스트에 추가
        //UnitManager.Instance.AddUnitList(this, false);

        //InvokeRepeating("SpawnUnit", 0f, spawnInterval);

        WaveSystem = GetComponent<EnemyWaveSystem>();
        SetUnitCoolTime();
    }
    protected override void Start()
    {
        // 이벤트 발행 => OnAction?.Invoke() 방식과 동일
        EventManager.GetPublisher<SpawnHQEvent>().Publish(new SpawnHQEvent { baseHQ = this, type = EUIHpBarType.EnemyUnit, hpBarSize = new Vector2(300f, 16.5f), isPlayer = false });
        base.Start();

        // 계속해서 유닛을 스폰하도록
        SetSpawnEnemyActive(true);
        // 아래는 테스트 코드
        /*GameObject hero = ObjectPoolManager.Instance.Get(PoolType.EnemyUnit10);
        hero.transform.position = GetRandomSpawnPos();
        for (int i = 0; i < 30; i++)
        {
            hero = ObjectPoolManager.Instance.Get(PoolType.EnemyUnit1);
            hero.transform.position = GetRandomSpawnPos();
        }*/
    }
    protected override void Update()
    {
        base.Update();
#if UNITY_EDITOR
        // 테스트 키
        /*if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            WaveSystem.SpawnDefenseWave();
        }*/
#endif
    }
    public override void Dead()
    {
        base.Dead();

        if(GameManager.IsTutorialCompleted)
        {
            GameManager.Instance.OpenSelectArtifactUI();
        }
        else
        {
            GameManager.Instance.ShowResultUI(true).Forget(); // await 일부러 뺀거에 컴파일 경고 안뜨드록 처리
        }
        GameManager.Instance.ClearStage().Forget(); //// await 일부러 뺀거에 컴파일 경고 안뜨드록 처리
        Debug.Log("적군 HQ 파괴! 승리!");
    }
    void SetUnitCoolTime()
    {
        for (int i = 0; i < enemyUnits.Count; i++)
        {
            BaseUnitData data = DataManager.EnemyUnitData.GetData((int)enemyUnits[i]);
            float cooltime = data.spawnCooldown * (1f + Modifiercalculator.GetMultiplier(EffectTarget.EnemyUnit, StatType.SpawnCooldown, data));

            enemyUnitCoolTimes[enemyUnits[i]] = cooltime;
            //Debug.Log($"{enemyUnits[i]} 쿨타임 {cooltime}초로 세팅");
            StartCoroutine(EnemyCoolTimeRoutin(enemyUnits[i], cooltime));
        }
    }
    bool SpawnUnit() // 소환했으면 리턴 트루
    {
        if (enemyUnits.Count == 0) return false;
        //Debug.Log($"적 유닛 스폰 가능 수{enemyUnits.Count}");
        // 여기서 오브젝트 풀에서 가져오기
        for (int i = 0; i < enemyUnits.Count; i++)
        {
            //Debug.Log(enemyUnits[i] + " 스폰 시도");
            // 251014 변경 -> 처음 추가하는 거라도 쿨타임 적용, Awake에서 미리 세팅
            if (!enemyUnitCanSpawn[enemyUnits[i]]) continue; // 스폰 못하면 다음

            GameObject enemyUnitGO = ObjectPoolManager.Instance.Get(enemyUnits[i]);
            enemyUnitGO.transform.position = GetRandomSpawnPos();
            StartCoroutine(EnemyCoolTimeRoutin(enemyUnits[i], enemyUnitCoolTimes[enemyUnits[i]]));
            return true;
        }
        // 쿨타임 때문에 못 스폰했으면 false 리턴
        return false;
    }
    IEnumerator SpawnUnitRoutine()
    {
        yield return new WaitForSeconds(0.1f); // 잠깐 유예시간 주기

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
