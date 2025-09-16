using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class ConstructionUpgradePanel : BaseUI
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private BuildingTile _targetTile;
    private BuildingUpgradeData _dataToShow; // 건설 또는 다음 레벨 업그레이드 데이터
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        actionButton.onClick.AddListener(OnActionButtonClick);
    }

    public void Initialize(BuildingTile tile)
    {
        _targetTile = tile;
        BuildingUpgradeData currentData = DataManager.Instance.BuildingGridData[tile.X, tile.Y];

        if (currentData == null) // 건설
        {
            _dataToShow = DataManager.Instance.BuildingUpgradeData.GetData(201); // 임시로 벌목장 건설 ID
            titleText.text = $"{_dataToShow.buildingName} 건설";
            actionButtonText.text = "건설";
        }
        else // 업그레이드
        {
            _dataToShow = DataManager.Instance.BuildingUpgradeData.GetData(currentData.nextLevel);
            titleText.text = $"{currentData.buildingName} Lv.{currentData.level} → Lv.{_dataToShow.level}";
            actionButtonText.text = "업그레이드";
        }

        UpdatePanelContents();
    }

    private void UpdatePanelContents()
    {
        if (_dataToShow == null) { /* 최대 레벨 처리 */ return; }

        descriptionText.text = _dataToShow.description;

        string costStr = "필요 자원:\n";
        bool canAfford = true;
        foreach (Cost cost in _dataToShow.costs)
        {
            costStr += $"{cost.resourceType}: {cost.amount}\n";
            if (ResourceManager.Instance.GetResourceAmount(cost.resourceType) < cost.amount)
                canAfford = false;
        }
        costText.text = costStr;
        actionButton.interactable = canAfford;
    }

    private void OnActionButtonClick()
    {
        BuildingUpgradeData currentData = _targetTile.GetBuildingData();
        if (currentData == null)
            BuildingManager.Instance.BuildBuildingOnTile(_targetTile, _dataToShow.idNumber);
        else
           // BuildingManager.Instance.UpgradeBuildingOnTile(_targetTile);

        CloseUI();
    }

    public override void OpenUI()
    {
        base.OpenUI();
        _canvasGroup.alpha = 0;
        _canvasGroup.DOFade(1, 0.3f).SetUpdate(true);
    }
    public override void CloseUI()
    {
        _canvasGroup.DOFade(0, 0.2f).OnComplete(() => base.CloseUI()).SetUpdate(true);
    }
}