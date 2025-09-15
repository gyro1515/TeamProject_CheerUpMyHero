using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingTile : MonoBehaviour
{
    [SerializeField] public int x, y; 

    // 이 타일에 건설된 건물의 데이터 (비어있으면 null)
    [SerializeField] private BuildingUpgradeData _buildingData;
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnTileClick);
    }

    // 타일이 클릭되면 BuildingManager에게 알림
    private void OnTileClick()
    {
        BuildingManager.Instance.HandleTileClick(this, _buildingData);
    }

    public void SetBuilding(BuildingUpgradeData buildingData)
    {
        _buildingData = buildingData;
        // GetComponent<Image>().sprite = ... // 건물 이미지로 교체
    }
}
