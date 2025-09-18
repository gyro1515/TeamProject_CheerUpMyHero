using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConstructionUpgradePanel : BaseUI
{
    [Header("UI 요소 연결")]
    // [SerializeField] private TextMeshProUGUI titleText;  삭제
    //[SerializeField] private TextMeshProUGUI descriptionText; 삭제
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
    [SerializeField] private GameObject arrowImage; // 업그레이드 화살표

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

        BuildingUpgradeData currentData = PlayerDataManager.Instance.BuildingGridData[tile.X, tile.Y];
        if (currentData == null)
        {
            Debug.LogError("업그레이드할 건물이 없습니다.");
            return;
        }

        _dataToShow = DataManager.Instance.BuildingUpgradeData.GetData(currentData.nextLevel);

        if (_dataToShow == null) // 최대 레벨
        {
            currentLevelText.text = $"{currentData.buildingName} (최대 레벨)"; // 현재 레벨 텍스트에 표시
            actionButton.gameObject.SetActive(false);
            costText.text = "더 이상 업그레이드할 수 없습니다.";
            // descriptionText.text = currentData.description; 사용x
            effectText.text = "";
        }
        else
        {
            actionButton.gameObject.SetActive(true);
            arrowImage.SetActive(true);
            currentLevelText.text = $"{currentData.buildingName} Lv.{currentData.level}";
            nextLevelText.text = $"Lv.{_dataToShow.level}";
            actionButtonText.text = "업그레이드";
            UpdatePanelContents(currentData.costs, _dataToShow.effects);
        }
    }

    // --- 건설 초기화 ---
    public void InitializeForConstruction(BuildingTile tile, int buildingBaseID)
    {
        _targetTile = tile;
        _mode = PanelMode.Construction; 

        // 건설 비용을 위해 0레벨 데이터를 가져옴
        BuildingUpgradeData constructionData = DataManager.Instance.BuildingUpgradeData.GetData(buildingBaseID);
        if (constructionData == null) return;
        _dataToShow = constructionData;

        // 건설 후 적용될 효과를 위해 1레벨 데이터를 미리 가져옴
        BuildingUpgradeData level1Data = DataManager.Instance.BuildingUpgradeData.GetData(constructionData.nextLevel);
        if (level1Data == null) return;

        actionButton.gameObject.SetActive(true);
        arrowImage.SetActive(true);
        currentLevelText.text = "빈 땅";
        nextLevelText.text = $"{level1Data.buildingName} Lv.{level1Data.level}";
        actionButtonText.text = "건설";

        // 비용은 0레벨 데이터, 효과는 1레벨 데이터를 사용해서 UI를 채움
        UpdatePanelContents(constructionData.costs, level1Data.effects);
    }


    private void UpdatePanelContents(List<Cost> costs, List<BuildingEffect> effects)
    {
        // --- cost text ---
        StringBuilder costSb = new StringBuilder("필요 자원:\n");
        bool canAffordOverall = true; 

        foreach (var cost in costs)
        {
            int playerAmount = PlayerDataManager.Instance.GetResourceAmount(cost.resourceType);
            int requiredAmount = cost.amount;

            // 현재 자원이 이 비용을 감당할 수 있는지 확인
            bool hasEnoughForThisCost = playerAmount >= requiredAmount;
            Debug.Log($"[자원 확인] 타입:{cost.resourceType}, 보유:{playerAmount}, 필요:{requiredAmount}");


            //하나라도 부족한 자원이 있으면, 전체 업그레이드는 불가능
            if (!hasEnoughForThisCost)
            {
                canAffordOverall = false;
            }

            //Rich Text 형식으로 표시할 때, 개별 자원의 충족 여부(hasEnoughForThisCost)를 사용
            costSb.AppendLine($"<color={(hasEnoughForThisCost ? "black" : "red")}>{cost.resourceType}: {playerAmount}/{cost.amount}</color>");
        }

        costText.text = costSb.ToString();
        costText.richText = true; // Rich Text 기능이 켜져 있는지 확인


        // --- effect text ---
        StringBuilder effectSb = new StringBuilder("적용 효과:\n");
        foreach (BuildingEffect effect in effects)
        {
            string effectName = GetEffectNameInKorean(effect.effectType);
            string valueString = GetEffectValueString(effect);
            effectSb.AppendLine($"{effectName}: +{valueString}");
        }
        effectText.text = effectSb.ToString();

        // --- 버튼 ---
        actionButton.interactable = canAffordOverall;
        actionButtonText.color = canAffordOverall ? Color.black : Color.red;
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
