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
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI effectText;

    private BuildingTile _targetTile;
    private BuildingUpgradeData _dataToShow;
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

        BuildingUpgradeData currentData = DataManager.Instance.BuildingGridData[tile.X, tile.Y];
        if (currentData == null)
        {
            Debug.LogError("업그레이드할 건물이 없습니다.");
            return;
        }

        _dataToShow = DataManager.Instance.BuildingUpgradeData.GetData(currentData.nextLevel);

        if (_dataToShow == null) // 최대 레벨
        {
            titleText.text = $"{currentData.buildingName} (최대 레벨)";
            actionButton.gameObject.SetActive(false);
            costText.text = "더 이상 업그레이드할 수 없습니다.";
            descriptionText.text = currentData.description;
        }
        else
        {
            actionButton.gameObject.SetActive(true);
            titleText.text = $"{currentData.buildingName} Lv.{currentData.level} → Lv.{_dataToShow.level}";
            actionButtonText.text = "업그레이드";
            UpdatePanelContents(currentData.costs, _dataToShow.effects, currentData.description);
        }
    }

    // --- 건설 초기화 ---
    public void InitializeForConstruction(BuildingTile tile, int buildingBaseID)
    {
        _targetTile = tile;
        _mode = PanelMode.Construction; 

        // 건설 비용을 위해 0레벨 데이터를 가져옵니다.
        BuildingUpgradeData constructionData = DataManager.Instance.BuildingUpgradeData.GetData(buildingBaseID);
        if (constructionData == null) return;
        _dataToShow = constructionData;

        // 건설 후 적용될 효과를 위해 1레벨 데이터를 미리 가져옵니다.
        BuildingUpgradeData level1Data = DataManager.Instance.BuildingUpgradeData.GetData(constructionData.nextLevel);
        if (level1Data == null) return;

        actionButton.gameObject.SetActive(true);
        titleText.text = $"{constructionData.buildingName} 건설";
        actionButtonText.text = "건설";

        // 비용은 0레벨 데이터, 효과는 1레벨 데이터를 사용해서 UI를 채웁니다.
        UpdatePanelContents(constructionData.costs, level1Data.effects, constructionData.description);
    }


    private void UpdatePanelContents(List<Cost> costs, List<BuildingEffect> effects, string description)
    {
        // --- description ---
        descriptionText.text = description;

        // --- cost text ---
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("업그레이드 시 필요 자원:");

        StringBuilder effectSb = new StringBuilder();
        effectSb.AppendLine("적용 효과:");
        foreach (BuildingEffect effect in effects)
        {
            string effectName = GetEffectNameInKorean(effect.effectType);

            // GetEffectValueString 함수를 새로 만들어서 호출
            string valueString = GetEffectValueString(effect);

            effectSb.AppendLine($"{effectName}: +{valueString}");
        }
        effectText.text = effectSb.ToString();
    



        bool canAfford = true;

        foreach (var cost in costs)
        {
            int playerAmount = PlayerDataManager.Instance.GetResourceAmount(cost.resourceType);
            bool enough = playerAmount >= cost.amount;
            if (!enough) canAfford = false;

            // Rich Text 형식으로 표시
            sb.AppendLine($"<color={(enough ? "black" : "red")}>{cost.resourceType}: {playerAmount}/{cost.amount}</color>");
        }

        costText.text = sb.ToString();
        costText.richText = true; // Rich Text 켜기

        // --- 버튼 ---
        actionButtonText.text = _mode == PanelMode.Construction ? "건설" : "업그레이드";
        actionButton.interactable = canAfford;
        actionButtonText.color = canAfford ? Color.black : Color.red;
    }

    private string GetEffectValueString(BuildingEffect effect)
    {
        // 비율(%)로 표시해야 할 효과 타입들을 여기에 추가
        switch (effect.effectType)
        {
            case BuildingEffectType.IncreaseFoodGainSpeed:
            case BuildingEffectType.AdditionalWoodProduction:
            case BuildingEffectType.AdditionalIronProduction:
            case BuildingEffectType.MagicStoneFindChance:
                // Min과 Max 값이 같으므로 Min 값만 사용하고 뒤에 '%'를 붙임
                return $"{effect.effectValueMin}%";

            default: // 그 외 모든 효과 (고정 값 또는 범위 값)
                if (effect.effectValueMin == effect.effectValueMax)
                {
                    return effect.effectValueMin.ToString(); // 고정 값
                }
                else
                {
                    return $"{effect.effectValueMin}~{effect.effectValueMax}"; // 범위 값
                }
        }
    }

    private string GetEffectNameInKorean(BuildingEffectType type)
    {
        switch (type)
        {
            case BuildingEffectType.IncreaseMaxFood:
                return "최대 식량";
            case BuildingEffectType.IncreaseFoodGainSpeed:
                return "식량 획득 속도";
            case BuildingEffectType.BaseWoodProduction:
                return "기본 목재 획득량";
            case BuildingEffectType.AdditionalWoodProduction:
                return "추가 목재 획득량";
            case BuildingEffectType.BaseIronProduction:
                return "기본 철괴 획득량";
            case BuildingEffectType.AdditionalIronProduction:
                return "추가 철괴 획득량";
            case BuildingEffectType.MaxPopulation:
                return "최대 인구 수";
            case BuildingEffectType.MagicStoneFindChance:
                return "마력석 발견 확률";
            case BuildingEffectType.MagicStoneProduction:
                return "마력석 획득량";
            case BuildingEffectType.CanSummonRareUnits:
                return "레어 유닛 소환 가능";
            case BuildingEffectType.CanSummonEpicUnits:
                return "에픽 유닛 소환 가능";
            case BuildingEffectType.None:
                return "효과 없음";
            default:
                //목록에 없는 타입이 들어오면 원래 영어 이름을 보여줌 (안전장치)
                return type.ToString();
        }
    }
    private void OnActionButtonClick()
    {
        if (_targetTile == null || _dataToShow == null) return;

        switch (_mode)
        {
            case PanelMode.Construction:
                BuildingManager.Instance.BuildBuildingOnTile(_targetTile, _dataToShow.idNumber);
                break;
            case PanelMode.Upgrade:
                BuildingManager.Instance.UpgradeBuildingOnTile(_targetTile);
                break;
        }

        CloseUI();
    }

    public override void OpenUI()
    {
        base.OpenUI();

        FadeInUI(_canvasGroup);
    }
    public override void CloseUI()
    {
        FadeOutUI(_canvasGroup);

        StartCoroutine(CoCloseAfterDelay(0.3f));
    }

    private IEnumerator CoCloseAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        base.CloseUI();
    }
}
