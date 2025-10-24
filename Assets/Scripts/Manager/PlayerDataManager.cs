using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;
using Random = UnityEngine.Random;

public struct SynergyDataUpdatedEvent { }
public struct ClearedStagesUpdatedEvent { }
public struct LimitedPityCountUpdatedEvent
{
    public int NewCount;
}

public struct StandardPityCountUpdatedEvent
{
    public int NewCount;
}
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
    public List<int> UnitIds { get; private set; }
    public List<BaseUnitData> BaseUnitDatas { get; private set; }
    // 251023: UnitIds을 쓰게 되면 계속 Dictionary에서 BaseUnitData가져와야 해서, baseUnitData 자체를 저장할 거 같습니다.
    // 기껏 데이터 저장했는데 또 다른 곳에서 데이터를 불러오면 너무 비효율 적이라고 생각합니다.
    public DeckData(string defaultName)
    {
        DeckName = defaultName;
        // 8개의 빈 슬롯(-1)으로 초기화
        UnitIds = new List<int>(new int[8]);
        for (int i = 0; i < 8; i++)
        {
            UnitIds[i] = -1;
        }
        BaseUnitDatas = new List<BaseUnitData>(new BaseUnitData[8]);
    }
}
public class PlayerDataManager : SingletonMono<PlayerDataManager>
{
    // 선택한 스테이지 선택용
    public (int mainStageIdx, int subStageIdx) SelectedStageIdx { get; set; } = (-1, -1);

    private Dictionary<(int, int), bool> clearedStages = new Dictionary<(int, int), bool>();
    IEventPublisher<ClearedStagesUpdatedEvent> _clearedStagesEvent;
    public TileDataHandler _TileDataHandler { get; private set; }

    //테스트용 카드 데이터(유닛 테이블로 교체될 예정
    //public Dictionary<int, TempCardData> cardDic;

    //모든 유닛 데이터
    private Dictionary<int, BaseUnitData> AllCardData = new Dictionary<int, BaseUnitData>();
    //해금된 유닛 데이터
    public Dictionary<int, BaseUnitData> OwnedCardData { get; private set; } = new Dictionary<int, BaseUnitData>();
    IEventSubscriber<GridStateChangedEvent> onGridStateChangedEvent;
    IEventSubscriber<BattleEndedEvent> onBattleEndedEvent;
    #region 시너지 보너스
    //모든 시너지 효과를 합산하여 저장할 프로퍼티들
    public float SynergyUnitCooldownReduction { get; private set; }
    public float SynergyFoodProductionBonus { get; private set; }
    public float SynergyAllUnitAttackBonus { get; private set; }
    public float SynergyAllUnitHealthBonus { get; private set; }
    public float SynergyWoodCostReduction { get; private set; }
    public float SynergyIronCostReduction { get; private set; }
    public float SynergyMagicStoneCostReduction { get; private set; }
    public float SynergyMaxFoodBonus { get; private set; }
    public float SynergyUnitAttackCooldownReduction { get; private set; }
    public float SynergyBlockBonusPercent { get; private set; } // 전문 기술 단지

    private Dictionary<(int x, int y), float> _tileEfficiencyBonuses;
    public IReadOnlyDictionary<(int x, int y), float> TileEfficiencyBonuses => _tileEfficiencyBonuses;
    public List<DetectedSynergy> ActiveSynergies { get; private set; }

    private IEventPublisher<SynergyDataUpdatedEvent> _synergyDataUpdatedPublisher;
    #endregion
    public int LimitedGachaPityCount { get; private set; } = 0;   // 1페이지 (한정/이벤트) 뽑기 횟수
    public int StandardGachaPityCount { get; private set; } = 0;  // 2페이지 (상시) 뽑기 횟수

    public const int LIMITED_GACHA_PITY_LIMIT = 150;
    public const int STANDARD_GACHA_PITY_LIMIT = 150;

