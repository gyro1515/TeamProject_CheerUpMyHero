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
    MagicStone
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
        }
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
    //편성된 덱 정보
    public List<int> DeckList { get; private set; } = new();

    public void SetDeckList(List<int> deckList)
    {
        DeckList.Clear();
        DeckList = deckList;
        StringBuilder sb = new StringBuilder();

        //
        Debug.Log($"[PlayerDataManager] 현재 덱리스트 크기: {DeckList.Count}");

        for (int i = 0; i < DeckList.Count; i++)
        {
            sb.Append(DeckList[i].ToString());
            sb.Append(", ");
        }
        if (sb.Length < 2)
        {
            Debug.Log($"[PlayerDataManager] 덱 세팅 완료 안내메시지를 호출 실패했습니다");
            return;
        }

        sb.Length -= 2;
        Debug.Log($"[PlayerDataManager] 덱 세팅 완료: {sb.ToString()}");
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
        _resources[ResourceType.Gold] = 100000;
        _resources[ResourceType.Wood] = 10000;
        _resources[ResourceType.Iron] = 10000;
        _resources[ResourceType.Food] = CurrentFood;
        _resources[ResourceType.MagicStone] = 10000;
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
    // --- 푸드/농장 관련 ---
    public int CurrentFood { get; private set; } = 0;
    public int MaxFood { get; private set; } = 20000;

    private int _calculatedMaxFood = 20000;

    private float foodAccumulator = 0f;

    public int SupplyLevel { get; private set; } = 1; // 보급품 레벨, 최소 1
    private float currentFarmGainPercent = 0f; // 모든 농장의 식량 획득률 보너스를 합산할 변수

    // 농장 레벨별 최대 저장량 (엑셀 데이터 기반)
    private readonly int[] maxFoodByFarmLevel = { 500, 750, 1000, 1250, 1500, 1750, 2000, 2250, 2500 };

    // 농장 레벨별 획득률 증가(%)
    private readonly int[] farmFoodGainPercentByLevel = { 5, 10, 15, 20, 25, 30, 35, 40, 50 };

    // 보급품 레벨별 기본 초당 획득량
    private readonly int[] baseFoodGainBySupplyLevel = { 25, 29, 37, 47, 59, 75, 95, 119, 147 };

    private readonly int[] supplyUpgradeCosts = { 100, 220, 450, 900, 1800, 3500, 5500, 8000 };


    //모든 농장 건물의 효과를 합산하여 MaxFood와 획득률을 계산
    public void UpdateTotalFarmEffect()
    {
        int totalCalculatedMax = 20000;
        float totalGainPercent = 0f;

        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                var building = BuildingGridData[x, y];
                if (building != null && building.buildingType == BuildingType.Farm)
                {
                    int levelIndex = Mathf.Clamp(building.level - 1, 0, maxFoodByFarmLevel.Length - 1);
                    totalCalculatedMax += maxFoodByFarmLevel[levelIndex];
                    totalGainPercent += farmFoodGainPercentByLevel[levelIndex];
                }
            }
        }

        // 최종 계산된 값으로 MaxFood와 currentFarmGainPercent를 업데이트합니다.
        _calculatedMaxFood = totalCalculatedMax; // '원래 최대 저장량' 갱신
        currentFarmGainPercent = totalGainPercent;


        // 최대 저장량이 줄었을 경우를 대비해 현재 식량을 조절합니다.
        //if (CurrentFood > MaxFood)
        //    CurrentFood = MaxFood;
        if (MaxFood > _calculatedMaxFood)
        {
            MaxFood = _calculatedMaxFood;
        }

        // UI 갱신을 위해 이벤트를 호출합니다.
        OnResourceChangedEvent?.Invoke(ResourceType.Food, CurrentFood);
        Debug.Log($"농장 효과 총합 계산 완료: 최대 식량 = {MaxFood}, 추가 획득률 = {currentFarmGainPercent}%");
    }

    public void UpgradeSupplyLevel()
    {
        // 현재 레벨이 최대 레벨인지 확인
        if (SupplyLevel >= baseFoodGainBySupplyLevel.Length)
        {
            Debug.Log("최대 레벨입니다.");
            return;
        }

        // 다음 레벨업에 필요한 비용을 가져옴 (현재 레벨 1 -> 비용 인덱스 0)
        int requiredFood = supplyUpgradeCosts[SupplyLevel - 1];

        // 현재 식량이 필요한 비용보다 충분한지 확인
        if (CurrentFood >= requiredFood)
        {
            // 식량 차감
            AddResource(ResourceType.Food, -requiredFood);

            // 레벨업
            SupplyLevel++;
            Debug.Log($"Supply Level Up! 현재 SupplyLevel: {SupplyLevel}");
        }
        else
        {
            // 식량이 부족할 경우
            Debug.Log($"식량이 부족합니다. 필요 식량: {requiredFood}");
        }
    }



    // --- 보급품 획득 ---
    public void AddFoodOverTime(float deltaTime)
    {
        // 최대 저장량이 0이면 식량이 오르지 않도록 방지
        if (MaxFood <= 0) return;

        int baseGain = baseFoodGainBySupplyLevel[SupplyLevel - 1];

        // 새로 만든 currentFarmGainPercent 변수를 사용하도록 수정
        float gainThisFrame = baseGain * (1f + currentFarmGainPercent / 100f) * deltaTime;
        foodAccumulator += gainThisFrame;

        int gainInt = Mathf.FloorToInt(foodAccumulator);
        if (gainInt > 0)
        {
            //얻는 식량만큼 현재 최대 저장량을 감소
            MaxFood -= gainInt;
            if (MaxFood < 0) MaxFood = 0;

            CurrentFood += gainInt;

            // 현재 식량이 (줄어든) 최대 저장량을 초과할 수 없도록 제한
            if (CurrentFood > MaxFood)
                CurrentFood = MaxFood;

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

        _resources[ResourceType.Food] = CurrentFood;
        OnResourceChangedEvent?.Invoke(ResourceType.Food, CurrentFood);
        Debug.Log("현재 식량을 0으로, 최대 식량을 원래 값으로 초기화했습니다.");
    }
    public bool TryGetUpgradeCost(out int cost)
    {
        cost = 0;
        if (SupplyLevel >= baseFoodGainBySupplyLevel.Length)
        {
            return false; // 최대 레벨
        }
        cost = supplyUpgradeCosts[SupplyLevel - 1];
        return true;
    }

    #endregion
}



