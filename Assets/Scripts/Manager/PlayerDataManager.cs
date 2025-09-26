using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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
[System.Serializable]
public class DeckData
{
    public string DeckName;
    public List<int> UnitIds;

    public DeckData(string defaultName)
    {
        DeckName = defaultName;
        // 9개의 빈 슬롯(-1)으로 초기화
        UnitIds = new List<int>(new int[9]);
        for (int i = 0; i < 9; i++)
        {
            UnitIds[i] = -1;
        }
    }
}
public class PlayerDataManager : SingletonMono<PlayerDataManager>
{
    // 선택한 스테이지 선택용
    public (int mainStageIdx, int subStageIdx) SelectedStageIdx { get; set; } = (-1, -1);

    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
        {
            InitializeResources();
            LoadDecks();
        }

        LoadArtifactData();

        // 패시브 유물 테스트 -----
        AddArtifact(080200015);
        AddArtifact(080200014);
        AddArtifact(080200025);
        AddArtifact(080200024);
        AddArtifact(080200035);
        AddArtifact(080200034);
        AddArtifact(080200055);
        AddArtifact(080200054);
        AddArtifact(080200054);
        AddArtifact(080200085);
        AddArtifact(080200084);
        // ------------------------
    }

    private void Start()
    {
        
    }

    //빌딩 데이터
    #region Building
    public BuildingUpgradeData[,] BuildingGridData { get; set; } = new BuildingUpgradeData[5, 5];
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
        _resources[ResourceType.MagicStone] = 10000;
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

                if (amount < 0)
                {
                    MaxFood += amount;
                    if (MaxFood < 0) MaxFood = 0;
                }
            }

            OnResourceChangedEvent?.Invoke(type, _resources[type]);
        }
        else
        {
            Debug.LogWarning($"ResourceManager: 존재하지 않는 자원 타입입니다. ({type})");
        }
    }
    #endregion

    #region Food
    //식량에 관련된 변수와 함수
    public int CurrentFood { get; private set; } = 0;
    public int MaxFood { get; private set; } = 20000;
    private int _calculatedMaxFood = 20000;
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
        if (CurrentFood >= requiredFood)
        {
            AddResource(ResourceType.Food, -requiredFood);
            SupplyLevel++;
            Debug.Log($"Supply Level Up! 현재 SupplyLevel: {SupplyLevel}");
        }
        else
        {
            Debug.Log($"식량이 부족합니다. 필요 식량: {requiredFood}");
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
            MaxFood -= gainInt;
            if (MaxFood < 0) MaxFood = 0;
            CurrentFood += gainInt;
            if (CurrentFood > MaxFood) CurrentFood = MaxFood;
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
        // --- 계산 전, 모든 보너스 값을 초기화 ---
        int totalCalculatedMax = 20000;
        float totalGainPercent = 0f;
        TotalUnitCooldownReduction = 0f;
        RareUnitSlots = 0;
        EpicUnitSlots = 0;

        // --- 모든 그리드를 순회하며 건물 효과를 합산 ---
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                var building = BuildingGridData[x, y];
                if (building == null) continue;

                foreach (var effect in building.effects)
                {
                    switch (effect.effectType)
                    {
                        case BuildingEffectType.MaximumFood:
                            if (building.buildingType == BuildingType.Farm)
                            {
                                totalCalculatedMax += (int)effect.effectValueMin;
                            }
                            break;
                        case BuildingEffectType.IncreaseFoodGainSpeed:
                            if (building.buildingType == BuildingType.Farm)
                            {
                                totalGainPercent += effect.effectValueMin;
                            }
                            break;
                        case BuildingEffectType.UnitCoolDown:
                            if (building.buildingType == BuildingType.Barracks)
                            {
                                TotalUnitCooldownReduction += effect.effectValueMin;
                            }
                            break;
                        case BuildingEffectType.CanSummonRareUnits:
                            if (building.buildingType == BuildingType.Barracks)
                            {
                                RareUnitSlots += (int)effect.effectValueMin;
                            }
                            break;
                        case BuildingEffectType.CanSummonEpicUnits:
                            if (building.buildingType == BuildingType.Barracks)
                            {
                                EpicUnitSlots += (int)effect.effectValueMin;
                            }
                            break;
                    }
                }
            }
        }

        // --- 최종 계산된 농장 관련 값들을 업데이트 ---
        _calculatedMaxFood = totalCalculatedMax;
        currentFarmGainPercent = totalGainPercent;
        if (MaxFood > _calculatedMaxFood)
        {
            MaxFood = _calculatedMaxFood;
        }

        OnResourceChangedEvent?.Invoke(ResourceType.Food, CurrentFood);
        Debug.Log($"모든 건물 효과 계산 완료: 최대 식량={_calculatedMaxFood}, 식량 보너스={currentFarmGainPercent}%, 유닛 쿨감={TotalUnitCooldownReduction}%, 레어 슬롯={RareUnitSlots}, 에픽 슬롯={EpicUnitSlots}");
    }
    #endregion

    // 유물 관련
    #region Artifact

    // 장비 장착했을 때 변경사항 반영하는 델리게이트
    public event Action OnEquipArtifactChanged;  

    // 플레이어가 보유 중인 유물 리스트
    public List<ArtifactData> OwnedArtifacts { get; private set; } = new List<ArtifactData>();

    // 플레이어가 장착한 유물 딕셔너리 -> EffectType이 장착 부위? 처럼 작동하는 것 같아서 딕셔너리로 했어용
    // value는 리스트 말고 배열로 바꿈 -> 몇 번째인 지 알아야 함 + 고정된 슬롯 수가 필요할 것 같아서.
    public Dictionary<EffectTarget, PassiveArtifactData[]> EquippedArtifacts { get; private set; }

    private const int PlayerArtifactSlotCount = 4;
    private const int MeleeArtifactSlotCount = 2;
    private const int RangedArtifactSlotCount = 2;

    public void AddArtifact(int artifactId)     // 특정 유물을 플레이어가 보유 중인 유물 리스트에 추가하는 메서드
    {
        if (DataManager.Instance.ArtifactData.TryGetValue(artifactId, out ArtifactData data))
        {
            OwnedArtifacts.Add(data);
        }
        else
        {
            Debug.Log("유물 id null이거나 뭔가 문제 있어요 점검하기");
        }
    }

    // 유물장착하는 메서드인데 아직 유물 장착, 해제 로직이 분리가 안 됨 분리 해야 함
    public void EquipArtifact(PassiveArtifactData equipArtifact, int slotIndex)     
    {
        if (equipArtifact == null) return;

        EffectTarget target = equipArtifact.effectTarget;

        PassiveArtifactData[] slots = EquippedArtifacts[target];

        if (slotIndex >= 0 && slotIndex < slots.Length)
        {
            if (slots[slotIndex] != null)
            {
                Debug.Log($"{slots[slotIndex]} 장착 해제하고 유물 갈아끼움");
            }

            slots[slotIndex] = equipArtifact;
            Debug.Log($"{target}의 {slotIndex}번 슬롯에 {equipArtifact.name} 유물 정상 장착함");

            OnEquipArtifactChanged?.Invoke();
        }
    }

    public void LoadArtifactData()
    {
        // 저장된 데이터 불러오는 로직 넣기~~~~ 지금은 못 넣음~~~~~

        bool hasSaveData = false;

        if (hasSaveData)
        {
            // 저장 데이터 불러오는 거 넣기~~~
        }
        else    // 아예 게임 처음이면 초기화 메서드 
        {
            InitializeEquippedArtifacts();
        }
    }

    private void InitializeEquippedArtifacts()      // 유물 초기화 메서드 -> 없어도 괜찮은데 나중에 저장 기능 생길까봐
    {
        EquippedArtifacts = new Dictionary<EffectTarget, PassiveArtifactData[]>
        {
            {EffectTarget.Player, new PassiveArtifactData[PlayerArtifactSlotCount]},
            {EffectTarget.MeleeUnit, new PassiveArtifactData[MeleeArtifactSlotCount]},
            {EffectTarget.RangedUnit, new PassiveArtifactData[RangedArtifactSlotCount]}
        };
    }
    #endregion
}



