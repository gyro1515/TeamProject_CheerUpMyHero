using System.Resources;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildingManager : SingletonMono<BuildingManager>
{
    private GameObject tilePrefab;
    private Transform gridParent;
    private BuildingTile[,] _tiles = new BuildingTile[5, 5];
    private BuildingTile _selectedTile;

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
        //건설 / 업그레이드 관련 패널이 이미 활성화되어 있는지 확인합니다.
        var selectPanel = UIManager.Instance.GetUI<ConstructionSelectPanel>();
        var upgradePanel = UIManager.Instance.GetUI<ConstructionUpgradePanel>();
 
        // 둘 중 하나라도 켜져있다면, 함수를 즉시 종료하여 아무 일도 일어나지 않게 막기
        if (upgradePanel.gameObject.activeInHierarchy || selectPanel.gameObject.activeInHierarchy)
        {
            Debug.Log("이미 UI 패널이 열려있어 추가 행동을 막았습니다.");
            return;
        }


        // 이전에 선택된 타일이 있다면 선택 해제
        if (_selectedTile != null)
        {
            _selectedTile.SetSelected(false);
        }

        // 새로 클릭된 타일을 선택 상태로 만듦
        _selectedTile = tile;
        _selectedTile.SetSelected(true);

        // 타일 타입에 따라 다른 UI를 열어줌
        if (tile.MyTileType == TileType.Normal)
        {
            var currentBuilding = PlayerDataManager.Instance.BuildingGridData[tile.X, tile.Y];
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
        else if (tile.MyTileType == TileType.Special)
        {
            Debug.Log("특수 영지를 클릭했습니다!");
            //특수 영지용 UI 열기 추가
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
        foreach (var cost in next.costs)
            if (PlayerDataManager.Instance.GetResourceAmount(cost.resourceType) < cost.amount) canAfford = false;

        if (!canAfford) 
        { Debug.Log("자원이 부족"); return; }

        foreach (var cost in next.costs)
            PlayerDataManager.Instance.AddResource(cost.resourceType, - cost.amount);

        PlayerDataManager.Instance.BuildingGridData[tile.X, tile.Y] = next;
        tile.SetBuilding(next);

        Debug.Log($"{current.buildingName} Lv.{current.level} → Lv.{next.level} 업그레이드 완료!");
    }
}