    private IEventPublisher<LimitedPityCountUpdatedEvent> _limitedPityPublisher;
    private IEventPublisher<StandardPityCountUpdatedEvent> _standardPityPublisher;
    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
        {
            _TileDataHandler = new TileDataHandler();
            _tileEfficiencyBonuses = new Dictionary<(int, int), float>();
            _synergyDataUpdatedPublisher = EventManager.GetPublisher<SynergyDataUpdatedEvent>();
            _limitedPityPublisher = EventManager.GetPublisher<LimitedPityCountUpdatedEvent>();
            _standardPityPublisher = EventManager.GetPublisher<StandardPityCountUpdatedEvent>();
            _clearedStagesEvent = EventManager.GetPublisher<ClearedStagesUpdatedEvent>();
            LoadDecks();
            // TODO: 추후 아래 테스트 카드 생성 부분 제거 필요
            //테스트용 카드 생성*********
            List<BaseUnitData> unitList = DataManager.PlayerUnitData.SO.allianceCommon;
            for (int i = 0; i < unitList.Count; i++)
            {
                OwnedCardData[unitList[i].idNumber] = unitList[i];
            }
            // **************************

            onGridStateChangedEvent = EventManager.GetSubscriber<GridStateChangedEvent>();
            onBattleEndedEvent = EventManager.GetSubscriber<BattleEndedEvent>();
        }
    }
    private void OnEnable()
    {
        onGridStateChangedEvent.Subscribe(OnGridStateChanged);
        onBattleEndedEvent.Subscribe(OnBattleEnded);
    }

    private void OnDisable()
    {
        onGridStateChangedEvent.Unsubscribe(OnGridStateChanged);
        onBattleEndedEvent.Unsubscribe(OnBattleEnded);
    }
    private void OnGridStateChanged(GridStateChangedEvent e)
    {
        UpdateAllSynergyEffects();
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


    // 251023: 유닛 데이터는 데이터 매니저에서 바로 가져오도록 변경
    /*public BaseUnitData GetUnitData(int cardId)
    {
        if (OwnedCardData.TryGetValue(cardId, out BaseUnitData data))
        {
            return data;
        }
        // 만약 cardDic에 해당 ID가 없으면 null을 반환
        Debug.LogWarning($"Card ID {cardId}에 해당하는 임시 데이터를 찾을 수 없습니다.");
        return null;
    }*/
    //영지 시너지
    #region 시너지 로직

    public void UpdateAllSynergyEffects()
    {
        //모든 보너스 값을 0으로 초기화
        ResetSynergyBonuses();
        ActiveSynergies = _TileDataHandler.DetectAllSynergies();
        //TileDataHandler에게 시너지 분석을 요청
        List<DetectedSynergy> activeSynergies = _TileDataHandler.DetectAllSynergies();
        if (activeSynergies.Count > 0)
        {
            Debug.Log($"[시너지] {activeSynergies.Count}개의 시너지 감지!");

            var synergyLog = new System.Text.StringBuilder();
            synergyLog.AppendLine("--- 활성화된 시너지 목록 ---");

            foreach (var synergy in activeSynergies)
            {
                // 각 시너지의 타일 좌표를 (x,y) 형태의 문자열로 변환
                string positions = string.Join(", ", synergy.TilePositions.Select(p => $"({p.x},{p.y})"));
                synergyLog.AppendLine($"-> 종류: {synergy.Type}, 위치: [{positions}]");
            }

            Debug.Log(synergyLog.ToString());
        }

        // 분석 결과를 바탕으로 보너스 값 합산
        foreach (var synergy in activeSynergies)
        {
            ApplySynergyEffect(synergy);
        }

        // 시너지 계산 후 건물 효과를 다시 계산해야 시너지 보너스가 반영됨
        UpdateAllBuildingEffects();
        _synergyDataUpdatedPublisher.Publish();
    }

    private void ResetSynergyBonuses()
    {
        SynergyUnitCooldownReduction = 0f;
        SynergyFoodProductionBonus = 0f;
        SynergyAllUnitAttackBonus = 0f;
        SynergyAllUnitHealthBonus = 0f;
        SynergyWoodCostReduction = 0f;
        SynergyIronCostReduction = 0f;
        SynergyMagicStoneCostReduction = 0f;
        SynergyMaxFoodBonus = 0f;
        SynergyUnitAttackCooldownReduction = 0f;
        SynergyBlockBonusPercent = 0f;
        _tileEfficiencyBonuses.Clear();
        ActiveSynergies?.Clear();
    }

    private void ApplySynergyEffect(DetectedSynergy synergy)
    {
        switch (synergy.Type)
        {
            // 인접 시너지
            case BuildingSynergyType.Farm_Barracks:
                SynergyUnitCooldownReduction += 2.5f;
                SynergyFoodProductionBonus -= 2.5f;
                break;
            case BuildingSynergyType.Barracks_Mine:
                SynergyAllUnitAttackBonus += 1.5f;
                break;
            case BuildingSynergyType.Barracks_LumberMill:
                SynergyAllUnitHealthBonus += 1.5f;
                break;
            case BuildingSynergyType.Mine_LumberMill:
                foreach (var pos in synergy.TilePositions)
                {
                    _tileEfficiencyBonuses.TryAdd(pos, 0);
                    _tileEfficiencyBonuses[pos] += 2.5f;
                }
                break;

            case BuildingSynergyType.Farm_Mine:
            case BuildingSynergyType.Farm_LumberMill:
                foreach (var pos in synergy.TilePositions)
                {
                    var building = _TileDataHandler.BuildingGridData[pos.x, pos.y];
                    if (building != null && building.buildingType == BuildingType.Farm)
                    {
                        _tileEfficiencyBonuses.TryAdd(pos, 0);
                        _tileEfficiencyBonuses[pos] += 2.5f;
                    }
                }
                break;

            // 라인 시너지
            case BuildingSynergyType.Farm_Line:
                SynergyMaxFoodBonus += 5f;
                SynergyFoodProductionBonus += 2.5f;
                break;
            case BuildingSynergyType.LumberMill_Line:
                SynergyWoodCostReduction += 5f;
                break;
            case BuildingSynergyType.Mine_Line:
                SynergyIronCostReduction += 5f;
                SynergyMagicStoneCostReduction += 2.5f;
                break;
            case BuildingSynergyType.Barracks_Line:
                SynergyUnitAttackCooldownReduction += 10f;
                break;

            //블록 시너지
            case BuildingSynergyType.Specialized_Block:
                SynergyBlockBonusPercent += 10f;
                break;
            case BuildingSynergyType.Balanced_Block:
                foreach (var pos in synergy.TilePositions)
                {
                    _tileEfficiencyBonuses.TryAdd(pos, 0);
                    _tileEfficiencyBonuses[pos] += 5f; // 효율 5% 증가
                }
                break;
        }

    }
    #endregion
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
    //건물 비용 합산
    public List<Cost> CalculateTotalInvestedCost(BuildingUpgradeData currentBuildingData)
    {
        var totalCostMap = new Dictionary<ResourceType, int>();

        BuildingUpgradeData level1Data = DataManager.Instance.BuildingUpgradeData.Values
            .FirstOrDefault(data => data.buildingType == currentBuildingData.buildingType && data.level == 1);

        if (level1Data == null) return new List<Cost>();

        BuildingUpgradeData buildData = DataManager.Instance.BuildingUpgradeData.Values
            .FirstOrDefault(data => data.nextLevel == level1Data.idNumber);

        if (buildData != null)
        {
            foreach (var cost in buildData.costs)
            {
                totalCostMap[cost.resourceType] = totalCostMap.GetValueOrDefault(cost.resourceType, 0) + cost.amount;
            }
        }

        BuildingUpgradeData current = level1Data;
        while (current != null && current.level < currentBuildingData.level)
        {
            foreach (var cost in current.costs)
            {
                totalCostMap[cost.resourceType] = totalCostMap.GetValueOrDefault(cost.resourceType, 0) + cost.amount;
            }

            if (current.nextLevel > 0)
            {
                current = DataManager.Instance.BuildingUpgradeData.GetData(current.nextLevel);
            }
            else
            {
                break;
            }
        }

        return totalCostMap.Select(pair => new Cost { resourceType = pair.Key, amount = pair.Value }).ToList();
    }

    public void DestroyBuildingAt(int x, int y)
    {
        var buildingData = _TileDataHandler.BuildingGridData[x, y];
        if (buildingData == null) return;

        List<Cost> totalCost = CalculateTotalInvestedCost(buildingData);
        foreach (var cost in totalCost)
        {
            int refundAmount = Mathf.FloorToInt(cost.amount * 0.5f);
            AddResource(cost.resourceType, refundAmount);
        }

        _TileDataHandler.BuildingGridData[x, y] = null;
        _TileDataHandler.CooldownEndTimeGrid[x, y] = DateTime.MinValue;

        Debug.Log($"({x},{y}) 위치의 {buildingData.buildingName} 파괴 완료 및 자원 환급.");
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
    // 251023: 안쓰는 거 같아 일단 주석처리합니다.
    /*public void UpdateDeck(int deckIndex, List<int> unitIds)
    {
        if (DeckPresets.ContainsKey(deckIndex))
        {
            DeckPresets[deckIndex].UnitIds = new List<int>(unitIds);
        }
    }*/

    // 게임 종료나 특정 시점에 덱 정보를 저장할 때 사용합니다.
    public void SaveDecks()
    {
        Debug.Log("현재 덱 구성을 파일에 저장합니다.");
    }

    void CardGenerate(List<int> unlockedCardIDLists)
    {

        //0. 모든 유닛의 딕셔너리 만들기

        List<BaseUnitData> commonList = DataManager.PlayerUnitData.SO.allianceCommon;
        List<BaseUnitData> rareList = DataManager.PlayerUnitData.SO.allianceRare;
        List<BaseUnitData> epicList = DataManager.PlayerUnitData.SO.allianceEpic;

        List<List<BaseUnitData>> unitListList = new() { commonList, rareList, epicList };

        foreach (List<BaseUnitData> list in unitListList)
        {
            for (int i = 0; i < list.Count; i++)
            {
                AllCardData[list[i].idNumber] = list[i];
            }
        }

        //1. 이 중에서 id int list 기반으로 해금된 카드 딕셔너리 만들기
        foreach (int id in unlockedCardIDLists)
        {
            OwnedCardData[id] = AllCardData[id];
        }

    }

    public void UnLockUnit(int id)
    {
        if (!AllCardData.ContainsKey(id))
        {
            Debug.LogWarning($"유닛 해금 실패, ID:{id} 에 해당하는 유닛이 존재하지 않거나 세팅되지 않았습니다.");
            return;
        }

        OwnedCardData[id] = AllCardData[id];
    }
    #endregion


    //자원 관련
    #region Resources
    //
    // 특정 자원의 수량 변경을 알리는 이벤트
    public event Action<ResourceType, int> OnResourceChangedEvent;

    // 각 자원 타입과 수량을 저장할 딕셔너리
    private Dictionary<ResourceType, int> _resources = new();

    //비동기로 시작씬에서 호출.
    public async UniTask InitializeResourcesAsync()
    {
        // 5가지 자원을 모두 딕셔너리에 추가하고 초기 수량을 설정.
        //_resources[ResourceType.Gold] = 10000;
        //_resources[ResourceType.Wood] = 10000;
        //_resources[ResourceType.Iron] = 10000;
        //_resources[ResourceType.Food] = CurrentFood;
        //_resources[ResourceType.MagicStone] = 10000;
        //_resources[ResourceType.Bm] = 0; 
        //_resources[ResourceType.Ticket] = 0;

        Dictionary<ResourceType, int> serverData = await BackendManager.LoadEconomyData();

        if (serverData == null)
        {
            Debug.LogError("인터넷 확인");
        }
        else
        {
            foreach (ResourceType resource in serverData.Keys)
            {
                Debug.Log(resource);
                _resources[resource] = serverData[resource];
            }
            Debug.Log("재화 불러오기 완료");
        }
#if UNITY_EDITOR //테스트 코드
        Debug.LogWarning("[테스트] 게임 시작 시 스테이지 (1, 3) 강제 클리어 처리.");
        List<(int main, int sub)> fakeServerResponse = new List<(int main, int sub)> { (1, 3) };

        UpdateClearedStagesFromServer(fakeServerResponse);

        if (1 == 1 && 3 == 3)
        {
            Debug.Log("<color=green>[테스트 보상]</color> 스테이지 1-3 최초 클리어 테스트 보상: 티켓 10개 지급!");
            AddResource(ResourceType.Ticket, 10);
        }

#endif
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
    // 아 이거 비동기로 바꿔야 하는데 그러면 다른 것도 계속 바꿔야 하네
    public async void AddResource(ResourceType type, int amount)
    {
        Debug.Log($"<color=yellow>[PlayerData AddResource]</color> '{type}' 자원 {amount} 변경 요청 받음.");

        if (_resources.ContainsKey(type))
        {
            int previousAmount = _resources[type];
            _resources[type] += amount;
            int currentAmount = _resources[type];

            Debug.Log($"[PlayerData AddResource] '{type}' 값 변경: {previousAmount} -> {currentAmount}");

            Debug.Log($"[PlayerData AddResource] '{type}' 값 변경: {previousAmount} -> {currentAmount}");

            if (type == ResourceType.Food)
            {
                CurrentFood = _resources[type];
            }

            OnResourceChangedEvent?.Invoke(type, _resources[type]);

            //음식은 서버에 저장되지 않음
            if (type == ResourceType.Food)
            {
                return;
            }

            await BackendManager.ChangeEconomy(BackendManager.EconomyEnumToId(type), amount);
            await SaveDataToCloudAsync();
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
    private readonly int[] baseFoodGainBySupplyLevel = { 35, 39, 47, 57, 74, 115, 155, 200, 255 };
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
            out int buildingBonusMaxFood,
            out float buildingFoodGainPercent,
            out float buildingCooldownReduction,
            out int buildingRareSlots,
            out int buildingEpicSlots,
            _tileEfficiencyBonuses
        );

        int baseMaxFood = 20000;

        // 기본값에 건물들의 '플랫(flat)' 보너스를 더함 (블록 시너지 포함)
        float blockMultiplier = 1.0f + (SynergyBlockBonusPercent / 100.0f);
        int bonusFromBuildings = Mathf.CeilToInt(buildingBonusMaxFood * blockMultiplier); // 결과: 2200


        // (기본값 + 플랫 보너스)에 시너지 '퍼센트(%)' 보너스를 적용
        float globalMultiplier = 1.0f + (SynergyMaxFoodBonus / 100.0f);

        _calculatedMaxFood = Mathf.CeilToInt((baseMaxFood + bonusFromBuildings) * globalMultiplier);

        // --- 나머지 효과들도 전역 시너지 보너스를 최종 합산 ---
        currentFarmGainPercent = buildingFoodGainPercent + SynergyFoodProductionBonus + SynergyBlockBonusPercent;
        TotalUnitCooldownReduction = buildingCooldownReduction + SynergyUnitCooldownReduction;
        RareUnitSlots = buildingRareSlots;
        EpicUnitSlots = buildingEpicSlots;

        if (MaxFood > _calculatedMaxFood) { MaxFood = _calculatedMaxFood; }

        OnResourceChangedEvent?.Invoke(ResourceType.Food, CurrentFood);
        Debug.Log($"모든 건물+시너지 효과 계산 완료: 최대 식량={_calculatedMaxFood}, 식량 보너스={currentFarmGainPercent}%, 유닛 쿨감={TotalUnitCooldownReduction}%");
        EventManager.GetPublisher<SynergyDataUpdatedEvent>().Publish(new SynergyDataUpdatedEvent());
    }

    #endregion

    // 스테이지 클리어 기록
    #region Clear Stage

    public bool IsStageCleared(int mainStage, int subStage)
    {
        return clearedStages.ContainsKey((mainStage, subStage)) && clearedStages[(mainStage, subStage)];
    }

    public void UpdateClearedStagesFromServer(List<(int main, int sub)> serverClearedStages) //서버에서 클리어 
    {
        clearedStages.Clear(); // 일단 로컬 정보 초기화
        foreach (var stage in serverClearedStages)
        {
            clearedStages[stage] = true;
        }

        _clearedStagesEvent?.Publish(new ClearedStagesUpdatedEvent());
        Debug.Log("ClearedStagesUpdatedEvent 발행 완료.");
    }
    public void MarkLocalStageClear(int mainStage, int subStage)
    {
        if (IsStageCleared(mainStage, subStage)) return;

        Debug.Log($"<color=cyan>[PlayerData]</color> 스테이지 ({mainStage}, {subStage}) 로컬 최초 클리어 기록!");
        clearedStages[(mainStage, subStage)] = true;

        if (mainStage == 1 && subStage == 3)
        {
            AddResource(ResourceType.Ticket, 10);
            Debug.Log("<color=green>[보상 지급]</color> 스테이지 1-3 최초 클리어 보상: 티켓 10개 지급!");
        }
        _clearedStagesEvent?.Publish(new ClearedStagesUpdatedEvent());
        Debug.Log("[PlayerData] ClearedStagesUpdatedEvent 발행 완료.");
    }

    #endregion

    //가챠시스템
    #region
    public void UpdateLimitedPityCount(bool isEpicResult)
    {
        if (isEpicResult)
        {
            LimitedGachaPityCount = 0; // 에픽 획득 시 초기화
            Debug.Log("<color=yellow>[천장-한정]</color> 에픽 획득! 카운터 초기화.");
        }
        else
        {
            LimitedGachaPityCount++; // 에픽 아니면 증가
                                     // 천장 도달 시 초기화는 가챠 로직(GachaUIPanel)에서 처리 후 0으로 리셋 요청
            Debug.Log($"<color=yellow>[천장-한정]</color> 카운터 증가: {LimitedGachaPityCount}");
        }

        _limitedPityPublisher?.Publish(new LimitedPityCountUpdatedEvent { NewCount = LimitedGachaPityCount });
    }

    public void UpdateStandardPityCount(bool isEpicResult)
    {
        if (isEpicResult)
        {
            StandardGachaPityCount = 0;
            Debug.Log("<color=yellow>[천장-상시]</color> 에픽 획득! 카운터 초기화.");
        }
        else
        {
            StandardGachaPityCount++;
            Debug.Log($"<color=yellow>[천장-상시]</color> 카운터 증가: {StandardGachaPityCount}");
        }

        _standardPityPublisher?.Publish(new StandardPityCountUpdatedEvent { NewCount = StandardGachaPityCount });
    }

    public void LoadPityCounts(int loadedLimitedCount, int loadedStandardCount)
    {
        LimitedGachaPityCount = loadedLimitedCount;
        StandardGachaPityCount = loadedStandardCount;
        Debug.Log($"[PlayerData] 천장 카운터 로드 완료 - 한정: {LimitedGachaPityCount}, 상시: {StandardGachaPityCount}");

        _limitedPityPublisher?.Publish(new LimitedPityCountUpdatedEvent { NewCount = LimitedGachaPityCount });
        _standardPityPublisher?.Publish(new StandardPityCountUpdatedEvent { NewCount = StandardGachaPityCount });
    }
    #endregion
    public StageDestinyData currentDastiny { get; set; } = new StageDestinyData();
    public Dictionary<int, int> activeChallenges { get; private set; } = new Dictionary<int, int>();


    #region 저장 관련
    private Dictionary<int, List<int>> ConvertDeckToInt()
    {
        Dictionary<int, List<int>> result = new();

        for (int i = 1; i <= DeckPresets.Count; i++)
        {
            result[i] = DeckPresets[i].UnitIds;
        }

        return result;
    }

    private void ConvertIntToDeck(Dictionary<int, List<int>> loadIntDic)
    {
        for (int i = 1; i <= loadIntDic.Count; i++)
        {
            for (int j = 0; j < DeckPresets[i].BaseUnitDatas.Count; j++)
            {
                int id = loadIntDic[i][j];
                if (id != -1)
                    DeckPresets[i].BaseUnitDatas[j] = DataManager.PlayerUnitData.GetData(id);
            }

        }
    }


    public async UniTask SaveDataToCloudAsync()
    {
        // 1. 현재 PlayerDataManager의 상태를 스냅샷으로 생성
        var saveData = new PlayerSaveData
        {
            ClearData = SettingDataManager.Instance.SaveClearData(),
            DeckPresets = ConvertDeckToInt(), // 딕셔너리 전체 저장 //하니까 직렬화에서 에러나서 저장할땐 int로 하겠습니당
            ActiveDeckIndex = this.ActiveDeckIndex,
            OwnedCardData = this.OwnedCardData.Keys.ToList<int>(),
            OwnedArtifacts = ArtifactManager.Instance.SaveArtifactData(ArtifactManager.Instance.OwnedArtifacts),
            EquippedArtifacts = ArtifactManager.Instance.SaveArtifactData(ArtifactManager.Instance.EquippedArtifacts),

            // TileDataHandler의 상태를 직렬화 가능한 형태로 변환
            TileGridData = _TileDataHandler.GetSnapshot()
        };

        Dictionary<string, object> cloudData = new();

        cloudData[Constants.PLAYER_DATA_KEY] = saveData;

        Debug.Log("플레이어 데이터 스냅샷 생성 완료.");

        // 2. BackendManager를 사용하여 클라우드에 데이터 전송
        await BackendManager.SaveDataAsync(cloudData);

        Debug.Log("✅ 플레이어 데이터 클라우드 저장 완료.");
    }


    public async UniTask LoadDataFromCloundAsync()
    {
        PlayerSaveData loadedData = await BackendManager.LoadDataAsync();

        //처음 실행하면 초기 데이터 세팅
        if (loadedData == null)
        {
            //일단 가챠 유닛 제외 전부 넣어둠
            List<int> initalUnitIds = new List<int>();
            for (int i = 100001; i < 100011; i++)
            {
                initalUnitIds.Add(i);
            }

            CardGenerate(initalUnitIds);
            return;
        }

        try
        {
            SettingDataManager.Instance.LoadClearData(loadedData.ClearData);
            ConvertIntToDeck(loadedData.DeckPresets);
            this.ActiveDeckIndex = loadedData.ActiveDeckIndex;
            CardGenerate(loadedData.OwnedCardData);
            _TileDataHandler.RestoreFromSnapshot(loadedData.TileGridData);
            ArtifactManager.Instance.LoadArtifactData(loadedData.OwnedArtifacts, loadedData.EquippedArtifacts);

        }
        catch (NullReferenceException)
        {
            Debug.Log("세이브 데이터가 손상되었거나 이전 개발 버전입니다.");

            //일단 가챠 유닛 제외 전부 넣어둠
            List<int> initalUnitIds = new List<int>();
            for (int i = 100001; i < 100011; i++)
            {
                initalUnitIds.Add(i);
            }

            CardGenerate(initalUnitIds);

        }

        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    #endregion
}

[System.Serializable]
public class PlayerSaveData
{
    // 1. 스테이지 해금 정보
    //SettingDataManger와 연계 필요
    public List<List<bool>> ClearData;

    // 2. 덱 데이터
    public Dictionary<int, List<int>> DeckPresets;
    public int ActiveDeckIndex;

    // 3. 영지 타일 데이터 
    public TileDataSnapshot TileGridData;

    //4. 보유한 유닛
    public List<int> OwnedCardData;

    //5. 보유한 유물
    //유물들은 Newtonsoft.Json를 사용해 패시브, 액티브로 알아서 전환
    public string OwnedArtifacts;

    //6. 장착한 유물
    public string EquippedArtifacts;
}




