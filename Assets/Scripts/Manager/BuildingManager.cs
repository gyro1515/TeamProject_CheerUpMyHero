using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        if (scene.name == "SeongminMainScene")//임시로
        {
            LoadResources();
            CreateGrid();
        }
    }

    private void LoadResources()
    {
        // 타일 프리팹 로드는 동일
        tilePrefab = Resources.Load<GameObject>("Prefabs/UI/BuildingTile");

        //MainScreenUI를 찾는 것은 동일
        MainScreenUI mainUI = UIManager.Instance.GetUI<MainScreenUI>();
        if (mainUI != null)
        {
            GridLayoutGroup gridLayout = mainUI.GetComponentInChildren<GridLayoutGroup>(true);

            if (gridLayout != null)
            {
                // Grid Layout Group 컴포넌트가 붙어있는 오브젝트의 Transform을 gridParent로 사용
                gridParent = gridLayout.transform;
            }
            else
            {
                Debug.LogError("MainScreenUI의 자식 중에 GridLayoutGroup을 가진 BuildingGridPanel을 찾을 수 없습니다!");
            }
        }
        else
        {
            Debug.LogError("UIManager가 MainScreenUI를 찾을 수 없습니다!");
        }
    }
    private void CreateGrid()
    {
        if (gridParent == null)
        {
            Debug.LogError("gridParent가 null이므로 타일을 생성할 수 없습니다.");
            return;
        }

        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                // 타일 생성
                GameObject tileGO = Instantiate(tilePrefab, gridParent);
                BuildingTile tile = tileGO.GetComponent<BuildingTile>();
                tile.Initialize(x, y);
                _tiles[x, y] = tile;

                //DataManager에 저장된 건물 데이터를 가져와서 타일에 적용
                BuildingUpgradeData buildingData = DataManager.Instance.BuildingGridData[x, y];
                if (buildingData != null)
                {
                    tile.SetBuilding(buildingData);
                }
            }
        }
        Debug.Log("타일 그리드 생성 완료!");
    }

    // 타일로부터 클릭 이벤트를 받는 중앙 처리 함수
    public void HandleTileClick(BuildingTile tile)
    {
        BuildingUpgradeData currentBuilding = DataManager.Instance.BuildingGridData[tile.X, tile.Y];

        if (currentBuilding == null)
        {
            // 빈 땅이면 -> 건설 종류를 선택하는 패널을 엽니다.
            UIManager.Instance.GetUI<ConstructionSelectPanel>().OpenUI();
        }
        else
        {
            // 건물이 있으면 -> 업그레이드 정보를 보여주는 패널을 엽니다.
            var panel = UIManager.Instance.GetUI<ConstructionUpgradePanel>();
            panel.Initialize(tile);
            panel.OpenUI();
        }
    }

    public void BuildBuildingOnTile(BuildingTile tile, int buildingBaseID)
    {
        //데이터 가져오기
        BuildingUpgradeData constructionData = DataManager.Instance.BuildingUpgradeData.GetData(buildingBaseID);
        if (constructionData == null)
        {
            Debug.LogError($"ID: {buildingBaseID}에 해당하는 건설 데이터를 찾을 수 없습니다.");
            return;
        }

        //건설 가능 여부 확인
        bool canAfford = true;
        foreach (Cost cost in constructionData.costs)
        {
            // 현재 보유한 자원이 필요한 자원보다 적은지 확인
            if (ResourceManager.Instance.GetResourceAmount(cost.resourceType) < cost.amount)
            {
                canAfford = false;
                Debug.Log($"{cost.resourceType} 자원이 부족합니다.");
                break; // 하나라도 부족하면 즉시 확인 중단
            }
        }

        //자원이 충분할 때만 건설 진행
        if (canAfford)
        {
            //비용 차감 (AddResource에 음수 값을 넣어 자원을 감소시킴)
            foreach (Cost cost in constructionData.costs)
            {
                ResourceManager.Instance.AddResource(cost.resourceType, -cost.amount);
            }
            BuildingUpgradeData level1Data = DataManager.Instance.BuildingUpgradeData.GetData(constructionData.nextLevel);

            DataManager.Instance.BuildingGridData[tile.X, tile.Y] = level1Data;
            tile.SetBuilding(level1Data);

            Debug.Log($"{tile.X},{tile.Y}에 {level1Data.buildingName} 건설 완료!");
        }
        else
        {
            Debug.Log("자원이 부족하여 건설할 수 없습니다.");
        }
    }
    public void UpgradeBuildingOnTile(BuildingTile tile)
    {
        // 1. 현재 타일의 건물 데이터 가져오기
        BuildingUpgradeData currentData = DataManager.Instance.BuildingGridData[tile.X, tile.Y];
        if (currentData == null)
        {
            Debug.LogError("업그레이드할 건물이 없습니다.");
            return;
        }

        // 2. 다음 레벨 업그레이드 데이터 가져오기
        BuildingUpgradeData nextData = DataManager.Instance.BuildingUpgradeData.GetData(currentData.nextLevel);
        if (nextData == null)
        {
            Debug.Log("최대 레벨이라 더 이상 업그레이드할 수 없습니다.");
            return;
        }

        // 3. 비용 확인
        bool canAfford = true;
        foreach (Cost cost in nextData.costs)
        {
            if (ResourceManager.Instance.GetResourceAmount(cost.resourceType) < cost.amount)
            {
                canAfford = false;
                break;
            }
        }

        // 4. 자원이 충분하면 업그레이드 진행
        if (canAfford)
        {
            // 비용 차감
            foreach (Cost cost in nextData.costs)
            {
                ResourceManager.Instance.AddResource(cost.resourceType, -cost.amount);
            }

            // 그리드 데이터와 타일 상태를 다음 레벨 데이터로 업데이트
            DataManager.Instance.BuildingGridData[tile.X, tile.Y] = nextData;
            tile.SetBuilding(nextData);

            Debug.Log($"{nextData.buildingName}이(가) {nextData.level}레벨로 업그레이드되었습니다!");
        }
        else
        {
            Debug.Log("자원이 부족하여 업그레이드할 수 없습니다.");
        }
    }
}