using System.Resources;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildingManager : SingletonMono<BuildingManager>
{
    private GameObject tilePrefab;
    private Transform gridParent;

    private BuildingTile[,] _tiles = new BuildingTile[4, 4];

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        if (Instance != null) { }
    }

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SeongminMainScene")
        {
            LoadResources();
            CreateGrid();
        }
    }

    private void LoadResources()
    {
        tilePrefab = Resources.Load<GameObject>("Prefabs/UI/BuildingTile");

        MainScreenUI mainUI = UIManager.Instance.GetUI<MainScreenUI>();
        if (mainUI != null)
        {
            var gridLayout = mainUI.GetComponentInChildren<UnityEngine.UI.GridLayoutGroup>(true);
            if (gridLayout != null)
                gridParent = gridLayout.transform;
            else
                Debug.LogError("GridLayoutGroup이 없습니다.");
        }
        else
        {
            Debug.LogError("MainScreenUI를 찾을 수 없습니다.");
        }
    }

    private void CreateGrid()
    {
        if (gridParent == null)
        {
            Debug.LogError("gridParent가 null입니다.");
            return;
        }

        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                GameObject tileGO = Instantiate(tilePrefab, gridParent);
                BuildingTile tile = tileGO.GetComponent<BuildingTile>();
                tile.Initialize(x, y);
                _tiles[x, y] = tile;

                // 그리드 데이터 적용
                if (DataManager.Instance.BuildingGridData != null)
                {
                    var buildingData = DataManager.Instance.BuildingGridData[x, y];
                    if (buildingData != null)
                        tile.SetBuilding(buildingData);
                }
            }
        }
        Debug.Log("타일 그리드 생성 완료!");
    }

    public void HandleTileClick(BuildingTile tile)
    {
        if (DataManager.Instance.BuildingGridData == null) return;

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
        if (tile == null)
        {
            Debug.LogError("BuildBuildingOnTile: tile is null!");
            return;
        }

        var constructionData = DataManager.Instance.BuildingUpgradeData.GetData(buildingBaseID);
        if (constructionData == null)
        {
            Debug.LogError($"ID {buildingBaseID}에 해당하는 건설 데이터를 찾을 수 없습니다.");
            return;
        }

        // 비용 확인
        bool canAfford = true;
        foreach (var cost in constructionData.costs)
        {
            if (PlayerDataManager.Instance.GetResourceAmount(cost.resourceType) < cost.amount)
            {
                canAfford = false;
                Debug.Log($"{cost.resourceType} 자원이 부족합니다.");
                break;
            }
        }

        if (!canAfford) return;

        // 비용 차감
        foreach (var cost in constructionData.costs)
            PlayerDataManager.Instance.AddResource(cost.resourceType, -cost.amount);

        // ⚡ 0레벨 데이터 그대로 설치
        if (DataManager.Instance.BuildingGridData == null)
        {
            Debug.LogError("BuildingGridData가 초기화되지 않았습니다!");
            return;
        }

        DataManager.Instance.BuildingGridData[tile.X, tile.Y] = constructionData;
        tile.SetBuilding(constructionData);

        Debug.Log($"{tile.X},{tile.Y}에 {constructionData.buildingName} 건설 완료!");
    }

    // ---------------- 업그레이드 ----------------
    public void UpgradeBuildingOnTile(BuildingTile tile)
    {
        if (tile == null)
        {
            Debug.LogError("UpgradeBuildingOnTile: tile is null!");
            return;
        }

        var currentData = DataManager.Instance.BuildingGridData[tile.X, tile.Y];
        if (currentData == null)
        {
            Debug.LogError("업그레이드할 건물이 없습니다.");
            return;
        }

        var nextData = DataManager.Instance.BuildingUpgradeData.GetData(currentData.nextLevel);
        if (nextData == null)
        {
            Debug.Log("최대 레벨이라 업그레이드할 수 없습니다.");
            return;
        }

        bool canAfford = true;
        foreach (var cost in nextData.costs)
        {
            if (PlayerDataManager.Instance.GetResourceAmount(cost.resourceType) < cost.amount)
            {
                canAfford = false;
                break;
            }
        }

        if (!canAfford)
        {
            Debug.Log("자원이 부족하여 업그레이드할 수 없습니다.");
            return;
        }

        // 비용 차감
        foreach (var cost in nextData.costs)
            PlayerDataManager.Instance.AddResource(cost.resourceType, -cost.amount);

        // 그리드 데이터와 타일 적용
        DataManager.Instance.BuildingGridData[tile.X, tile.Y] = nextData;
        tile.SetBuilding(nextData);

        Debug.Log($"{nextData.buildingName}이(가) {nextData.level}레벨로 업그레이드되었습니다!");
    }
}
