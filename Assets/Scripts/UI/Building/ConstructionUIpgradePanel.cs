using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConstructionUpgradePanel : BaseUI
{
    [Header("UI 요소 연결")]
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI effectText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [SerializeField] private Button closeButton;

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
    private CanvasGroup _canvasGroup;

    private enum PanelMode { None, Construction, Upgrade }
    private PanelMode _mode = PanelMode.None;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        actionButton.onClick.AddListener(OnActionButtonClick);
        closeButton.onClick.AddListener(() => CloseUI());
    }

    // --- 업그레이드 초기화 ---
    public void InitializeForUpgrade(BuildingTile tile)
    {
        _targetTile = tile;
        _mode = PanelMode.Upgrade;

        BuildingUpgradeData currentData = PlayerDataManager.Instance.BuildingGridData[tile.X, tile.Y];
        if (currentData == null) return;

        _upgradeData = DataManager.Instance.BuildingUpgradeData.GetData(currentData.nextLevel);

        UpdatePanelContents();
    }

    // --- 건설 초기화 ---
    public void InitializeForConstruction(BuildingTile tile, int buildingBaseID)
    {
        _targetTile = tile;
        _mode = PanelMode.Construction;

        _constructionData = DataManager.Instance.BuildingUpgradeData.GetData(buildingBaseID);

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

                UpdateCostText(currentData.costs);
                UpdateEffectText(currentData, _upgradeData);
            }
        }
    }

    // --- 비용 텍스트 ---
    private void UpdateCostText(List<Cost> costs)
    {
        StringBuilder costSb = new StringBuilder("필요 자원:\n");
        bool canAfford = true;

        foreach (var cost in costs)
        {
            int playerAmount = PlayerDataManager.Instance.GetResourceAmount(cost.resourceType);
            bool enough = playerAmount >= cost.amount;
            if (!enough) canAfford = false;

            string resourceName = GetResourceNameInKorean(cost.resourceType);
            costSb.AppendLine($"<color={(enough ? "black" : "red")}>{resourceName}: {playerAmount}/{cost.amount}</color>");
        }

        costText.text = costSb.ToString();
        costText.richText = true;

        UpdateActionButtonState(canAfford);
    }

    private void UpdateActionButtonState(bool canAfford)
    {
        actionButton.interactable = canAfford;
        actionButtonText.color = canAfford ? Color.black : Color.red;
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
    private void OnActionButtonClick()
    {
        if (_targetTile == null) return;

        if (_mode == PanelMode.Construction && _constructionData != null)
        {
            BuildingManager.Instance.BuildBuildingOnTile(_targetTile, _constructionData.idNumber);
        }
        else if (_mode == PanelMode.Upgrade)
        {
            BuildingManager.Instance.UpgradeBuildingOnTile(_targetTile);
        }

        CloseUI();
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

    private static readonly Dictionary<ResourceType, string> ResourceNames = new()
    {
        { ResourceType.Gold, "골드" },
        { ResourceType.Food, "식량" },
        { ResourceType.Wood, "목재" },
        { ResourceType.Iron, "철" },
        { ResourceType.MagicStone, "마력석" }
    };

    private string GetResourceNameInKorean(ResourceType type)                                                          
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
         BuildingEffectType.MagicStoneFindChance => $"{effect.effectValueMin}%",
         _ => effect.effectValueMin == effect.effectValueMax
             ? effect.effectValueMin.ToString()
             : $"{effect.effectValueMin}~{effect.effectValueMax}"
     };
    private static readonly Dictionary<BuildingEffectType, string> EffectNames = new()
{
    { BuildingEffectType.IncreaseFoodGainSpeed, "식량 획득 속도" },
    { BuildingEffectType.BaseWoodProduction, "기본 목재 획득량" },
    { BuildingEffectType.AdditionalWoodProduction, "추가 목재 획득량" },
    { BuildingEffectType.BaseIronProduction, "기본 철괴 획득량" },
    { BuildingEffectType.AdditionalIronProduction, "추가 철괴 획득량" },
    { BuildingEffectType.MaxPopulation, "최대 인구 수" },
    { BuildingEffectType.MagicStoneFindChance, "마력석 발견 확률" },
    { BuildingEffectType.MagicStoneProduction, "마력석 획득량" },
    { BuildingEffectType.CanSummonRareUnits, "레어 유닛 소환 가능" },
    { BuildingEffectType.CanSummonEpicUnits, "에픽 유닛 소환 가능" },
    { BuildingEffectType.None, "효과 없음" }
};

    private string GetEffectNameInKorean(BuildingEffectType type) // 딕셔너리에서 한글 이름을 가져오는 헬퍼 메서드
        => EffectNames.TryGetValue(type, out var name) ? name : type.ToString();

    // --- 애니메이션 ---
    public override void OpenUI()
    {
        base.OpenUI();
        FadeEffectManager.Instance.FadeInUI(_canvasGroup);
    }
    public override void CloseUI()
    {
        FadeEffectManager.Instance.FadeOutUI(_canvasGroup);
        StartCoroutine(CoCloseAfterDelay(0.3f));
    }

    private IEnumerator CoCloseAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        base.CloseUI();
    }
}
