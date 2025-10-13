using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] private GameObject selectedFrameObject;

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

    //private void OnDisable()
    //{
    //    if (_tiles == null) return;

    //    foreach (var tile in _tiles)
    //    {
    //        if (tile != null)
    //            tile.OnTileClicked -= HandleTileClick;
    //    }
    //}
    // ---------------- 타일 선택 ----------------
    private void HandleTileClick(BuildingTile tile)
    {
        _selectedTile = tile;

        selectedFrameObject.SetActive(true);
        selectedFrameObject.transform.position = tile.transform.position;

        TileStatus status = PlayerDataManager.Instance.TileStatusGrid[tile.X, tile.Y];
        var currentBuilding = PlayerDataManager.Instance.BuildingGridData[tile.X, tile.Y];

        if (status == TileStatus.Damaged && currentBuilding != null)
        {
            // '반파'된 건물이면 -> 수리 확인창 열기
            upgradePanel.InitializeForRepair(tile);
            upgradePanel.OpenUI();
        }
        else if (status == TileStatus.Normal && tile.MyTileType == TileType.Normal)
        {
            // '정상' 상태의 일반 타일이면 -> 기존 건설/업그레이드 로직
            if (currentBuilding == null)
            {
                selectPanel.Initialize(tile, upgradePanel);
                selectPanel.OpenUI();
            }
            else
            {
                upgradePanel.InitializeForUpgrade(tile);
                upgradePanel.OpenUI();
            }
        }
        else
        {
            // 황폐화, 수리 중, 스페셜 타일 등은 선택만 하고 패널은 열지 않음
            Debug.Log($"타일 ({tile.X},{tile.Y})은(는) 현재 상호작용할 수 없습니다.");
            // DeselectTile(); // 바로 선택 해제할 수도 있음
        }
    }

    // ---------------- 타일 선택 해제 ----------------
    public void DeselectTile()
    {
        if (_selectedTile != null)
        {
            _selectedTile = null;
            selectedFrameObject.SetActive(false);
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

        PlayerDataManager.Instance.UpdateAllBuildingEffects();

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

        PlayerDataManager.Instance.UpdateAllBuildingEffects(); 

        Debug.Log($"{current.buildingName} Lv.{current.level} → Lv.{next.level} 업그레이드 완료!");
    }

    // ------수리------
    public void RepairBuildingOnTile(BuildingTile tile)
    {
        var currentBuildingData = PlayerDataManager.Instance.BuildingGridData[tile.X, tile.Y];
        if (currentBuildingData == null) return;

        BuildingUpgradeData prevLevelData = DataManager.Instance.BuildingUpgradeData.Values
                                            .FirstOrDefault(data => data.nextLevel == currentBuildingData.idNumber);

        if (prevLevelData == null)
        {
            Debug.LogError($"건물 ID {currentBuildingData.idNumber}의 이전 레벨 데이터를 찾을 수 없어 수리 비용을 계산할 수 없습니다.");
            return;
        }

        // prevLevelData.costs가 바로 현재 건물을 지을 때 들었던 비용
        List<Cost> repairCosts = prevLevelData.costs;

        // 모든 필요 자원을 확인
        bool canAfford = true;
        foreach (var cost in repairCosts)
        {
            // 각 자원의 필요량은 50%로 계산
            int requiredAmount = Mathf.CeilToInt(cost.amount * 0.5f);
            if (PlayerDataManager.Instance.GetResourceAmount(cost.resourceType) < requiredAmount)
            {
                canAfford = false;
                break; // 하나라도 부족하면 즉시 중단
            }
        }

        if (!canAfford)
        {
            Debug.Log("자원이 부족하여 수리할 수 없습니다.");
            return;
        }

        // 모든 자원을 차감
        foreach (var cost in repairCosts)
        {
            int costAmount = Mathf.CeilToInt(cost.amount * 0.5f);
            PlayerDataManager.Instance.AddResource(cost.resourceType, -costAmount);
        }

        //상태를 'Damaged'에서 'Repairing'으로 변경
        PlayerDataManager.Instance.TileStatusGrid[tile.X, tile.Y] = TileStatus.Repairing;

        tile.UpdateStatusVisual();
        Debug.Log($"타일 ({tile.X},{tile.Y})의 수리를 시작합니다. 남은 턴: {PlayerDataManager.Instance.TileRepairTurnsGrid[tile.X, tile.Y]}");
    }
}
