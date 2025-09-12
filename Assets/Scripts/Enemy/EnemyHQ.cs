using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHQ : BaseHQ
{
    [Header("적 본부 세팅")]
    [SerializeField] List<PoolType> enemyUnits = new List<PoolType>();
    //[SerializeField] PoolType spawnableEnemyUnits; // 소환할 적 유닛 선택하기
    protected override void Awake()
    {
        base.Awake();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.enemyHQ = this;
        }
        UIManager.Instance.GetUI<UIHpBarContainer>().AddHpBar(this, EUIHpBarType.EnemyUnit, new Vector2(300f, 16.5f));
        UnitManager.Instance.AddUnitList(this, false);
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
}
