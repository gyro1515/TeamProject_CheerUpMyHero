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

    public event System.Action<BuildingTile> OnTileClicked;

    private BuildingUpgradeData _buildingData;
    [Header("타일 이미지 설정")]
    [SerializeField] private Image tileImage; // 타일의 이미지를 표시할 Image 컴포넌트
    [SerializeField] public Sprite emptyTileSprite; // 건물이 없을 때 표시할 기본 빈 타일 이미지
   


    // BuildingManager가 타일을 생성할 때 호출해 줄 초기화 함수
    public void Initialize(int x, int y)
    {
        X = x;
        Y = y;

        MyTileType = TileType.Normal;
        tileImage.sprite = emptyTileSprite;


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
        GetComponent<Button>().onClick.AddListener(OnTileClick);

    }


    private void OnTileClick()
    {
        OnTileClicked?.Invoke(this);
    }

    // 건물이 건설/업그레이드되면 이 함수를 호출해서 타일의 모양과 데이터를 바꿈
    public void SetBuilding(BuildingUpgradeData buildingData)
    {
        _buildingData = buildingData;

        if (buildingData == null)
        {
            // 데이터가 null이면, 빈 타일 이미지로 되돌립니다.
            tileImage.sprite = emptyTileSprite;
        }
        else
        {
            if (buildingData.buildingSprite != null)
            {
                tileImage.sprite = buildingData.buildingSprite;
            }
            else
            {
                // 만약 데이터에 이미지가 없다면, 기본 빈 타일로 표시
                tileImage.sprite = emptyTileSprite;
                Debug.LogWarning($"{buildingData.buildingName} Lv.{buildingData.level} 데이터에 이미지가 없습니다.");
            }
        }
    }
    public BuildingUpgradeData GetBuildingData()
    {
        return _buildingData;
    }
}
