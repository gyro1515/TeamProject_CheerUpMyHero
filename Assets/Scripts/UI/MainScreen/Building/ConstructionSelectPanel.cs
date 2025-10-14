using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionSelectPanel : BasePopUpUI
{
    [SerializeField] private GameObject buildingSelectItemPrefab;
    [SerializeField] private Transform contentParent;

    private BuildingTile _targetTile;
    
    public void Initialize(BuildingTile tile, ConstructionUpgradePanel upgradePanel)
    {
        _targetTile = tile;

        // 1. 기존에 있던 버튼들을 모두 삭제
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        List<BuildingUpgradeData> buildableList = PlayerDataManager.Instance.GetBuildableList();

        // 3. 각 건물 데이터에 해당하는 버튼을 생성하여 Content 자식으로 추가
        foreach (BuildingUpgradeData data in buildableList)
        {
            GameObject itemGO = Instantiate(buildingSelectItemPrefab, contentParent);
            BuildingSelectItem item = itemGO.GetComponent<BuildingSelectItem>();
            // data.idNumber를 사용하여 버튼을 초기화
            item.Initialize(data.idNumber, _targetTile, this, upgradePanel); 
        }
    }
    public override void CloseUI()
    {
        base.CloseUI();
        if (_targetTile != null)
        {
            MainScreenBuildingController.Instance.DeselectTile();
            _targetTile = null; 
        }
    }
}