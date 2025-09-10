using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHQ : BaseHQ
{
    [Header("적 본부 세팅")]
    [SerializeField] List<GameObject> enemyUnitPrefabs = new List<GameObject>();
    protected override void Awake()
    {
        base.Awake();
        UIManager.Instance.GetUI<UIHpBarContainer>().AddHpBar(this, EUIHpBarType.EnemyUnit, new Vector2(300f, 16.5f));
        UnitManager.Instance.EnemyUnitList.Add(this);

    }

    protected override void SpawnUnit()
    {
        if (enemyUnitPrefabs[0] == null) return;
        Vector3 spawnPos = gameObject.transform.position;
        spawnPos.y += UnityEngine.Random.Range(tmpMinY, tmpMaxY) / 100f;
        GameObject enemyUnitGO = Instantiate(enemyUnitPrefabs[0], spawnPos, Quaternion.identity);
        enemyUnitGO.transform.SetParent(gameObject.transform);
        EnemyUnit enemyUnit = enemyUnitGO.GetComponent<EnemyUnit>();
        enemyUnit?.InitCharacter();
        UnitManager.Instance.EnemyUnitList.Add(enemyUnit);
    }
}
