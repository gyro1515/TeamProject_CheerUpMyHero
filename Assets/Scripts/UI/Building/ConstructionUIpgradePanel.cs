using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class ConstructionUpgradePanel : BaseUI
{
    [Header("UI 요소 연결")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [SerializeField] private Button closeButton;

    private BuildingTile _targetTile;
    private BuildingUpgradeData _dataToShow;
    private CanvasGroup _canvasGroup;

    // 🔹 모드 추가: 건설/업그레이드 구분
    private enum PanelMode { None, Construction, Upgrade }
    private PanelMode _mode = PanelMode.None;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        actionButton.onClick.AddListener(OnActionButtonClick);
        closeButton.onClick.AddListener(() => CloseUI());
    }

    // --- 업그레이드용 초기화 ---
    public void InitializeForUpgrade(BuildingTile tile)
    {
        _targetTile = tile;
        _mode = PanelMode.Upgrade;

        BuildingUpgradeData currentData = DataManager.Instance.BuildingGridData[tile.X, tile.Y];
        if (currentData == null) return;

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
            UpdatePanelContents();
        }
    }

    // --- 건설용 초기화 ---
    public void InitializeForConstruction(BuildingTile tile, int buildingBaseID)
    {
        _targetTile = tile;
        _mode = PanelMode.Construction;

        _dataToShow = DataManager.Instance.BuildingUpgradeData.GetData(buildingBaseID);
        if (_dataToShow == null) return;

        actionButton.gameObject.SetActive(true);
        titleText.text = $"{_dataToShow.buildingName} 건설";
        actionButtonText.text = "건설";
        UpdatePanelContents();
    }

    private void UpdatePanelContents()
    {
        descriptionText.text = _dataToShow.description;

        string costStr = "필요 자원:\n";
        bool canAfford = true;
        foreach (Cost cost in _dataToShow.costs)
        {
            costStr += $"{cost.resourceType}: {cost.amount}\n";
            if (PlayerDataManager.Instance.GetResourceAmount(cost.resourceType) < cost.amount)
            {
                canAfford = false;
            }
        }
        costText.text = costStr;
        actionButton.interactable = canAfford;
    }

    private void OnActionButtonClick()
    {
        Debug.Log($"ActionButton clicked. TargetTile: {_targetTile}, DataToShow: {_dataToShow}");

        if (_mode == PanelMode.Construction)
        {
            BuildingManager.Instance.BuildBuildingOnTile(_targetTile, _dataToShow.idNumber);
        }
        else if (_mode == PanelMode.Upgrade)
        {
            BuildingManager.Instance.UpgradeBuildingOnTile(_targetTile);
        }
    }

    // --- DOTween 애니메이션 ---
    public override void OpenUI()
    {
        base.OpenUI();
        _canvasGroup.alpha = 0;
        transform.localScale = Vector3.one * 0.8f;

        _canvasGroup.interactable = false;
        _canvasGroup.DOFade(1, 0.3f).SetUpdate(true)
            .OnComplete(() => _canvasGroup.interactable = true);
        transform.DOScale(1, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public override void CloseUI()
    {
        _canvasGroup.interactable = false;
        _canvasGroup.DOFade(0, 0.2f).SetUpdate(true);
        transform.DOScale(0.8f, 0.2f).SetUpdate(true)
            .OnComplete(() => base.CloseUI());
    }
}
