using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHQ : BaseHQ
{
    [Header("아군 본부 세팅")]
    [SerializeField] List<PoolType> playerUnits = new List<PoolType>();

    protected override void Awake()
    {
        base.Awake();
        UIManager.Instance.GetUI<UIHpBarContainer>().AddHpBar(this, EUIHpBarType.PlayerUnit, new Vector2(300f, 16.5f));
        UnitManager.Instance.AddUnitList(this, true);
        GameManager.Instance.PlayerHQ = this;
    }
    protected override void SpawnUnit() // 현재 사용 안함
    {
        if (playerUnits.Count == 0) return;
        
        // 여기서 오브젝트 풀에서 가져오기
        GameObject playerUnitGO = ObjectPoolManager.Instance.Get(playerUnits[0]);
        playerUnitGO.transform.position = GetRandomSpawnPos();
        //playerUnitGO.transform.SetParent(gameObject.transform);
        //PlayerUnit playerUnit = playerUnitGO.GetComponent<PlayerUnit>();
    }
    public void SpawnUnit(PoolType poolType)
    {
        GameObject playerUnitGO = ObjectPoolManager.Instance.Get(poolType);
        playerUnitGO.transform.position = GetRandomSpawnPos();
    }
}
