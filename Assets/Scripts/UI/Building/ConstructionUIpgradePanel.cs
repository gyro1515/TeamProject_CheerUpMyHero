using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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
            UpdatePanelContents();
        }
    }

    // --- 건설 초기화 ---
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
        if (_dataToShow == null)
        {
            costText.text = "데이터 없음";
            actionButtonText.text = _mode == PanelMode.Construction ? "건설" : "업그레이드";
            actionButton.interactable = false;
            actionButtonText.color = Color.red;
            return;
        }

        // --- description ---
        descriptionText.text = _dataToShow.description;

        // --- cost text ---
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("필요 자원:");

        bool canAfford = true;

        foreach (var cost in _dataToShow.costs)
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
