using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;


public class MainScreenBuildingController : MonoBehaviour
{
    [Header("프리팹연결")]
    [SerializeField] private GameObject tilePrefab;                 // 타일 프리팹
    [SerializeField] private Transform gridParent;                  // 타일 그리드 부모 (GridLayoutGroup이 붙은 오브젝트)
    [SerializeField] private ConstructionSelectPanel selectPanel;   // 건설 선택 패널
    [SerializeField] private ConstructionUpgradePanel upgradePanel; // 업그레이드 패널
    [SerializeField] private BuildingSynergyPanel synergyPanel; // 시너지 패널
    [SerializeField] private AdCooldownPopup adCooldownPopup; // 팝업 UI
    [SerializeField] private DestroyConfirmPopup destroyPopup;

    [Header("드래그 앤 드랍")]
    [SerializeField] private Image dragIcon;

    [Header("타일 테두리")]
    [SerializeField] private GameObject selectedFrameObject;

    private BuildingTile[,] _tiles = new BuildingTile[5, 5];
    private BuildingTile _selectedTile;
    private BuildingTile _sourceDragTile; // 드래그를 시작한 타일

    public static MainScreenBuildingController Instance { get; private set; }


    public bool IsDragging() => _sourceDragTile != null; // 현재 드래그 중인지 확인하는 프로퍼티


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
        CreateGrid();
    }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    private void Start()
    {
        UpdateAllTilesUI();
    }
    void Update()
    {
        foreach (var tile in _tiles)
        {
            if (tile == null) continue;

            var dataHandler = PlayerDataManager.Instance._TileDataHandler;
            DateTime cooldownEndTime = dataHandler.CooldownEndTimeGrid[tile.X, tile.Y];

            if (cooldownEndTime > DateTime.UtcNow)
            {
                TimeSpan remainingTime = cooldownEndTime - DateTime.UtcNow;
                tile.UpdateTimerText(remainingTime);
            }
        }
    }
    private void OnEnable()
    {
        //// GameManager가 존재하고, "화면 갱신" 깃발이 세워져 있는지 확인
        //if (GameManager.Instance != null && GameManager.Instance.NeedsTileVisualUpdate)
        //{
        //    //깃발을 확인했으면, 스스로 화면 갱신을 실행
               UpdateAllTileVisuals();

        //    //일을 끝냈으므로, 깃발을 다시 내립니다. (중복 실행 방지)
        //    GameManager.Instance.NeedsTileVisualUpdate = false;
        //}
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
                tile.Initialize(x, y, this);

                _tiles[x, y] = tile;

                var buildingData = PlayerDataManager.Instance._TileDataHandler.BuildingGridData[x, y];
                if (buildingData != null)
                    tile.SetBuilding(buildingData);

                // 클릭 이벤트 연결
                tile.OnTileClicked += HandleTileClick;
            }
        }
        UpdateAllTilesUI();
        Debug.Log("타일 그리드 생성 완료!");
    }
    private void UpdateAllTilesUI()
    {
        if (_tiles == null) return;
        foreach (var tile in _tiles)
        {
            if (tile != null) UpdateTileUI(tile);
        }
    }

    private void UpdateTileUI(BuildingTile tile)
    {
        var buildingData = PlayerDataManager.Instance._TileDataHandler.BuildingGridData[tile.X, tile.Y];
        tile.SetBuilding(buildingData);
        tile.UpdateStatusVisual();
        tile.UpdateCooldownStatus();
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
        if (synergyPanel != null)
            synergyPanel.gameObject.SetActive(false);

        _selectedTile = tile;
        selectedFrameObject.SetActive(true);
        selectedFrameObject.transform.position = tile.transform.position;

        if (tile.MyTileType == TileType.Special)
        {

            Debug.Log($"스페셜 타일 ({tile.X},{tile.Y})을 클릭했습니다. (현재 기능 없음)");

            // 현재는 상호작용할 패널이 없으므로, 즉시 선택을 해제
            DeselectTile();
        }
        else if (tile.MyTileType == TileType.Normal)
        {

            TileStatus status = PlayerDataManager.Instance._TileDataHandler.TileStatusGrid[tile.X, tile.Y];
            var currentBuilding = PlayerDataManager.Instance._TileDataHandler.BuildingGridData[tile.X, tile.Y];

            if (status == TileStatus.Damaged && currentBuilding != null)
            {
                // '반파'된 건물이면 -> 수리 확인창 열기
                upgradePanel.InitializeForRepair(tile);
                upgradePanel.OpenUI();
            }
            else if (status == TileStatus.Normal)
            {
                // '정상' 상태의 일반 타일이면 -> 건설/업그레이드
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
                // 황폐화, 수리 중 상태의 '일반' 타일은 상호작용 불가
                Debug.Log($"타일 ({tile.X},{tile.Y})은(는) 현재 상호작용할 수 없습니다.");
                DeselectTile();
            }
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

        if (synergyPanel != null)
            synergyPanel.gameObject.SetActive(true);
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
        PlayerDataManager.Instance._TileDataHandler.BuildingGridData[tile.X, tile.Y] = level1Data;
        tile.SetBuilding(level1Data);

        EventManager.Publish(new GridStateChangedEvent());

        Debug.Log($"{tile.X},{tile.Y}에 {level1Data.buildingName} 건설 완료!");
    }

    // ---------------- 업그레이드 ----------------
    public void UpgradeBuildingOnTile(BuildingTile tile)
    {
        if (tile == null) { Debug.LogError("tile이 null입니다."); return; }

        var current = PlayerDataManager.Instance._TileDataHandler.BuildingGridData[tile.X, tile.Y];
        if (current == null) { Debug.LogError("업그레이드할 건물 없음"); return; }

        // 다음 레벨 데이터 확인 (업그레이드 비용은 current 데이터에 있음)
        var next = DataManager.Instance.BuildingUpgradeData.GetData(current.nextLevel);
        if (next == null)
        {
            Debug.Log("최대 레벨");
            return;
        }

        // 비용 계산을 위한 리스트 선언
        var finalCosts = new List<(ResourceType type, int amount)>();

        // --- 비용 체크 ---
        foreach (var cost in current.costs)
        {
            float reductionPercent = 0f;
            switch (cost.resourceType)
            {
                case ResourceType.Wood:
                    reductionPercent = PlayerDataManager.Instance.SynergyWoodCostReduction;
                    break;
                case ResourceType.Iron:
                    reductionPercent = PlayerDataManager.Instance.SynergyIronCostReduction;
                    break;
                case ResourceType.MagicStone:
                    reductionPercent = PlayerDataManager.Instance.SynergyMagicStoneCostReduction;
                    break;
            }

            // 최종 필요 비용 계산 (할인율 적용, 소수점 올림)
            int finalCost = Mathf.CeilToInt(cost.amount * (1.0f - reductionPercent / 100.0f));

            // 계산된 최종 비용을 리스트에 저장
            finalCosts.Add((cost.resourceType, finalCost));

            if (PlayerDataManager.Instance.GetResourceAmount(cost.resourceType) < finalCost)
            {
                Debug.Log($"자원이 부족하여 업그레이드 불가. 필요 {cost.resourceType}: {finalCost} (할인율: {reductionPercent}%)");
                return;
            }
        }

        // --- 비용 차감 ---
        foreach (var finalCost in finalCosts)
        {
            PlayerDataManager.Instance.AddResource(finalCost.type, -finalCost.amount);
        }

        // --- 저장 & 반영 ---
        PlayerDataManager.Instance._TileDataHandler.BuildingGridData[tile.X, tile.Y] = next;
        tile.SetBuilding(next);

        EventManager.Publish(new GridStateChangedEvent());

        Debug.Log($"{current.buildingName} Lv.{current.level} → Lv.{next.level} 업그레이드 완료!");
    }

    // ------수리------
    public void RepairBuildingOnTile(BuildingTile tile)
    {
        var currentBuildingData = PlayerDataManager.Instance._TileDataHandler.BuildingGridData[tile.X, tile.Y];
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
        PlayerDataManager.Instance._TileDataHandler.TileStatusGrid[tile.X, tile.Y] = TileStatus.Repairing;
        PlayerDataManager.Instance.UpdateAllBuildingEffects();

        tile.UpdateStatusVisual();
        Debug.Log($"타일 ({tile.X},{tile.Y})의 수리를 시작합니다. 남은 턴: {PlayerDataManager.Instance._TileDataHandler.TileStatusGrid[tile.X, tile.Y]}");
    }

    public void InitiateDestruction(BuildingTile tile)
    {
        var buildingData = PlayerDataManager.Instance._TileDataHandler.BuildingGridData[tile.X, tile.Y];
        if (buildingData == null) return;

        List<Cost> totalCost = PlayerDataManager.Instance.CalculateTotalInvestedCost(buildingData);

        var refundAmounts = new Dictionary<ResourceType, int>();

        foreach (var cost in totalCost)
        {
            int refundAmount = Mathf.FloorToInt(cost.amount * 0.5f);
            if (refundAmount > 0)
            {
                refundAmounts[cost.resourceType] = refundAmount;
            }
        }
        string buildingInfo = $"{buildingData.buildingName} Lv.{buildingData.level} 파괴";

        destroyPopup.OpenPopup(tile, buildingInfo, refundAmounts, this);
    }

    public void ConfirmDestruction(BuildingTile tile)
    {
        PlayerDataManager.Instance.DestroyBuildingAt(tile.X, tile.Y);

        UpdateTileUI(tile);
        PlayerDataManager.Instance.UpdateAllSynergyEffects();
        if (synergyPanel != null)
        {
            synergyPanel.UpdateDisplay();
        }
    }
    public void UpdateAllTileVisuals()
    {
        if (_tiles == null) return;

        // CreateGrid가 아직 호출되지 않아 _tiles가 비어있을 수 있으므로 방어 코드 추가
        if (_tiles[0, 0] == null)
        {
            CreateGrid(); // 만약 타일이 없다면 생성부터 하도록 강제
        }

        foreach (var tile in _tiles)
        {
            if (tile != null)
            {
                tile.UpdateStatusVisual();
            }
        }
        Debug.Log("모든 타일의 시각적 상태를 업데이트했습니다.");
    }

    #region 드래그 앤 드랍 로직

    public void StartDrag(BuildingTile sourceTile)
    {
        var dataHandler = PlayerDataManager.Instance._TileDataHandler;
        if (sourceTile == null) return;

        _sourceDragTile = sourceTile;
        var buildingData = sourceTile.GetBuildingData();

        if (buildingData != null)
        {
            dragIcon.sprite = buildingData.buildingSprite;
            dragIcon.gameObject.SetActive(true);
            dragIcon.transform.position = Input.mousePosition;

            sourceTile.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
        }
    }
    public void UpdateDrag(PointerEventData eventData)
    {
        if (dragIcon.gameObject.activeInHierarchy)
        {
            dragIcon.transform.position = eventData.position;
        }
    }

    public void EndDrag()
    {
        if (_sourceDragTile != null)
        {
            // 드랍이 성공하지 못하고 끝났을 경우, 원래 타일의 모습을 복원
            UpdateTileUI(_sourceDragTile);
        }
        _sourceDragTile = null;
        dragIcon.gameObject.SetActive(false);
    }

    public void HandleDrop(BuildingTile destinationTile)
    {
        var dataHandler = PlayerDataManager.Instance._TileDataHandler;

        if (_sourceDragTile == null || _sourceDragTile == destinationTile) return;

        var destStatus = dataHandler.TileStatusGrid[destinationTile.X, destinationTile.Y];
        if (destinationTile.MyTileType == TileType.Special || destStatus != TileStatus.Normal) return;

        DateTime sourceCooldownEndTime = dataHandler.CooldownEndTimeGrid[_sourceDragTile.X, _sourceDragTile.Y];
        bool isSourceOnCooldown = (sourceCooldownEndTime > DateTime.UtcNow);

        DateTime destCooldownEndTime = dataHandler.CooldownEndTimeGrid[destinationTile.X, destinationTile.Y];
        bool isDestOnCooldown = (destCooldownEndTime > DateTime.UtcNow);

        if (!isSourceOnCooldown && !isDestOnCooldown)
        {
            PerformMoveOrSwap(destinationTile, true); // 새 쿨타임 적용
            return;
        }

        var sourceBuilding = dataHandler.BuildingGridData[_sourceDragTile.X, _sourceDragTile.Y];
        if (sourceBuilding != null && adCooldownPopup != null)
        {
            adCooldownPopup.OpenPopup(_sourceDragTile, destinationTile, this);
        }
        else
        {
            _sourceDragTile = null;
            dragIcon.gameObject.SetActive(false);
        }
    }
    #endregion

    private void PerformMoveOrSwap(BuildingTile destinationTile, bool applyNewCooldown = true)
    {
        var dataHandler = PlayerDataManager.Instance._TileDataHandler;
        var destBuilding = dataHandler.BuildingGridData[destinationTile.X, destinationTile.Y];

        if (destBuilding == null) // Case 1: 빈 타일로 이동
        {
            if (applyNewCooldown)
            {
                dataHandler.StartCooldownForBuildingAt(_sourceDragTile.X, _sourceDragTile.Y);
            }
            dataHandler.MoveBuildingData(_sourceDragTile.X, _sourceDragTile.Y, destinationTile.X, destinationTile.Y);
        }
    
        else // Case 2: 다른 건물과 위치 교체
        {
            if (applyNewCooldown)
            {
                dataHandler.StartCooldownForBuildingAt(_sourceDragTile.X, _sourceDragTile.Y);
                dataHandler.StartCooldownForBuildingAt(destinationTile.X, destinationTile.Y);
            }
            dataHandler.SwapBuildingData(_sourceDragTile.X, _sourceDragTile.Y, destinationTile.X, destinationTile.Y);
        }
   

        // 시너지 및 타일 UI 갱신
        PlayerDataManager.Instance.UpdateAllSynergyEffects();
        if (synergyPanel != null)
        {
            synergyPanel.UpdateDisplay();
        }

        UpdateTileUI(_sourceDragTile);
        UpdateTileUI(destinationTile);

        // 드래그 상태 초기화
        _sourceDragTile = null;
        dragIcon.gameObject.SetActive(false);
    }


    public void ConfirmAdAndMove(BuildingTile source, BuildingTile destination)
    {
        AdManager.Instance.ShowRewardedAd(() =>
        {
            var dataHandler = PlayerDataManager.Instance._TileDataHandler;
            var destBuilding = dataHandler.BuildingGridData[destination.X, destination.Y];

            dataHandler.ReduceCooldownForBuildingAt(source.X, source.Y, 30);
            if (destBuilding != null)
            {
                dataHandler.ReduceCooldownForBuildingAt(destination.X, destination.Y, 30);
            }

            UpdateTileUI(source);
            UpdateTileUI(destination);

            _sourceDragTile = source;
            PerformMoveOrSwap(destination, false);
        });
    }
}

