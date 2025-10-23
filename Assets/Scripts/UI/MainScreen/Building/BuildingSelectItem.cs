using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingSelectItem : BaseUI
{
    [SerializeField] private Button selectButton;
    [SerializeField] private Button descriptionSelectButton;
    [SerializeField] private Image buildingImage;
    //[SerializeField] private TextMeshProUGUI buildingNameText;
    //[SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    private ConstructionSelectPanel _parentPanel;
    private ConstructionUpgradePanel _upgradePanel;



    private int _buildingID;
    private BuildingTile _targetTile;

    public void Initialize(int buildingID, BuildingTile targetTile, ConstructionSelectPanel parent, ConstructionUpgradePanel upgradePanel)
    {
        _buildingID = buildingID;
        _targetTile = targetTile;
        _parentPanel = parent;
        _upgradePanel = upgradePanel;

        BuildingUpgradeData constructionData = DataManager.Instance.BuildingUpgradeData.GetData(buildingID);

        BuildingUpgradeData level1Data = DataManager.Instance.BuildingUpgradeData.GetData(constructionData.nextLevel);

        //buildingNameText.text = constructionData.buildingName;
        descriptionText.text = constructionData.description;

        //string costStr = "";
        //foreach (Cost cost in constructionData.costs)
        //{
        //    costStr += $"{cost.resourceType}: {cost.amount} ";
        //}
        //costText.text = costStr;

        if (level1Data != null && level1Data.buildingSprite != null)
        {
            buildingImage.sprite = level1Data.buildingSprite;
        }
        else
        {
            // 1레벨 데이터나 이미지가 없을 경우를 대비한 예외 처리
            buildingImage.gameObject.SetActive(false);
        }

        selectButton.onClick.AddListener(OnSelect);
        descriptionSelectButton.onClick.AddListener(OnSelect);
    }

    private void OnSelect()
    {
        //_parentPanel?.CloseUI(); // 닫지 말기
        // 닫을거면 바로 닫기
        _parentPanel.gameObject.SetActive(false);

        // 건설/업그레이드 확인 패널을 엽니다.
        _upgradePanel.InitializeForConstruction(_targetTile, _buildingID);
        _upgradePanel.OpenUI();
    }
}