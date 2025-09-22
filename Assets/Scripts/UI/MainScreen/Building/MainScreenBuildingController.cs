using UnityEngine;

public class MainScreenBuildingController : SingletonMono<MainScreenBuildingController>
{
    [Header("프리팹연결")]
    [SerializeField] private GameObject tilePrefab;                 // 타일 프리팹
    [SerializeField] private Transform gridParent;                  // 타일 그리드 부모 (GridLayoutGroup이 붙은 오브젝트)
    [SerializeField] private ConstructionSelectPanel selectPanel;   // 건설 선택 패널
    [SerializeField] private ConstructionUpgradePanel upgradePanel; // 업그레이드 패널

    private BuildingTile[,] _tiles = new BuildingTile[5, 5];
    private BuildingTile _selectedTile;


    protected override void Awake() //돈디스트로이 온 로드 에러가 떠서 추가했습니다
    {
        Transform originalParent = transform.parent; //UIManager에 의해 설정된 현재 부모를 기억

        transform.SetParent(null);//DontDestroyOnLoad를 호출하기 위해 잠시 루트 오브젝트로 만듦

        base.Awake();

        transform.SetParent(originalParent);  //원래의 부모에게 다시 자식으로 돌아갑니다.
    }
    private void Start()
    {
        CreateGrid();
    }

    // ---------------- 그리드 생성 ----------------
    private void CreateGrid()
    {
        if (gridParent == null || tilePrefab == null)
        {
            Debug.LogError("gridParent 또는 tilePrefab이 설정되지 않았습니다!");
            return;
        }

        // 기존 자식 제거
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        // 타일 생성
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                var tileGO = Instantiate(tilePrefab, gridParent);
                var tile = tileGO.GetComponent<BuildingTile>();
                tile.Initialize(x, y);

                _tiles[x, y] = tile;

                var buildingData = PlayerDataManager.Instance.BuildingGridData[x, y];
                if (buildingData != null)
                    tile.SetBuilding(buildingData);

                // 클릭 이벤트 연결
                tile.OnTileClicked += HandleTileClick;
            }
        }

        Debug.Log("타일 그리드 생성 완료!");
    }

    private void OnDisable()
    {
        if (_tiles == null) return;

        foreach (var tile in _tiles)
        {
            if (tile != null)
                tile.OnTileClicked -= HandleTileClick;
        }
    }
    // ---------------- 타일 선택 ----------------
    private void HandleTileClick(BuildingTile tile)
    {
        if (_selectedTile != null)
            _selectedTile.SetSelected(false);

        _selectedTile = tile;
        _selectedTile.SetSelected(true);

        var currentBuilding = PlayerDataManager.Instance.BuildingGridData[tile.X, tile.Y];

        if (tile.MyTileType == TileType.Normal)
        {
            if (currentBuilding == null)
            {
                // 건설
                selectPanel.Initialize(tile, upgradePanel);
                selectPanel.OpenUI();
            }
            else
            {
                // 업그레이드
                upgradePanel.InitializeForUpgrade(tile);
                upgradePanel.OpenUI();
            }
        }
    }

    // ---------------- 타일 선택 해제 ----------------
    public void DeselectTile()
    {
        if (_selectedTile != null)
        {
            _selectedTile.SetSelected(false);
            _selectedTile = null;
        }
    }

    // ---------------- 건설 ----------------
    public void BuildBuildingOnTile(BuildingTile tile, int buildingBaseID)
    {
        if (tile == null) { Debug.LogError("tile이 null입니다."); return; }

        var constructionData = DataManager.Instance.BuildingUpgradeData.GetData(buildingBaseID);
        if (constructionData == null)
        {
            Debug.LogError($"ID {buildingBaseID} 건설 데이터 없음.");
            return;
        }

        // 비용 체크
        foreach (var cost in constructionData.costs)
        {
            if (PlayerDataManager.Instance.GetResourceAmount(cost.resourceType) < cost.amount)
            {
                Debug.Log("자원이 부족하여 건설 불가");
                return;
            }
        }

        // 비용 차감
        foreach (var cost in constructionData.costs)
            PlayerDataManager.Instance.AddResource(cost.resourceType, -cost.amount);

        // 1레벨 데이터 가져오기
        var level1Data = DataManager.Instance.BuildingUpgradeData.GetData(constructionData.nextLevel);
        if (level1Data == null)
        {
            Debug.LogError($"ID {constructionData.nextLevel}의 1레벨 데이터를 찾을 수 없습니다.");
            return;
        }

        // 저장 & 반영
        PlayerDataManager.Instance.BuildingGridData[tile.X, tile.Y] = level1Data;
        tile.SetBuilding(level1Data);

        Debug.Log($"{tile.X},{tile.Y}에 {level1Data.buildingName} 건설 완료!");
    }

    // ---------------- 업그레이드 ----------------
    public void UpgradeBuildingOnTile(BuildingTile tile)
    {
        if (tile == null) { Debug.LogError("tile이 null입니다."); return; }

        var current = PlayerDataManager.Instance.BuildingGridData[tile.X, tile.Y];
        if (current == null) { Debug.LogError("업그레이드할 건물 없음"); return; }

        var next = DataManager.Instance.BuildingUpgradeData.GetData(current.nextLevel);
        if (next == null)
        {
            Debug.Log("최대 레벨");
            return;
        }

        // 비용 체크
        foreach (var cost in current.costs)
        {
            if (PlayerDataManager.Instance.GetResourceAmount(cost.resourceType) < cost.amount)
            {
                Debug.Log("자원이 부족하여 업그레이드 불가");
                return;
            }
        }

        // 비용 차감
        foreach (var cost in current.costs)
            PlayerDataManager.Instance.AddResource(cost.resourceType, -cost.amount);

        // 저장 & 반영
        PlayerDataManager.Instance.BuildingGridData[tile.X, tile.Y] = next;
        tile.SetBuilding(next);

        Debug.Log($"{current.buildingName} Lv.{current.level} → Lv.{next.level} 업그레이드 완료!");
    }
}
