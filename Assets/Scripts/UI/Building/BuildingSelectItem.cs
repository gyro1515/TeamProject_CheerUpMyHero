using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingSelectItem : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    [SerializeField] private Image buildingImage;
    [SerializeField] private TextMeshProUGUI buildingNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI descriptionText;



    private int _buildingID;
    private BuildingTile _targetTile;

    public void Initialize(int buildingID, BuildingTile targetTile)
    {
        _buildingID = buildingID;
        _targetTile = targetTile;

        // DataManager에서 건물 정보를 가져와 UI를 채웁니다.
        BuildingUpgradeData data = DataManager.Instance.BuildingUpgradeData.GetData(buildingID);
        // buildingImage.sprite = ... 건물 이미지
        buildingNameText.text = data.buildingName;

        string costStr = "";
        foreach (Cost cost in data.costs)
        {
            costStr += $"{cost.resourceType}: {cost.amount} ";
        }
        costText.text = costStr;

        descriptionText.text = data.description;

        selectButton.onClick.AddListener(OnSelect);
    }

    private void OnSelect()
    {
        // 건설/업그레이드 확인 패널을 엽니다.
        var panel = UIManager.Instance.GetUI<ConstructionUpgradePanel>();
        panel.InitializeForConstruction(_targetTile, _buildingID);
        panel.OpenUI();
    }
}