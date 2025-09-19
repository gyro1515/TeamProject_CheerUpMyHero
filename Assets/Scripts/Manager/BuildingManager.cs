using System.Resources;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildingManager : SingletonMono<BuildingManager>
{
    private GameObject tilePrefab;
    private Transform gridParent;
    private BuildingTile[,] _tiles = new BuildingTile[5, 5];
    private BuildingTile _selectedTile;

    private ConstructionSelectPanel selectPanel;
    private ConstructionUpgradePanel upgradePanel;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init() { if (Instance != null) { } }

    protected override void Awake()
    {
        base.Awake();

    }


    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;




    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "MainScene") return;

        // 패널 캐싱
        var mainUI = FindObjectOfType<MainScreenUI>();
        if (mainUI != null)
        {
            selectPanel = mainUI.GetComponentInChildren<ConstructionSelectPanel>(true);
            upgradePanel = mainUI.GetComponentInChildren<ConstructionUpgradePanel>(true);
        }

        if (selectPanel == null) Debug.LogError("SelectPanel을 찾을 수 없음!");
        if (upgradePanel == null) Debug.LogError("UpgradePanel을 찾을 수 없음!");

        LoadResources();
        CreateGrid();
    }

    private void LoadResources()
    {
        tilePrefab = Resources.Load<GameObject>("Prefabs/UI/BuildingTile");

        var mainUI = UIManager.Instance.GetUI<MainScreenUI>();
        if (mainUI != null)
        {
            var gridLayout = mainUI.GetComponentInChildren<UnityEngine.UI.GridLayoutGroup>(true);
            if (gridLayout != null) gridParent = gridLayout.transform;
            else Debug.LogError("GridLayoutGroup이 없습니다.");
        }
        else Debug.LogError("MainScreenUI를 찾을 수 없습니다.");
    }

    private void CreateGrid()
    {
        if (gridParent == null) { Debug.LogError("gridParent가 null입니다."); return; }

        foreach (Transform child in gridParent) Destroy(child.gameObject);

        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                var tileGO = Instantiate(tilePrefab, gridParent);
                var tile = tileGO.GetComponent<BuildingTile>();
                tile.Initialize(x, y);
                _tiles[x, y] = tile;

                var buildingData = PlayerDataManager.Instance.BuildingGridData[x, y];
                if (buildingData != null) tile.SetBuilding(buildingData);
            }
        }

        Debug.Log("타일 그리드 생성 완료!");
    }

    public void HandleTileClick(BuildingTile tile)
    {
        if (_selectedTile != null) _selectedTile.SetSelected(false);
        _selectedTile = tile;
        _selectedTile.SetSelected(true);

        var currentBuilding = PlayerDataManager.Instance.BuildingGridData[tile.X, tile.Y];

        if (tile.MyTileType == TileType.Normal)
        {
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
    }

    //UI 패널이 닫힐 때 호출할 타일 선택 해제 함수
    public void DeselectTile()
    {
        if (_selectedTile != null)
        {
            _selectedTile.SetSelected(false);
            _selectedTile = null; // 선택된 타일 정보 초기화
        }

    }

    // ---------------- 건설 ----------------
    public void BuildBuildingOnTile(BuildingTile tile, int buildingBaseID)
    {
        if (tile == null) { Debug.LogError("tile이 null입니다."); return; }

        // 0레벨(건설) 데이터 가져오기
        var constructionData = DataManager.Instance.BuildingUpgradeData.GetData(buildingBaseID);
        if (constructionData == null) { Debug.LogError($"ID {buildingBaseID} 건설 데이터 없음."); return; }

        // 비용 체크
        bool canAfford = true;
        foreach (var cost in constructionData.costs)
        {
            if (PlayerDataManager.Instance.GetResourceAmount(cost.resourceType) < cost.amount)
            {
                canAfford = false;
                break;
            }
        }

        if (!canAfford) { Debug.Log("자원이 부족하여 건설 불가"); return; }

        // 비용 차감
        foreach (var cost in constructionData.costs)
        {
            PlayerDataManager.Instance.AddResource(cost.resourceType, -cost.amount);
        }

        // 건설 후의 상태인 '1레벨 데이터'를 가져옵니다.
        var level1Data = DataManager.Instance.BuildingUpgradeData.GetData(constructionData.nextLevel);
        if (level1Data == null)
        {
            Debug.LogError($"ID {constructionData.nextLevel}에 해당하는 1레벨 데이터를 찾을 수 없습니다.");
            return;
        }

        // 그리드 데이터에 '1레벨 데이터'를 저장하고, 타일 상태를 업데이트합니다.
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
        if (next == null) { Debug.Log("최대 레벨"); return; }

        bool canAfford = true;
        foreach (var cost in current.costs)
            if (PlayerDataManager.Instance.GetResourceAmount(cost.resourceType) < cost.amount) canAfford = false;

        if (!canAfford) 
        { Debug.Log("자원이 부족"); return; }

        foreach (var cost in current.costs)
            PlayerDataManager.Instance.AddResource(cost.resourceType, - cost.amount);

        PlayerDataManager.Instance.BuildingGridData[tile.X, tile.Y] = next;
        tile.SetBuilding(next);

        Debug.Log($"{current.buildingName} Lv.{current.level} → Lv.{next.level} 업그레이드 완료!");
    }
}
