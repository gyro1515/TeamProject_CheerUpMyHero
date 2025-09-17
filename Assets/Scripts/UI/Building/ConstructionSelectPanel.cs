using System.Collections.Generic;
using UnityEngine;

public class ConstructionSelectPanel : BaseUI
{
    [SerializeField] private GameObject buildingSelectItemPrefab;
    [SerializeField] private Transform contentParent;

    private BuildingTile _targetTile;

    // 패널이 열릴 때 호출되는 함수
    public void Initialize(BuildingTile tile)
    {
        _targetTile = tile;

        //// 1. 기존에 있던 버튼들을 모두 삭제
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. 건설 가능한 모든 건물의 ID 목록을 가져오기 (임시)
        List<int> buildableIDs = new List<int> { 101, 201, 301, 401 };

        // 3. 각 ID에 해당하는 건물의 버튼을 생성하여 Content 자식으로 추가
        foreach (int id in buildableIDs)
        {
            GameObject itemGO = Instantiate(buildingSelectItemPrefab, contentParent);
            BuildingSelectItem item = itemGO.GetComponent<BuildingSelectItem>();
            item.Initialize(id, _targetTile);
        }
    }
}