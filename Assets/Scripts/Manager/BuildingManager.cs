using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildingManager : SingletonMono<BuildingManager>
{
    private GameObject tilePrefab;
    private Transform gridParent;
    private BuildingTile[,] _tiles = new BuildingTile[4, 4];

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init() { if (Instance != null) { } }

    protected override void Awake() { base.Awake(); }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "SeongminMainScene") return;
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

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                var tileGO = Instantiate(tilePrefab, gridParent);
                var tile = tileGO.GetComponent<BuildingTile>();
                tile.Initialize(x, y);
                _tiles[x, y] = tile;

                var buildingData = DataManager.Instance.BuildingGridData[x, y];
                if (buildingData != null) tile.SetBuilding(buildingData);
            }
        }

        Debug.Log("타일 그리드 생성 완료!");
    }

    public void HandleTileClick(BuildingTile tile)
    {
        var currentBuilding = DataManager.Instance.BuildingGridData[tile.X, tile.Y];

        if (currentBuilding == null)
        {
            var panel = UIManager.Instance.GetUI<ConstructionSelectPanel>();
            panel.Initialize(tile);
            panel.OpenUI();
        }
        else
        {
            var panel = UIManager.Instance.GetUI<ConstructionUpgradePanel>();
            panel.InitializeForUpgrade(tile);
            panel.OpenUI();
        }
    }

    // ---------------- 건설 ----------------
    public void BuildBuildingOnTile(BuildingTile tile, int buildingBaseID)
    {
        if (tile == null) { Debug.LogError("tile이 null입니다."); return; }

        var data = DataManager.Instance.BuildingUpgradeData.GetData(buildingBaseID);
        if (data == null) { Debug.LogError($"ID {buildingBaseID} 건설 데이터 없음."); return; }

        // 비용 체크
        bool canAfford = true;
        foreach (var cost in data.costs)
            if (PlayerDataManager.Instance.GetResourceAmount(cost.resourceType) < cost.amount) canAfford = false;

        if (!canAfford) { Debug.Log("자원이 부족하여 건설 불가"); return; }

        // 비용 차감
        foreach (var cost in data.costs)
            PlayerDataManager.Instance.AddResource(cost.resourceType, -cost.amount);

        DataManager.Instance.BuildingGridData[tile.X, tile.Y] = data;
        tile.SetBuilding(data);

        Debug.Log($"{tile.X},{tile.Y}에 {data.buildingName} 건설 완료!");
    }

    // ---------------- 업그레이드 ----------------
    public void UpgradeBuildingOnTile(BuildingTile tile)
    {
        if (tile == null) { Debug.LogError("tile이 null입니다."); return; }

        var current = DataManager.Instance.BuildingGridData[tile.X, tile.Y];
        if (current == null) { Debug.LogError("업그레이드할 건물 없음"); return; }

        var next = DataManager.Instance.BuildingUpgradeData.GetData(current.nextLevel);
        if (next == null) { Debug.Log("최대 레벨"); return; }

        bool canAfford = true;
        foreach (var cost in next.costs)
            if (PlayerDataManager.Instance.GetResourceAmount(cost.resourceType) < cost.amount) canAfford = false;

        if (!canAfford) { Debug.Log("자원이 부족"); return; }

        foreach (var cost in next.costs)
            PlayerDataManager.Instance.AddResource(cost.resourceType, -cost.amount);

        DataManager.Instance.BuildingGridData[tile.X, tile.Y] = next;
        tile.SetBuilding(next);

        Debug.Log($"{current.buildingName} Lv.{current.level} → Lv.{next.level} 업그레이드 완료!");
    }
}
