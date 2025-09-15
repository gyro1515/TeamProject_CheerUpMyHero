using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHQ : BaseHQ
{
    [Header("적 본부 세팅")]
    [SerializeField] float waveTime = 90f; // 웨이브 타임 -> 테스트로 15초
    [SerializeField] float warningBeforeWaveTime = 15f; // 경고 타임 -> 테스트로 웨이브 3초 전에 출력
    [SerializeField] List<PoolType> enemyUnits = new List<PoolType>();
    
    float warningTime = -1f;
    float timeUntilWave = -1f;
    UIWaveWarning WarningUI { get; set; } // 일단 프로퍼티로

    protected override void Awake()
    {
        base.Awake();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.enemyHQ = this;
        }
        UIManager.Instance.GetUI<UIHpBarContainer>().AddHpBar(this, EUIHpBarType.EnemyUnit, new Vector2(300f, 16.5f));
        UnitManager.Instance.AddUnitList(this, false);
        InvokeRepeating("SpawnUnit", 0f, spawnInterval);
        WarningUI = UIManager.Instance.GetUI<UIWaveWarning>(); // 경고의 주체는 적 기지니까, 적 기지에 캐싱해 놓기 
        warningTime = waveTime - warningBeforeWaveTime;
        timeUntilWave = waveTime - warningTime;
    }
    protected override void Start()
    {
        base.Start();
        StartCoroutine(WaveTimeRoutine());
    }
    protected override void Update()
    {
        base.Update();
        // 워닝 테스트
        if(Input.GetKeyDown(KeyCode.Alpha2))
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
    IEnumerator WaveTimeRoutine()
    {
        int waveIdx = 0;

        while (true)
        {
            // warningTime까지 대기
            yield return new WaitForSeconds(warningTime);

            // 경고 표시 (한 번만)
            WarningUI.OpenUI();

            // timeUntilWave 동안 대기
            yield return new WaitForSeconds(timeUntilWave);

            // 웨이브 시작
            Debug.Log($"{++waveIdx}번째 웨이브 시작");
        }
    }
}
