using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingTile : MonoBehaviour
{
    public int X { get; private set; }
    public int Y { get; private set; }

    [SerializeField] private BuildingUpgradeData _buildingData;
    private Image _image;

    // BuildingManager가 타일을 생성할 때 호출해 줄 초기화 함수
    public void Initialize(int x, int y)
    {
        X = x;
        Y = y;
        _image = GetComponent<Image>();
        GetComponent<Button>().onClick.AddListener(OnTileClick);
    }

    // 타일이 클릭되면 BuildingManager에게 자신을 알림
    private void OnTileClick()
    {
        BuildingManager.Instance.HandleTileClick(this);
    }

    // 건물이 건설/업그레이드되면 이 함수를 호출해서 타일의 모양과 데이터를 바꿈
    public void SetBuilding(BuildingUpgradeData buildingData)
    {
        _buildingData = buildingData;
        // _image.sprite = ... // 데이터에 있는 건물 이미지로 교체하는 로직
    }

    public BuildingUpgradeData GetBuildingData()
    {
        return _buildingData;
    }
}
