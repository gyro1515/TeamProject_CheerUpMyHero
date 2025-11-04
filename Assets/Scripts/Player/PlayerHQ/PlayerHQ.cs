using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;

public class PlayerHQ : BaseHQ
{
    [Header("아군 본부 세팅")]
    [SerializeField] List<PoolType> playerUnits = new List<PoolType>();
    [SerializeField] float statMultiplier = 1.2f; // 아군 유닛 강화 배율
    [SerializeField] List<int> upgradeCntByRarity = new List<int>() { 8, 4, -1 }; // 커먼, 레어, 에픽 순서로 몇 번 소환시 강화할지 
    // 미리 캐싱하고 사용하는 방식 => 업데이트 같은 곳에서 사용할 때 성능 향상
    IEventPublisher<SpawnHQEvent> onSpawn;
    // 해당 유닛을 몇 번 소환했는지 체크용 // 아싸 이거 통계용으로 써야지
    Dictionary<PoolType, int> unitSpawnCnt = new Dictionary<PoolType, int>();
    public Dictionary<PoolType, int> UnitSpawnCnt { get { return unitSpawnCnt; } }
    // 해당 유닛 타입 저장용
    Dictionary<PoolType, Rarity> uunitRarityType = new Dictionary<PoolType, Rarity>();
    // 강화 횟수 체크용
    const int upgradeCnt = 3;

    IEventSubscriber<HeroSpawnEvent> onHeroSpawnEventSub;

    bool isSpawnHero = false;
    public bool IsSpawnHero {  get { return isSpawnHero; } }
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
        SetUnitDataFromCardDatd();
        onHeroSpawnEventSub = EventManager.GetSubscriber<HeroSpawnEvent>();
        onHeroSpawnEventSub.Subscribe(SetIsSpawnHeroActive);
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
        // 아래는 테스트 코드
        /*GameObject hero = ObjectPoolManager.Instance.Get(PoolType.Hero_Unit1);
        hero.transform.position = GetRandomSpawnPos();*/
        /*for (int i = 0; i < 20; i++)
        {
            hero = ObjectPoolManager.Instance.Get(PoolType.Allies_Unit1);
            hero.transform.position = GetRandomSpawnPos();
        }*/
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        onHeroSpawnEventSub.Unsubscribe(SetIsSpawnHeroActive);
    }
    public override void Dead()
    {
        base.Dead();

        GameManager.Instance.ShowResultUI(false).Forget(); // await 일부러 뺀거에 컴파일 경고 안뜨드록 처리
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
    public void SpawnUnit(PoolType poolType, UISpawnUnitSlot uiSlot)
    {
        GameObject playerUnitGO = ObjectPoolManager.Instance.Get(poolType);
        playerUnitGO.transform.position = GetRandomSpawnPos();

        Rarity unitRarity = uunitRarityType[poolType];

        if (unitRarity == Rarity.epic) return; //251016기준: epic 등급은 강화 없음

        unitSpawnCnt[poolType]++;
        // 4번 소환할 때마다 강화
        // 251015 변경 -> 커먼 유닛은 8번, 레어는 4번
        int tmpUpgradeCntByRarity = upgradeCntByRarity[(int)unitRarity];
        bool isLegendary = false;

        // [수정됨] 카운트가 0보다 크고, 강화 주기(N)의 배수일 때 강화
        if (unitSpawnCnt[poolType] > 0 && unitSpawnCnt[poolType] % tmpUpgradeCntByRarity == 0)
        {
            //unitSpawnCnt[poolType] = 0;
            isLegendary = true;
        }

        else if (unitSpawnCnt[poolType] % tmpUpgradeCntByRarity == tmpUpgradeCntByRarity - 1)
        {
            // 유닛 슬롯에 전설 유닛 소환 가능 알리기
            uiSlot.SetOutLineForSpawnLegendaryUnit();
            //Debug.Log("다음 소환시 전설 유닛 소환");
        }
        playerUnitGO.GetComponent<BaseUnit>().SetStatMultiplier(isLegendary ? statMultiplier : 1f, isSpawnHero);

    }
    void SetUnitDataFromCardDatd()
    {
        int activeDeckIndex = PlayerDataManager.Instance.ActiveDeckIndex;
        //List<int> deckUnitIds = PlayerDataManager.Instance.DeckPresets[activeDeckIndex].UnitIds;
        List<BaseUnitData> deckBaseUnitDatas = PlayerDataManager.Instance.DeckPresets[activeDeckIndex].BaseUnitDatas;
        for (int i = 0; i < deckBaseUnitDatas.Count; i++)
            {
            /*int unitId = deckUnitIds[i];
            if (unitId == -1) { Debug.LogWarning("세팅 오류"); continue; }*/
            //TempCardData cardData = PlayerDataManager.Instance.GetUnitData(unitId);
            BaseUnitData cardData = deckBaseUnitDatas[i];
            if (cardData == null) { /*Debug.LogWarning("세팅 오류");*/ continue; } // 빈 슬롯은 패스
            uunitRarityType[cardData.poolType] = cardData.rarity;
            unitSpawnCnt[cardData.poolType] = 0;
        }
    }
    void SetIsSpawnHeroActive(HeroSpawnEvent heroSpawnEvent)
    {
        isSpawnHero = true;
    }
}
