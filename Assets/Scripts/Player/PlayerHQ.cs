using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHQ : BaseHQ
{
    [Header("아군 본부 세팅")]
    [SerializeField] List<PoolType> playerUnits = new List<PoolType>();
    [SerializeField] float statMultiplier = 1.2f; // 아군 유닛 강화 배율
    [SerializeField] List<int> upgradeCntByRarity = new List<int>() { 8, 4 }; // 커먼, 레어, 에픽 순서로 몇 번 소환시 강화할지 
    // 미리 캐싱하고 사용하는 방식 => 업데이트 같은 곳에서 사용할 때 성능 향상
    EventChannel<SpawnHQEvent> onSpawn;
    // 해당 유닛을 몇 번 소환했는지 체크용
    Dictionary<PoolType, int> unitSpawnCnt = new Dictionary<PoolType, int>();
    // 강화 횟수 체크용
    const int upgradeCnt = 3;

    protected override void Awake()
    {
        base.Awake();

        //UIManager.Instance.GetUI<UIHpBarContainer>().AddHpBar(this, EUIHpBarType.PlayerUnit, new Vector2(300f, 16.5f));
        //UnitManager.Instance.AddUnitList(this, true);
        // 위와 다르게 아래는 바로 매니저를 호출한 이유는 다른 클래스의 start에서 GameManager.Instance.PlayerHQ를 사용하기 때문
        // 만약 이것도 이벤트로 바꾸면 start 실행 순서에 따라 null 참조가 발생할 수 있음
        GameManager.Instance.PlayerHQ = this;
        // 이벤트 채널 캐싱
        onSpawn = EventManager.GetPublisher<SpawnHQEvent>();
    }
    protected override void Start()
    {
        // EnemyHQ와 달리 PlayerHQ는 캐싱된 이벤트 채널로 발행
        SpawnHQEvent ev = new SpawnHQEvent();
        ev.baseHQ = this;
        ev.type = EUIHpBarType.PlayerUnit;
        ev.hpBarSize = new Vector2(300f, 16.5f);
        ev.isPlayer = true;
        onSpawn.Publish(ev);
        base.Start();
    }
    public override void Dead()
    {
        base.Dead();

        GameManager.Instance.ShowResultUI(false);
        Debug.Log("아군 HQ 파괴! 패배!");
    }
    /*protected override void SpawnUnit() // 현재 사용 안함
    {
        if (playerUnits.Count == 0) return;
        
        // 여기서 오브젝트 풀에서 가져오기
        GameObject playerUnitGO = ObjectPoolManager.Instance.Get(playerUnits[0]);
        playerUnitGO.transform.position = GetRandomSpawnPos();
        //playerUnitGO.transform.SetParent(gameObject.transform);
        //PlayerUnit playerUnit = playerUnitGO.GetComponent<PlayerUnit>();
    }*/
    public void SpawnUnit(PoolType poolType)
    {
        GameObject playerUnitGO = ObjectPoolManager.Instance.Get(poolType);
        playerUnitGO.transform.position = GetRandomSpawnPos();
        if(unitSpawnCnt.ContainsKey(poolType))
        {
            unitSpawnCnt[poolType]++;
            // 4번 소환할 때마다 강화
            // 251015 변경 -> 커먼 유닛은 8번, 레어는 4번
            Rarity unitRarity = Rarity.common;
            int tmpUpgradeCntByRarity = upgradeCntByRarity[(int)unitRarity];
            if (unitSpawnCnt[poolType] >= tmpUpgradeCntByRarity)
            {
                unitSpawnCnt[poolType] = 0;
                playerUnitGO.GetComponent<BaseUnit>().SetStatMultiplierByWave(statMultiplier);
            }
            else if(unitSpawnCnt[poolType] == tmpUpgradeCntByRarity - 1)
            {
                // 유닛 슬롯에 전설 유닛 소환 가능 알리기
            }
            return;
        }
        unitSpawnCnt[poolType] = 1;
    }
}
