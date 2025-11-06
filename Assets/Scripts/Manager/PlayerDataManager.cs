using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Unity.Services.CloudCode.GeneratedBindings.CheerUpMyHero.CloudCode;
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
    Ticket,
    EXP
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

    // 레벨 관련 프로퍼티
    private int _playerLevel = 1;
    public int PlayerLevel
    {
        get => _playerLevel;
        set
        {
            if (value <= 0)
            {
                Debug.LogWarning($"⚠️ PlayerLevel을 {value}로 설정하려 했으나, 최소값 1로 보정합니다.");
                _playerLevel = 1;
            }
            else
            {
                _playerLevel = value;
            }
        }
    }

    public int CurExp { get; set; }

    #region 영지 시너지 보너스
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

    public static int LimitedGachaPityLimit { get; private set; }
    public static int StandardGachaPityLimit { get; private set; }

    private IEventPublisher<LimitedPityCountUpdatedEvent> _limitedPityPublisher;
    private IEventPublisher<StandardPityCountUpdatedEvent> _standardPityPublisher;

    // 적용된 덱 유닛 시너지
    Dictionary<UnitSynergyType, SynergyGrade> appliedDeckUnitSynergies = new Dictionary<UnitSynergyType, SynergyGrade>();
    public static Dictionary<UnitSynergyType, SynergyGrade> AppliedDeckUnitSynergies { get => Instance.appliedDeckUnitSynergies; }

    // 
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
            //List<BaseUnitData> unitList = DataManager.PlayerUnitData.SO.allianceCommon;
            //for (int i = 0; i < unitList.Count; i++)
            //{
            //    OwnedCardData[unitList[i].idNumber] = unitList[i];
            //}
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
        if (!e.IsVictory && GameManager.IsTutorialCompleted) // 튜토리얼 중에는 영지 타일 데미지 없음
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

    public async UniTask DestroyBuildingAt(int x, int y)
    {
        var buildingData = _TileDataHandler.BuildingGridData[x, y];
        if (buildingData == null) return;

        List<Cost> totalCost = CalculateTotalInvestedCost(buildingData);
        foreach (var cost in totalCost)
        {
            int refundAmount = Mathf.FloorToInt(cost.amount * 0.5f);
            await AddResource(cost.resourceType, refundAmount);
        }

        _TileDataHandler.BuildingGridData[x, y] = null;
        _TileDataHandler.CooldownEndTimeGrid[x, y] = DateTime.MinValue;

        await SaveDataToCloudAsync();

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
        if (OwnedCardData.ContainsKey(id))
        {
            Debug.Log("중복 획득. 유닛 강화시스템 언젠가 추가 예정...");
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
        try
        {// 5가지 자원을 모두 딕셔너리에 추가하고 초기 수량을 설정.
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
        }
        catch (Exception ex) //실패하면 정상적으로 진행 안되니까 재시도??
        { 
        
            Debug.LogException(ex);
        }
  
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
    public async UniTask AddResource(ResourceType type, int amount)
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

            //if (type == ResourceType.EXP)
            //{
            //    CheckLevelUp();
            //}

            await BackendManager.ChangeEconomy(BackendManager.EconomyEnumToId(type), amount);
        }
        else
        {
            Debug.LogWarning($"ResourceManager: 존재하지 않는 자원 타입입니다. ({type})");
        }
    }

    //서버랑 직접 통신하지 않는, 로컬 버전. 서버쪽에서 알아서 재화를 바꾼 것을 반영하는데 사용
    public void ChangeResourceOnlyLocal(ResourceType type, int amount)
    {
        Debug.Log($"<color=yellow>[PlayerData AddResource]</color> '{type}' 자원 {amount} 변경 요청 받음.");

        if (_resources.ContainsKey(type))
        {
            int previousAmount = _resources[type];
            _resources[type] = amount;
            int currentAmount = _resources[type];

            Debug.Log($"[PlayerData AddResource] '{type}' 값 변경: {previousAmount} -> {currentAmount}");

            if (type == ResourceType.Food)
            {
                CurrentFood = _resources[type];
            }

            OnResourceChangedEvent?.Invoke(type, _resources[type]);

            //if (type == ResourceType.EXP)
            //{
            //    CheckLevelUp();
            //}
        }
        else
        {
            Debug.LogWarning($"ResourceManager: 존재하지 않는 자원 타입입니다. ({type})");
        }
    }

    public async UniTask<(int gold, int wood, int iron, int magicStone)> ApplyDefeatPenalties()
    {
        var resourcePenalties = await ApplyResourcePenalty();

        return resourcePenalties;
    }

    public async UniTask<(int gold, int wood, int iron, int magicStone)> ApplyResourcePenalty()
    {
        int goldPenalty = Mathf.CeilToInt(GetResourceAmount(ResourceType.Gold) * 0.05f); 
        int woodPenalty = Mathf.CeilToInt(GetResourceAmount(ResourceType.Wood) * 0.05f); 
        int ironPenalty = Mathf.CeilToInt(GetResourceAmount(ResourceType.Iron) * 0.05f); 
        int magicStonePenalty = Mathf.CeilToInt(GetResourceAmount(ResourceType.MagicStone) * 0.05f); 

        // 2. 실행할 비동기 작업(Task)들을 리스트에 담습니다.
        //    - 이때 await를 사용하지 않아 모든 작업이 즉시 "시작"됩니다.
        var penaltyTasks = new List<UniTask>
        {
            AddResource(ResourceType.Gold, -goldPenalty),
            AddResource(ResourceType.Wood, -woodPenalty),
            AddResource(ResourceType.Iron, -ironPenalty),
            AddResource(ResourceType.MagicStone, -magicStonePenalty)
        };

        // 3. Task.WhenAll을 사용해 리스트의 모든 작업이 완료될 때까지 "한 번만" 기다립니다.
        await UniTask.WhenAll(penaltyTasks);

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
    private readonly int[] baseFoodGainBySupplyLevel = { 45, 49, 57, 67, 84, 125, 165, 210, 265 };
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

    //현재 서버에서 List<List<bool>>을 쓰고 있어서, 거기에 맞출께용
    public void UpdateClearedStagesFromServer(List<List<bool>> serverClearedStages) //서버에서 해금 데이터 가져옴 
    {
        clearedStages.Clear(); // 일단 로컬 정보 초기화
        //foreach (var stage in serverClearedStages)
        //{
        //    clearedStages[stage] = true;
        //}

        for (int i = 0; i < serverClearedStages.Count; i++)
        {
            for(int j = 0; j < serverClearedStages[i].Count; j++)
            {
                //생각해보니, 모든 데이터를 볼 필요없이 true까지인 것만 보면 되는거 아닌가?

                if (serverClearedStages[i][j])
                    clearedStages[(i + 1, j + 1)] = serverClearedStages[i][j];
                else
                {
                    //예시: 1-5까지 깸 => 1-6까지 unlock = true (세팅 데이터 매니저 = 우리가 보는 스테이지 화면 정보)
                    //serverClearedStages[0][5]지 true, serverClearedStages[0][6] 부터 false
                    //근데 serverClearedStages[0][5]는 false여야 함 => 수동으로 처리

                    //만약 ClearedStages[0][8]까지 true라면, serverClearedStages[1][0] 부터 false
                    if (j == 0)
                    {
                        clearedStages[(i, 8)] = false;
                    }
                    else
                    {
                        clearedStages[(i + 1, j)] = false;
                    }
                    break;
                }

                
            }
        }


        _clearedStagesEvent?.Publish(new ClearedStagesUpdatedEvent());
        Debug.Log("ClearedStagesUpdatedEvent 발행 완료.");
    }
    public async UniTask MarkLocalStageClear(int mainStage, int subStage)
    {
        if (IsStageCleared(mainStage, subStage)) return;

        Debug.Log($"<color=cyan>[PlayerData]</color> 스테이지 ({mainStage}, {subStage}) 로컬 최초 클리어 기록!");
        clearedStages[(mainStage, subStage)] = true;

        if (mainStage == 1 && subStage == 2)
        {
            await AddResource(ResourceType.Ticket, 10);
            Debug.Log("<color=green>[보상 지급]</color> 스테이지 1-2 최초 클리어 보상: 티켓 10개 지급!");
        }
        _clearedStagesEvent?.Publish(new ClearedStagesUpdatedEvent());
        Debug.Log("[PlayerData] ClearedStagesUpdatedEvent 발행 완료.");
    }

    #endregion

    //가챠시스템
    #region
    //public void UpdateLimitedPityCount(bool isEpicResult)
    //{
    //    if (isEpicResult)
    //    {
    //        LimitedGachaPityCount = 0; // 에픽 획득 시 초기화
    //        Debug.Log("<color=yellow>[천장-한정]</color> 에픽 획득! 카운터 초기화.");
    //    }
    //    else
    //    {
    //        LimitedGachaPityCount++; // 에픽 아니면 증가
    //                                 // 천장 도달 시 초기화는 가챠 로직(GachaUIPanel)에서 처리 후 0으로 리셋 요청
    //        Debug.Log($"<color=yellow>[천장-한정]</color> 카운터 증가: {LimitedGachaPityCount}");
    //    }

    //    _limitedPityPublisher?.Publish(new LimitedPityCountUpdatedEvent { NewCount = LimitedGachaPityCount });
    //}

    public void UpdateLimitedPityCount(int currentPity)
    {
        LimitedGachaPityCount = currentPity;

        _limitedPityPublisher?.Publish(new LimitedPityCountUpdatedEvent { NewCount = LimitedGachaPityCount });
    }

    //public void UpdateStandardPityCount(bool isEpicResult)
    //{
    //    if (isEpicResult)
    //    {
    //        StandardGachaPityCount = 0;
    //        Debug.Log("<color=yellow>[천장-상시]</color> 에픽 획득! 카운터 초기화.");
    //    }
    //    else
    //    {
    //        StandardGachaPityCount++;
    //        Debug.Log($"<color=yellow>[천장-상시]</color> 카운터 증가: {StandardGachaPityCount}");
    //    }

    //    _standardPityPublisher?.Publish(new StandardPityCountUpdatedEvent { NewCount = StandardGachaPityCount });
    //}

    public void UpdateStandardPityCount(int currentPity)
    {
        StandardGachaPityCount = currentPity;

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
    public StageDestinyData currentDestiny { get; set; } = new StageDestinyData();
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
    private List<string> SaveDeckName()
    {
        List<string> result = new List<string>();
        for (int i = 1; i <= DeckPresets.Count; i++)
        {
            result.Add(DeckPresets[i].DeckName);
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
                {
                    DeckPresets[i].UnitIds[j] = id;
                    DeckPresets[i].BaseUnitDatas[j] = DataManager.PlayerUnitData.GetData(id);
                }
            }

        }
    }

    private void LoadDeckName(List<string> loadedName)
    {
        for (int i = 0; i < loadedName.Count; i++)
        {
            DeckPresets[i+1].DeckName = loadedName[i];
        }
    }


    public async UniTask SaveDataToCloudAsync()
    {
        if (!GameManager.IsTutorialCompleted)
            return;

        try
        {
            //저장 중이라는 표시를 띄울수도 있음. 근데 요즘 모바일 겜 중에 그런건 없으니..
            await InternalSaveDataToCloudAsync();
        }

        catch(Exception e) 
        {
            Debug.LogException(e);
        }
    }


    private async UniTask InternalSaveDataToCloudAsync()
    {
        // 1. 현재 PlayerDataManager의 상태를 스냅샷으로 생성
        var saveData = new PlayerSaveData
        {
            UnlockData = SettingDataManager.Instance.SaveClearData(), //주의 사항: 클리어 데이터가 아니고 해금 데이터. 마지막으로 true인 스테이지는 클리어된게 아니다. 
            DeckPresets = ConvertDeckToInt(), // 딕셔너리 전체 저장 //하니까 직렬화에서 에러나서 저장할땐 int로 하겠습니당
            DeckNames = SaveDeckName(),
            ActiveDeckIndex = this.ActiveDeckIndex,
            OwnedCardData = this.OwnedCardData.Keys.ToList<int>(),
            OwnedArtifacts = ArtifactManager.Instance.SaveArtifactData(ArtifactManager.Instance.OwnedArtifacts),
            EquippedArtifacts = ArtifactManager.Instance.SaveArtifactData(ArtifactManager.Instance.EquippedArtifacts),

            // TileDataHandler의 상태를 직렬화 가능한 형태로 변환
            TileGridData = _TileDataHandler.GetSnapshot(),

            //서버에서 알아서 저장
            //LimitedGachaPityCount = this.LimitedGachaPityCount,
            //StandardGachaPityCount = this.StandardGachaPityCount
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
        try
        {
            //비동기를 변수에 넣고 기다리지 않고 일단 진행
            var allCloudData = await BackendManager.LoadDataAsync();

            //await가 붙은 부분에서 기다림
            PlayerSaveData loadedData = allCloudData.PlayerSaveData;

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
                Debug.Log($"유닛해금 시도: {initalUnitIds.Count}");
                return;
            }

            //튜토리얼 끝날때 까지 저장 x => 저장된다면, 튜토리얼이 끝난 것 => 저장된 데이터가 있다면, 튜토리얼이 끝난 것
            GameManager.IsTutorialCompleted = true; 

            SettingDataManager.Instance.LoadClearData(loadedData.UnlockData);
            UpdateClearedStagesFromServer(loadedData.UnlockData);
            ConvertIntToDeck(loadedData.DeckPresets);
            LoadDeckName(loadedData.DeckNames);
            this.ActiveDeckIndex = loadedData.ActiveDeckIndex;
            CardGenerate(loadedData.OwnedCardData);
            _TileDataHandler.RestoreFromSnapshot(loadedData.TileGridData);
            ArtifactManager.Instance.LoadArtifactData(loadedData.OwnedArtifacts, loadedData.EquippedArtifacts);

            //구 가챠 데이터 불러오기
            //this.LimitedGachaPityCount = loadedData.LimitedGachaPityCount;
            //this.StandardGachaPityCount = loadedData.StandardGachaPityCount;

            //신 가챠 데이터 불러오기
            StandardGachaPityCount = allCloudData.NormalPity;
            StandardGachaPityLimit = allCloudData.NormalPityThreshold;
            LimitedGachaPityCount = allCloudData.PickupPity;
            LimitedGachaPityLimit = allCloudData.PickupPityThreshold;

        }

        catch (Exception ex) when (ex.GetType().Name == "DeserializationException" || ex is NullReferenceException) //문자열 비교라서 보기 안좋지만, 세이브파일 일치하지 않는건 개발 단계에서만 생기는 거라 대충처리합니다. 
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
    //홈 버튼 등으로 백그라운드로 갈때 호출
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            Debug.Log("앱이 백그라운드로 전환되었습니다. 저장을 시도합니다.");
            // await를 사용하지 않고 Task를 바로 실행시킵니다.
            SaveDataToCloudAsync().Forget();
        }
    }


    #endregion

    //#region 레벨 관련

    //private void CheckLevelUp()
    //{
    //    if (!_resources.ContainsKey(ResourceType.EXP))
    //    {
    //        _resources[ResourceType.EXP] = 0;
    //        return;
    //    }

    //    // PlayerLevel 유효성 검사
    //    if (PlayerLevel <= 0)
    //    {
    //        Debug.LogWarning($"레벨 {PlayerLevel}임. 레벨 불러오는 로직 오류 있어요. 일단 1로 만듦.");
    //        PlayerLevel = 1;
    //    }

    //    int currentExp = GetResourceAmount(ResourceType.EXP);

    //    PlayerData curLevelData = DataManager.PlayerData.GetData(PlayerLevel);

    //    if (curLevelData == null)
    //    {
    //        Debug.LogError($"레벨이 {PlayerLevel}라서 레벨 데이터 null로 뜸. 뭔가 오류 있어요");
    //        return;
    //    }

    //    int expToNextLevel = curLevelData.exp;
    //    if (expToNextLevel <= 0) return;

    //    bool hasLevelUp = false;
    //    while (currentExp >= expToNextLevel)
    //    {
    //        currentExp -= expToNextLevel;
    //        PlayerLevel++;
    //        hasLevelUp = true;

    //        curLevelData = DataManager.PlayerData.GetData(PlayerLevel);
    //        if (curLevelData == null)
    //        {
    //            PlayerLevel--;
    //            Debug.Log($"레벨 데이터 없음 -> 최고레벨 초과라서 없음 -> 최고 레벨로 돌림.");
    //            break;
    //        }

    //        expToNextLevel = curLevelData.exp;
    //        if (expToNextLevel <= 0) break;
    //    }

    //    if (hasLevelUp)
    //    {
    //        _resources[ResourceType.EXP] = currentExp;
    //    }
    //}

    //#endregion
}

[System.Serializable]
public class PlayerSaveData
{
    // 1. 스테이지 해금 정보
    //주의 사항: 클리어 데이터가 아니고 해금 데이터. 마지막으로 true인 스테이지는 클리어된게 아니다.
    public List<List<bool>> UnlockData;

    // 2. 덱 데이터
    public Dictionary<int, List<int>> DeckPresets;
    public List<string> DeckNames;
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

    //7. 가챠 천장: 따로 분리
    //public int LimitedGachaPityCount;
    //public int StandardGachaPityCount;
}






