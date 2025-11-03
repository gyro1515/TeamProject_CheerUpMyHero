using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class ConstructionUpgradePanel : BasePopUpUI
{
    [Header("UI 요소 연결")]
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI effectText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button destroyButton;

    [Header("이미지 및 레벨 텍스트")]
    [SerializeField] private Image currentImage;
    [SerializeField] private Image nextImage;
    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private TextMeshProUGUI nextLevelText;
    [SerializeField] private GameObject arrowImage;

    [Header("효과 UI 그룹")]
    [SerializeField] private GameObject effectContainer;
    [SerializeField] private GameObject currentEffectGroup;
    [SerializeField] private GameObject nextEffectGroup;
    [SerializeField] private TextMeshProUGUI currentEffectText;
    [SerializeField] private TextMeshProUGUI nextEffectText;

    private BuildingTile _targetTile;
    private BuildingUpgradeData _constructionData; // 건설 시 사용할 데이터 (0레벨)
    private BuildingUpgradeData _upgradeData;      // 업그레이드 시 사용할 데이터 (다음 레벨)
    //private CanvasGroup _canvasGroup;

    //private bool _isClosing = false;

    private enum PanelMode { None, Construction, Upgrade, Repair }
    private PanelMode _mode = PanelMode.None;

    protected override void Awake()
    {
        base.Awake();
        //_canvasGroup = GetComponent<CanvasGroup>();
        actionButton.onClick.AddListener(() => { OnActionButtonClick().Forget(); });
        closeButton.onClick.AddListener(() => CloseUI());
    }
    public override void CloseUI()
    {
        if (MainScreenBuildingController.Instance != null)
        {
            MainScreenBuildingController.Instance.DeselectTile();
        }

        _targetTile = null;

        base.CloseUI();
    }
    // --- 업그레이드 초기화 ---
    public void InitializeForUpgrade(BuildingTile tile)
    {
        _targetTile = tile;
        _mode = PanelMode.Upgrade;
        BuildingUpgradeData currentData = PlayerDataManager.Instance._TileDataHandler.BuildingGridData[tile.X, tile.Y];
        if (currentData == null) return;

        _upgradeData = DataManager.Instance.BuildingUpgradeData.GetData(currentData.nextLevel);

        destroyButton.gameObject.SetActive(true);
        destroyButton.GetComponentInChildren<TextMeshProUGUI>().text = "파괴";
        destroyButton.onClick.RemoveAllListeners(); // 기존 리스너 모두 제거
        destroyButton.onClick.AddListener(OnDestroyButtonClicked); // '파괴' 기능 연결
        UpdatePanelContents();
    }

    // --- 건설 초기화 ---
    public void InitializeForConstruction(BuildingTile tile, int buildingBaseID)
    {
        _targetTile = tile;
        _mode = PanelMode.Construction;

        _constructionData = DataManager.Instance.BuildingUpgradeData.GetData(buildingBaseID);
        currentImage.sprite = tile.emptyTileSprite;
        destroyButton.gameObject.SetActive(false);
        destroyButton.onClick.RemoveAllListeners(); // 리스너 비우기
        UpdatePanelContents();
    }

    // ---수리 초기화---
    public void InitializeForRepair(BuildingTile tile)
    {
        _targetTile = tile;
        _mode = PanelMode.Repair;
        _constructionData = null;
        _upgradeData = null;
        destroyButton.gameObject.SetActive(true); // "광고 수리" 버튼 (재활용)
        destroyButton.GetComponentInChildren<TextMeshProUGUI>().text = "광고 수리";
        destroyButton.onClick.RemoveAllListeners(); 
        destroyButton.onClick.AddListener(OnAdRepairButtonClicked); 
        UpdatePanelContents();
    }

    // --- UI 업데이트 ---
    private void UpdatePanelContents()
    {
        effectContainer.SetActive(true); // 효과 컨테이너는 항상 켬

        if (_mode == PanelMode.Construction)
        {
            var level1Data = DataManager.Instance.BuildingUpgradeData.GetData(_constructionData.nextLevel);
            if (level1Data == null) return;

            actionButton.gameObject.SetActive(true);
            nextImage.gameObject.SetActive(true);
            arrowImage.SetActive(true);
            currentEffectGroup.SetActive(false); // 건설 시 '현재 효과'는 숨김
            nextEffectGroup.SetActive(true);

            currentLevelText.text = "빈 땅";
            nextLevelText.text = FormatBuildingName(level1Data);
            actionButtonText.text = "건설";
            nextImage.sprite = level1Data.buildingSprite; // 다음 이미지는 1레벨 건물 이미지
            UpdateCostText(_constructionData.costs);
            UpdateEffectText(null, level1Data);
        }
        else if (_mode == PanelMode.Upgrade)
        {
            BuildingUpgradeData currentData = _targetTile.GetBuildingData();

            if (_upgradeData == null) // 최대 레벨
            {
                currentLevelText.text = $"{FormatBuildingName(currentData)} (최대)";
                nextImage.gameObject.SetActive(false);
                nextLevelText.text = "";
                arrowImage.SetActive(false);
                actionButton.gameObject.SetActive(false);
                costText.text = "더 이상 업그레이드할 수 없습니다.";
                UpdateEffectText(currentData, null); // 현재 효과만 표시
                currentImage.sprite = currentData.buildingSprite; // 현재 건물 이미지만 표시
                nextImage.gameObject.SetActive(false); // 다음 이미지 UI는 숨김
            }
            else // 업그레이드 가능
            {
                actionButton.gameObject.SetActive(true);
                nextImage.gameObject.SetActive(true);
                arrowImage.SetActive(true);
                currentEffectGroup.SetActive(true);
                nextEffectGroup.SetActive(true);

                currentLevelText.text = FormatBuildingName(currentData);
                nextLevelText.text = FormatBuildingName(_upgradeData);
                actionButtonText.text = "업그레이드";

                currentImage.sprite = currentData.buildingSprite; // 현재 레벨 건물 이미지
                nextImage.sprite = _upgradeData.buildingSprite; // 다음 레벨 건물 

                UpdateCostText(currentData.costs);
                UpdateEffectText(currentData, _upgradeData);
            }
        }
        else if (_mode == PanelMode.Repair)
        {
            BuildingUpgradeData currentData = _targetTile.GetBuildingData();

            // 수리 UI 설정
            actionButton.gameObject.SetActive(true);
            nextImage.gameObject.SetActive(false);
            arrowImage.SetActive(false);
            currentEffectGroup.SetActive(false);
            nextEffectGroup.SetActive(false);

            currentImage.sprite = currentData.buildingSprite;
            currentLevelText.text = $"{FormatBuildingName(currentData)} \n (반파)";
            nextLevelText.text = "수리하시겠습니까?";
            actionButtonText.text = "자원 수리";

            UpdateRepairCostText(currentData);
        }
    }

    // --- 비용 텍스트 ---
    private void UpdateCostText(List<Cost> costs)
    {
        StringBuilder costSb = new StringBuilder("필요 자원:\n");
        bool canAffordAll = true;

        foreach (var cost in costs)
        {
            int originalCost = cost.amount;
            int finalCost = originalCost; // 최종 비용을 기본 비용으로 초기화

            //업그레이드 모드일 때만 할인율을 적용
            if (_mode == PanelMode.Upgrade)
            {
                float reductionPercent = 0f;
                switch (cost.resourceType)
                {
                    case ResourceType.Wood:
                        reductionPercent = PlayerDataManager.Instance.SynergyWoodCostReduction;
                        break;
                    case ResourceType.Iron:
                        reductionPercent = PlayerDataManager.Instance.SynergyIronCostReduction;
                        break;
                    case ResourceType.MagicStone:
                        reductionPercent = PlayerDataManager.Instance.SynergyMagicStoneCostReduction;
                        break;
                }

                // 할인율이 0보다 클 경우에만 최종 비용을 다시 계산
                if (reductionPercent > 0)
                {
                    finalCost = Mathf.CeilToInt(originalCost * (1.0f - reductionPercent / 100.0f));
                }
            }

            int playerAmount = PlayerDataManager.Instance.GetResourceAmount(cost.resourceType);
            bool hasEnough = playerAmount >= finalCost;
            if (!hasEnough)
            {
                canAffordAll = false;
            }

            string resourceName = GetResourceNameInKorean(cost.resourceType);

            // 할인된 가격과 원래 가격을 함께 표시 
            if (finalCost < originalCost)
            {
                costSb.AppendLine($"<color={(hasEnough ? "black" : "red")}>{resourceName}: {playerAmount}/{finalCost}</color>");
            }
            else
            {
                // 할인이 없으면 원래대로 표시
                costSb.AppendLine($"<color={(hasEnough ? "black" : "red")}>{resourceName}: {playerAmount}/{finalCost}</color>");
            }
        }

        costText.text = costSb.ToString();
        costText.richText = true; // 취소선, 색상 등 서식 적용을 위해 필수

        UpdateActionButtonState(canAffordAll);
    }

    private void UpdateRepairCostText(BuildingUpgradeData currentBuildingData)
    {
        // 현재 건물을 짓는데 들었던 '이전 레벨'의 데이터 찾기
        BuildingUpgradeData prevLevelData = DataManager.Instance.BuildingUpgradeData.Values
                                            .FirstOrDefault(data => data.nextLevel == currentBuildingData.idNumber);

        if (prevLevelData == null)
        {
            costText.text = "비용 정보 없음";
            UpdateActionButtonState(false);
            return;
        }

        //  50% 수리 비용을 계산하고 텍스트를 만듦
        StringBuilder costSb = new StringBuilder("수리 비용:\n");
        bool canAfford = true;

        foreach (var cost in prevLevelData.costs)
        {
            int requiredAmount = Mathf.CeilToInt(cost.amount * 0.5f);
            int playerAmount = PlayerDataManager.Instance.GetResourceAmount(cost.resourceType);
            bool enough = playerAmount >= requiredAmount;
            if (!enough) canAfford = false;

            string resourceName = GetResourceNameInKorean(cost.resourceType);
            costSb.AppendLine($"<color={(enough ? "black" : "red")}>{resourceName}: {playerAmount}/{requiredAmount}</color>");
        }

        costText.text = costSb.ToString();
        costText.richText = true;

        UpdateActionButtonState(canAfford);
    }

    private void UpdateActionButtonState(bool canAfford)
    {
        actionButton.interactable = canAfford;
        actionButtonText.color = canAfford ? Color.white : Color.red;
    }

    // --- 효과 텍스트 ---
    private void UpdateEffectText(BuildingUpgradeData current, BuildingUpgradeData next)
    {
        SetEffectText(current, currentEffectText, currentEffectGroup);
        SetEffectText(next, nextEffectText, nextEffectGroup);
    }

    private void SetEffectText(BuildingUpgradeData data, TextMeshProUGUI textUI, GameObject group)
    {
        if (data != null && data.effects.Count > 0)
        {
            group.SetActive(true);
            StringBuilder sb = new StringBuilder();
            foreach (var effect in data.effects)
                sb.AppendLine(FormatEffectString(effect));
            textUI.text = sb.ToString();
        }
        else
        {
            group.SetActive(false);
        }
    }

    // --- 액션 버튼 ---
    //원래 async 있는 버튼 쪽에 try catch finally를 다는데, 여기선 건설 함수 내부에서 예외처리를 해줘서 생략
    private async UniTaskVoid OnActionButtonClick()
    {
        if (_targetTile == null) return;

        actionButton.interactable = false;
        if (_mode == PanelMode.Construction && _constructionData != null)
        {
            await MainScreenBuildingController.Instance.BuildBuildingOnTile(_targetTile, _constructionData.idNumber);
        }
        else if (_mode == PanelMode.Upgrade)
        {
            await MainScreenBuildingController.Instance.UpgradeBuildingOnTile(_targetTile);
        }
        else if (_mode == PanelMode.Repair)
        {
            await MainScreenBuildingController.Instance.RepairBuildingOnTile(_targetTile);
        }

        CloseUI();
        actionButton.interactable = true;

    }
    private void OnDestroyButtonClicked()
    {
        if (_targetTile != null)
        {
            gameObject.SetActive(false);
            MainScreenBuildingController.Instance.InitiateDestruction(_targetTile);

            MainScreenBuildingController.Instance.DeselectTile();
            _targetTile = null;
        }
    }
    private void OnAdRepairButtonClicked()
    {
        if (_targetTile == null || _mode != PanelMode.Repair) return;

        Debug.Log("광고 보고 즉시 수리 버튼 클릭됨");

        AdManager.Instance.ShowRewardedAd(() => {
            // --- 광고 시청 성공 시 ---
            Debug.Log("광고 시청 성공! 즉시 수리를 실행합니다.");

            // 2. 컨트롤러에게 '광고용 즉시 수리'를 요청
            MainScreenBuildingController.Instance.RepairBuildingWithAd(_targetTile);

            // 3. 패널 닫기
            CloseUI();
        });
    }
    // --- 헬퍼 함수 ---
    private string FormatBuildingName(BuildingUpgradeData data)
        => $"{data.buildingName} Lv.{data.level}";

    private string FormatEffectString(BuildingEffect effect)
    {
        string effectName = GetEffectNameInKorean(effect.effectType);
        string valueString = GetEffectValueString(effect);
        return $"{effectName}: +{valueString}";
    }

    public static readonly Dictionary<ResourceType, string> ResourceNames = new()
    {
        { ResourceType.Gold, "골드" },
        { ResourceType.Food, "식량" },
        { ResourceType.Wood, "목재" },
        { ResourceType.Iron, "철괴" },
        { ResourceType.MagicStone, "마력석" }
    };

    public static string GetResourceNameInKorean(ResourceType type)
        => ResourceNames.TryGetValue(type, out var name) ? name : type.ToString();


    // ===== 건물 효과 값 문자열 변환 =====
    // effectType에 따라 표시 방식을 다르게 처리
    // 1) 증가율(%) 효과: 퍼센트 기호 붙여서 표시
    // 2) 고정값 효과: Min과 Max가 같으면 단일값 표시
    // 3) 범위 효과: Min과 Max가 다르면 "Min~Max" 형식으로 표시

    private string GetEffectValueString(BuildingEffect effect)
      => effect.effectType switch
      {
          BuildingEffectType.IncreaseFoodGainSpeed or
          BuildingEffectType.AdditionalWoodProduction or
          BuildingEffectType.AdditionalIronProduction or
          BuildingEffectType.MagicStoneFindChance or
          BuildingEffectType.UnitCoolDown
          => $"{effect.effectValueMin}%",
          _ => effect.effectValueMin == effect.effectValueMax
                  ? effect.effectValueMin.ToString()
                  : $"{effect.effectValueMin}~{effect.effectValueMax}"
      };
    private static readonly Dictionary<BuildingEffectType, string> EffectNames = new()
{
    { BuildingEffectType.MaximumFood, "식량 최대 저장량" },
    { BuildingEffectType.IncreaseFoodGainSpeed, "식량 획득 속도" },
    { BuildingEffectType.BaseWoodProduction, "기본 목재 획득량" },
    { BuildingEffectType.AdditionalWoodProduction, "추가 목재 획득량" },
    { BuildingEffectType.BaseIronProduction, "기본 철괴 획득량" },
    { BuildingEffectType.AdditionalIronProduction, "추가 철괴 획득량" },
    { BuildingEffectType.UnitCoolDown, "유닛의 쿨타임 감소" },
    { BuildingEffectType.MagicStoneFindChance, "마력석 발견 확률" },
    { BuildingEffectType.MagicStoneProduction, "마력석 획득량" },
    { BuildingEffectType.CanSummonRareUnits, "레어 유닛 소환 가능" },
    { BuildingEffectType.CanSummonEpicUnits, "에픽 유닛 소환 가능" },
    { BuildingEffectType.None, "효과 없음" }
};

    private string GetEffectNameInKorean(BuildingEffectType type) // 딕셔너리에서 한글 이름을 가져오는 헬퍼 메서드
        => EffectNames.TryGetValue(type, out var name) ? name : type.ToString();
}
