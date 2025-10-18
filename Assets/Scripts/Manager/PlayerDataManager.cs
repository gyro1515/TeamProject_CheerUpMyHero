using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public enum ResourceType
{
    Gold,
    Wood,
    Iron,
    Food,
    MagicStone,
    Bm,
    Ticket
}
public enum TileStatus { Normal, Damaged, Repairing }

[System.Serializable]
public class DeckData
{
    public string DeckName;
    public List<int> UnitIds;

    public DeckData(string defaultName)
    {
        DeckName = defaultName;
        // 8개의 빈 슬롯(-1)으로 초기화
        UnitIds = new List<int>(new int[8]);
        for (int i = 0; i < 8; i++)
        {
            UnitIds[i] = -1;
        }
    }
}
public class PlayerDataManager : SingletonMono<PlayerDataManager>
{
    // 선택한 스테이지 선택용
    public (int mainStageIdx, int subStageIdx) SelectedStageIdx { get; set; } = (-1, -1);
    public TileDataHandler _TileDataHandler { get; private set; }

    //테스트용 카드 데이터(유닛 테이블로 교체될 예정
    public Dictionary<int, TempCardData> cardDic;

    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
        {
            _TileDataHandler = new TileDataHandler();

            InitializeResources();
            LoadDecks();
            TestCardGenerate();
        }
    }
    private void OnEnable()
    {
        EventManager.Subscribe<GridStateChangedEvent>(OnGridStateChanged);
        EventManager.Subscribe<BattleEndedEvent>(OnBattleEnded);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<GridStateChangedEvent>(OnGridStateChanged);
        EventManager.Unsubscribe<BattleEndedEvent>(OnBattleEnded);

    }
    private void OnGridStateChanged(GridStateChangedEvent e)
    {
        UpdateAllBuildingEffects();
    }
    private void OnBattleEnded(BattleEndedEvent e)
    {
        Debug.Log($"전투 종료 감지! (승리: {e.IsVictory})");
        _TileDataHandler.AdvanceRepairTurn();
        if (!e.IsVictory)
        {
            _TileDataHandler.DamageRandomTile();
        }

    }
    //테스트용 카드 생성
    void TestCardGenerate()
    {
        cardDic = new() 
        {
            {100001, new TempCardData(100001, "징집병", UnitType.Dealer, 250f, 75, 20f, 6f, PoolType.Allies_Unit1)},
            {100002, new TempCardData(100002, "방패병", UnitType.Tanker, 1000f, 150, 5f, 6f, PoolType.Allies_Unit2)},
            {100003, new TempCardData(100003, "도끼병", UnitType.Dealer, 500f, 300, 62f, 6f, PoolType.Allies_Unit3)},
            {100004, new TempCardData(100004, "궁수", UnitType.Dealer, 1000f, 600, 250f, 6.6f, PoolType.Allies_Unit4)},
            {100005, new TempCardData(100005, "기마병", UnitType.Dealer, 1250f, 750, 32f, 6f, PoolType.Allies_Unit5)},
            {100006, new TempCardData(100006, "견습 마법사", UnitType.Dealer, 750f, 975, 350f, 6f, PoolType.Allies_Unit6)},
            {100007, new TempCardData(100007, "중갑 보병", UnitType.Tanker, 1750f, 1200, 450f, 12.6f, PoolType.Allies_Unit7)},
            {100008, new TempCardData(100008, "궁병", UnitType.Dealer, 2000f, 1500, 875f, 30.6f, PoolType.Allies_Unit8)},
            {100009, new TempCardData(100009, "수도승", UnitType.Healer, 1000f, 900, 250f, 6.6f, PoolType.Allies_Unit11)},
            {100010, new TempCardData(100010, "왕국 기마병", UnitType.Dealer, 1750f, 2400, 1050f, 58.6f, PoolType.Allies_Unit10)},
            {105001, new TempCardData(105001, "왕국 근위대장", UnitType.Tanker, 2500f, 1950, 700f, 54.6f, PoolType.Allies_Unit9)},
            {105002, new TempCardData(105002, "애쉬", UnitType.Dealer, 2250f, 2250, 2750f, 59.6f, PoolType.Allies_Unit12)},
            {105003, new TempCardData(105003, "사냥꾼", UnitType.Dealer, 1250f, 825, 87f, 21.6f, PoolType.Allies_Unit13)},
            {105004, new TempCardData(105004, "검투사", UnitType.Tanker, 1750f, 1035, 250f, 17.6f, PoolType.Allies_Unit14)},
            {105005, new TempCardData(105005, "광전사", UnitType.Tanker, 1750f, 1440, 330f, 17.6f, PoolType.Allies_Unit15)},
            {105006, new TempCardData(105006, "황국 기마병", UnitType.Dealer, 2000f, 1125, 750f, 19.6f, PoolType.Allies_Unit16)},
            {105007, new TempCardData(105007, "견습 사제", UnitType.Healer, 1000f, 525, 250f, 6.6f, PoolType.Allies_Unit17)},
            {105008, new TempCardData(105008, "큰 도끼 광전사", UnitType.Dealer, 1500f, 1490, 1000f, 17.3f, PoolType.Allies_Unit18)},
            {105009, new TempCardData(105009, "마법사", UnitType.Dealer, 1000f, 1050, 450f, 6f, PoolType.Allies_Unit19)},
            {105010, new TempCardData(105010, "자경단원", UnitType.Dealer, 600f, 300, 35f, 6f, PoolType.Allies_Unit20)},
        };
    }

    public TempCardData GetUnitData(int cardId)
    {
        if (cardDic.TryGetValue(cardId, out TempCardData data))
        {
            return data;
        }
        // 만약 cardDic에 해당 ID가 없으면 null을 반환
        Debug.LogWarning($"Card ID {cardId}에 해당하는 임시 데이터를 찾을 수 없습니다.");
        return null;
    }

    //빌딩 데이터
    #region Building
    //public void DamageRandomTile() => _TileHandler.DamageRandomTile();
    //public void AdvanceRepairTurn() => _TileHandler.AdvanceRepairTurn();
    // 건설 가능한 건물 목록을 저장해 둘 리스트 (한 번만 생성)
    private List<BuildingUpgradeData> _buildableList;

    // 건설 가능한 모든 건물의 목록을 반환하는 함수
    public List<BuildingUpgradeData> GetBuildableList()
    {
        if (_buildableList == null)
        {
            _buildableList = new List<BuildingUpgradeData>();

            // ❗️ DataManager.Instance를 통해 건물 데이터베이스에 접근하도록 수정합니다.
            foreach (var data in DataManager.Instance.BuildingUpgradeData.Values)
            {
                // 0레벨인 데이터(최초 건설 데이터)만 목록에 추가
                if (data.level == 0)
                {
                    _buildableList.Add(data);
                }
            }
        }
        return _buildableList;
    }
    #endregion

    //덱 편성 관련
    #region Deck
    // Dictionary<덱 번호, 유닛 ID 리스트> 형태로 5개의 덱을 관리합니다.
    public Dictionary<int, DeckData> DeckPresets { get; private set; } = new Dictionary<int, DeckData>();

    public int ActiveDeckIndex { get; set; } = 1;

    private void LoadDecks()
    {
        for (int i = 1; i <= 5; i++)
        {
            if (!DeckPresets.ContainsKey(i))
            {
                DeckPresets[i] = new DeckData("덱 " + i); // 기본 이름 "덱 1", "덱 2"...
            }
        }
        Debug.Log("덱 프리셋 5개를 초기화했습니다.");
    }


    // 현재 덱 구성을 딕셔너리에 업데이트합니다.
    public void UpdateDeck(int deckIndex, List<int> unitIds)
    {
        if (DeckPresets.ContainsKey(deckIndex))
        {
            DeckPresets[deckIndex].UnitIds = new List<int>(unitIds);
        }
    }

    // 게임 종료나 특정 시점에 덱 정보를 저장할 때 사용합니다.
    public void SaveDecks()
    {
        Debug.Log("현재 덱 구성을 파일에 저장합니다.");
    }
    #endregion


    //자원 관련
    #region Resources
    //
    // 특정 자원의 수량 변경을 알리는 이벤트
    public event Action<ResourceType, int> OnResourceChangedEvent;

    // 각 자원 타입과 수량을 저장할 딕셔너리
    private Dictionary<ResourceType, int> _resources = new();

    private void InitializeResources()
    {
        // 5가지 자원을 모두 딕셔너리에 추가하고 초기 수량을 설정.
        _resources[ResourceType.Gold] = 10000;
        _resources[ResourceType.Wood] = 10000;
        _resources[ResourceType.Iron] = 10000;
        _resources[ResourceType.Food] = CurrentFood;
        _resources[ResourceType.MagicStone] = 100;
        _resources[ResourceType.Bm] = 0; 
        _resources[ResourceType.Ticket] = 0;
    }

    // 특정 자원의 현재 수량을 반환하는 메서드
    public int GetResourceAmount(ResourceType type)
    {
        if (_resources.TryGetValue(type, out int amount))
        {
            return amount;
        }
        Debug.LogWarning($"ResourceManager: 존재하지 않는 자원 타입입니다. ({type})");
        return -1;
    }

    // 특정 자원의 수량을 변경하는 메서드
    public void AddResource(ResourceType type, int amount)
    {
        if (_resources.ContainsKey(type))
        {
            _resources[type] += amount;

            if (type == ResourceType.Food)
            {
                CurrentFood = _resources[type];
            }

            OnResourceChangedEvent?.Invoke(type, _resources[type]);
        }
        else
        {
            Debug.LogWarning($"ResourceManager: 존재하지 않는 자원 타입입니다. ({type})");
        }
    }

    public (int gold, int wood, int iron, int magicStone) ApplyDefeatPenalties()
    {
        var resourcePenalties = ApplyResourcePenalty();

        return resourcePenalties;
    }

    public (int gold, int wood, int iron, int magicStone) ApplyResourcePenalty()
    {
        int goldPenalty = Mathf.CeilToInt(GetResourceAmount(ResourceType.Gold) * 0.05f); AddResource(ResourceType.Gold, -goldPenalty);
        int woodPenalty = Mathf.CeilToInt(GetResourceAmount(ResourceType.Wood) * 0.05f); AddResource(ResourceType.Wood, -woodPenalty);
        int ironPenalty = Mathf.CeilToInt(GetResourceAmount(ResourceType.Iron) * 0.05f); AddResource(ResourceType.Iron, -ironPenalty);
        int magicStonePenalty = Mathf.CeilToInt(GetResourceAmount(ResourceType.MagicStone) * 0.05f); AddResource(ResourceType.MagicStone, -magicStonePenalty);
        Debug.Log($"패배 페널티: 골드 -{goldPenalty}, 목재 -{woodPenalty}, 철 -{ironPenalty}, 마력석 -{magicStonePenalty}");
        return (goldPenalty, woodPenalty, ironPenalty, magicStonePenalty);
    }
    #endregion

    #region Food
    //식량에 관련된 변수와 함수
    public int CurrentFood { get; private set; } = 0;
    public int MaxFood { get; private set; } = 20000;
    private int _calculatedMaxFood = 20000;
    public int CalculatedMaxFood { get { return _calculatedMaxFood; } }
    private float foodAccumulator = 0f;
    public int SupplyLevel { get; private set; } = 1;
    private float currentFarmGainPercent = 0f;

    //private readonly int[] maxFoodByFarmLevel = { 500, 750, 1000, 1250, 1500, 1750, 2000, 2250, 2500 };
    //private readonly int[] farmFoodGainPercentByLevel = { 5, 10, 15, 20, 25, 30, 35, 40, 50 };
    private readonly int[] baseFoodGainBySupplyLevel = { 25, 29, 37, 47, 59, 75, 95, 119, 147 };
    private readonly int[] supplyUpgradeCosts = { 100, 220, 450, 900, 1800, 3500, 5500, 8000 };

    public void UpgradeSupplyLevel()
    {
        if (SupplyLevel >= baseFoodGainBySupplyLevel.Length)
        {
            Debug.Log("최대 레벨입니다.");
            return;
        }
        int requiredFood = supplyUpgradeCosts[SupplyLevel - 1];
        if (CurrentFood >= requiredFood && MaxFood >= requiredFood)
        {
            CurrentFood -= requiredFood;
            MaxFood -= requiredFood;

            _resources[ResourceType.Food] = CurrentFood;
            OnResourceChangedEvent?.Invoke(ResourceType.Food, CurrentFood);

            SupplyLevel++;
            Debug.Log($"Supply Level Up! 현재 SupplyLevel: {SupplyLevel}");
        }
        else
        {
            Debug.Log($"보급품 또는 최대 보급품이 부족하여 레벨업할 수 없습니다. 필요량: {requiredFood}");
        }
    }

    public void AddFoodOverTime(float deltaTime)
    {
        if (MaxFood <= 0) return;

        int baseGain = baseFoodGainBySupplyLevel[SupplyLevel - 1];
        float gainThisFrame = baseGain * (1f + currentFarmGainPercent / 100f) * deltaTime;
        foodAccumulator += gainThisFrame;

        int gainInt = Mathf.FloorToInt(foodAccumulator);

        if (gainInt > 0)
        {
            if (gainInt > MaxFood)
            {
                gainInt = MaxFood;
            }

            CurrentFood += gainInt;
            MaxFood -= gainInt;

            _resources[ResourceType.Food] = CurrentFood;
            OnResourceChangedEvent?.Invoke(ResourceType.Food, CurrentFood);
            foodAccumulator -= gainInt;
        }
    }

    public void ResetFood()
    {
        CurrentFood = 0;
        foodAccumulator = 0f;
        MaxFood = _calculatedMaxFood;
        SupplyLevel = 1;
        _resources[ResourceType.Food] = CurrentFood;
        OnResourceChangedEvent?.Invoke(ResourceType.Food, CurrentFood);
        Debug.Log("현재 식량을 0으로, 최대 식량을 원래 값으로 초기화했습니다.");
    }

    public bool TryGetUpgradeCost(out int cost)
    {
        cost = 0;
        if (SupplyLevel >= baseFoodGainBySupplyLevel.Length) return false;
        cost = supplyUpgradeCosts[SupplyLevel - 1];
        return true;
    }
    #endregion

    //건물 효과를 종합적으로 관리하는 영역
    #region Building Effects 
    public float TotalUnitCooldownReduction { get; private set; } = 0f;
    public int RareUnitSlots { get; private set; } = 0;
    public int EpicUnitSlots { get; private set; } = 0;
    // 모든 건물의 효과를 한 번에 합산하여 계산하는 범용 함수
    public void UpdateAllBuildingEffects()
    {
        _TileDataHandler.CalculateTotalBuildingEffects(
            out int bonusMaxFood,
            out float foodGainPercent,
            out float cooldownReduction,
            out int rareSlots,
            out int epicSlots
        );

        _calculatedMaxFood = 20000 + bonusMaxFood;
        currentFarmGainPercent = foodGainPercent;
        if (MaxFood > _calculatedMaxFood)
        {
            MaxFood = _calculatedMaxFood;
        }

        TotalUnitCooldownReduction = cooldownReduction;
        RareUnitSlots = rareSlots;
        EpicUnitSlots = epicSlots;

        OnResourceChangedEvent?.Invoke(ResourceType.Food, CurrentFood);
        Debug.Log($"모든 건물 효과 계산 완료: 최대 식량={_calculatedMaxFood}, 식량 보너스={currentFarmGainPercent}%, 유닛 쿨감={TotalUnitCooldownReduction}%, 레어 슬롯={RareUnitSlots}, 에픽 슬롯={EpicUnitSlots}");
    }
    #endregion

    // 스테이지 클리어 기록
    #region Clear Stage



    #endregion

    // 현재 운명 + 현재 도전 기능 데이터 + Set 메서드, Clear 메서드
    #region Destiny + Challenge
    // 지금 선택된 운명 기능
    public StageDestinyData currentDastiny { get; private set; }

    // 운명 설정 메서드 : 운명은 하나만 설정됨
    public void SetDestiny(StageDestinyData destiny)
    {
        currentDastiny = destiny;
    }

    // 운명 비우는 기능 -> 매 스테이지마다 
    public void ClearDestiny()
    {
        currentDastiny = null;
    }

    // 지금 선택된 도전 기능
    public Dictionary<int, int> activeChallenges { get; private set; } = new Dictionary<int, int>();

    // 도전 기능 설정 메서드
    public void SetChallenges(int id, int lv)
    {
        if (lv > 0)
        {
            activeChallenges[id] = lv;
        }
        else
        {
            activeChallenges.Remove(id);
        }
    }

    // 도전 기능 비우는 기능
    public void ClearChallenge()
    {
        activeChallenges.Clear();
    }
    #endregion

}



