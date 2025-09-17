using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TileType
{
    Normal,    // 일반 영지
    Special,   // 특수 영지
    None
}

public class BuildingTile : MonoBehaviour
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public TileType MyTileType { get; private set; } 


    private BuildingUpgradeData _buildingData;
    private Image _image;

    private Color _emptyColor = Color.white;      // 빈 땅일 때의 기본 색상
    private Color _builtColor = Color.cyan;       // 건물이 지어졌을 때의 색상
    private Color _selectedColor = Color.yellow;  // 선택됐을 때의 색상



    // BuildingManager가 타일을 생성할 때 호출해 줄 초기화 함수
    public void Initialize(int x, int y)
    {
        X = x;
        Y = y;

        MyTileType = TileType.Normal;


        if (x == 4 && (y == 0 || y == 1 || y == 2 || y == 3))
        {
            MyTileType = TileType.Special;
        }
        //아래쪽 맨 밑 5칸 (0,4), (1,4), (2,4), (3,4), (4,4)
        if (y == 4)
        {
            MyTileType = TileType.Special;
        }
        if (MyTileType == TileType.Special)
        {
            GetComponent<Image>().color = Color.gray;
        }

        _image = GetComponent<Image>();
        GetComponent<Button>().onClick.AddListener(OnTileClick);
        _image.color = _emptyColor;

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

        _image.color = (buildingData != null) ? _builtColor : _emptyColor;


        // _image.sprite = ... // 데이터에 있는 건물 이미지로 교체하는 로직
    }

    public BuildingUpgradeData GetBuildingData()
    {
        return _buildingData;
    }
    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            // 선택되면 무조건 선택 색상으로 변경
            _image.color = _selectedColor;
        }
        else
        {
            // 선택이 해제되면, 건물이 있는지 없는지에 따라 원래 색으로 복원
            _image.color = (_buildingData != null) ? _builtColor : _emptyColor;
        }

    }
}
