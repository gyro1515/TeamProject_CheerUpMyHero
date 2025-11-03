using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum TileType
{
    Normal,    // 일반 영지
    Special,   // 특수 영지
    None
}

/// <summary>
/// 메인 화면의 각 타일을 나타내고 상호작용을 처리하는 스크립트입니다.
/// </summary>
public class BuildingTile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("쿨타임 UI")]
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TextMeshProUGUI cooldownTimerText;

    [Header("상태 UI")]
    [SerializeField] private TextMeshProUGUI repairTurnText; 

    [Header("타일 이미지 설정")]
    [SerializeField] private Image tileImage; // 타일 이미지를 표시할 Image 컴포넌트
    [SerializeField] public Sprite emptyTileSprite; // 건물이 없을 때 표시할 기본 빈 타일 이미지
    [SerializeField] private Sprite diplomacyBuildingSprite; // 외교 건물 스프라이트 
    [SerializeField] private Sprite yeongjugwanBuildingSprite; // 영주관 건물 스프라이트 
    [SerializeField] private Sprite militaryBuildingSprite; // 군사구역 건물 스프라이트 

    [Header("등급 (별) UI")]
    [SerializeField] private GameObject starContainer1; // 별 1개짜리 컨테이너
    [SerializeField] private GameObject starContainer2; // 별 2개짜리 컨테이너
    [SerializeField] private GameObject starContainer3; // 별 3개짜리 컨테이너

    public int X { get; private set; }
    public int Y { get; private set; }
    public TileType MyTileType { get; private set; }

    private BuildingUpgradeData _buildingData; // 현재 타일에 건설된 건물의 데이터 
    private MainScreenBuildingController _controller; // 메인 컨트롤러 참조
    private Button tileButton; // 타일 자체의 버튼 컴포넌트 참조

    public void Initialize(int x, int y, MainScreenBuildingController controller)
    {
        X = x;
        Y = y;
        _controller = controller; // 컨트롤러 참조 저장
        tileButton = GetComponent<Button>(); // 버튼 컴포넌트 찾아두기

        // 기본 상태 설정
        MyTileType = TileType.Normal;
        tileImage.sprite = emptyTileSprite;
        tileImage.color = Color.white;
        if (tileButton != null) tileButton.interactable = true; // 기본적으로 클릭 가능
        if (repairTurnText != null)
        {
            repairTurnText.gameObject.SetActive(false);
        }
        SetRarityStars(0); // 0레벨 = 별 0개
        // 특수 타일 기본 설정 (오른쪽과 아래쪽 라인)
        if (x == 4 || y == 4)
        {
            MyTileType = TileType.Special;
            if (tileButton != null) tileButton.interactable = false; // 특수 타일은 기본적으로 클릭 불가
            tileImage.color = Color.gray; // 회색으로 표시

            // --- (4,1) 외교 타일 특별 처리 ---
            if (x == 4 && y == 1)
            {
                // 외교 건물 스프라이트 설정
                if (diplomacyBuildingSprite != null)
                {
                    tileImage.sprite = diplomacyBuildingSprite;
                }
                else
                {
                    Debug.LogWarning($"(4,1) 타일({name})에 Diplomacy Building Sprite가 연결되지 않았습니다.");
                }
                // 외교 타일의 초기 상태 업데이트 (잠김/해제)
                UpdateDiplomacyTileStatus();
            }
            if (x == 4 && y == 0)
            {
                // 영주관 건물 스프라이트 설정
                if (yeongjugwanBuildingSprite != null)
                {
                    tileImage.sprite = yeongjugwanBuildingSprite;
                    if (tileButton != null) tileButton.interactable = true;
                }
                else
                {
                    Debug.LogWarning($"(4,0) 타일({name})에 yeongjugwanBuildingSprite가 연결되지 않았습니다.");
                }
            }
            if (x == 4 && y == 2)
            {
                // 외교 건물 스프라이트 설정
                if (militaryBuildingSprite != null)
                {
                    tileImage.sprite = militaryBuildingSprite;
                    if (tileButton != null) tileButton.interactable = true;
                }
                else
                {
                    Debug.LogWarning($"(4,1) 타일({name})에 militaryBuildingSprite가 연결되지 않았습니다.");
                }
            }
        }

        tileButton?.onClick.RemoveAllListeners(); 
        tileButton?.onClick.AddListener(OnTileClick);

    }

    public void UpdateDiplomacyTileStatus()
    {
        if (!(X == 4 && Y == 1)) return;

        bool isUnlocked = PlayerDataManager.Instance.IsStageCleared(1, 2);

        if (isUnlocked)
        {
            // 해금 상태: 원래 색상(흰색), 클릭 가능
            tileImage.color = Color.white;
            if (tileButton != null) tileButton.interactable = true;
            MyTileType = TileType.Special; // 클릭 시 외교 패널 열리도록 Special 유지
            Debug.Log($"(4,1) 외교 타일({name}) 활성화됨.");
        }
        else
        {
            // 잠김 상태: 회색, 클릭 불가능
            tileImage.color = Color.gray;
            if (tileButton != null) tileButton.interactable = false;
            Debug.Log($"(4,1) 외교 타일({name}) 비활성화됨 (잠김).");
        }
    }
    // 타일이 클릭되었을 때 호출됩니다. 컨트롤러에게 클릭 이벤트를 전달합니다.
    private void OnTileClick()
    {
        // 컨트롤러가 없거나, 컨트롤러가 현재 드래그 중이면 클릭 무시
        if (_controller == null || _controller.IsDragging()) return;

        // 컨트롤러의 HandleTileClick 함수 호출
        _controller.HandleTileClick(this);
    }

    #region 드래그 앤 드랍 인터페이스 구현 (컨트롤러 참조 사용)
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_controller == null) return;

        // 드래그 가능 조건 확인: 건물이 있고, 일반 타일이며, 상태가 Normal이어야 함
        if (_buildingData == null || MyTileType != TileType.Normal ||
            PlayerDataManager.Instance._TileDataHandler.TileStatusGrid[X, Y] != TileStatus.Normal)
        {
            eventData.pointerDrag = null; // 조건 미달 시 드래그 취소
            return;
        }
        _controller.StartDrag(this); // 컨트롤러에게 드래그 시작 알림
    }

    public void OnDrag(PointerEventData eventData)
    {
        _controller?.UpdateDrag(eventData); // 컨트롤러에게 드래그 중 위치 업데이트 알림
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _controller?.EndDrag(); // 컨트롤러에게 드래그 종료 알림
    }

    public void OnDrop(PointerEventData eventData)
    {
        _controller?.HandleDrop(this); // 컨트롤러에게 드랍 이벤트 알림
    }
    #endregion

    public void SetBuilding(BuildingUpgradeData buildingData)
    {
        if (MyTileType == TileType.Special)
        {
            _buildingData = buildingData; // 데이터는 받아올 수 있지만 (아마 null)
            return; //스프라이트를 덮어쓰지 않고 즉시 종료!
        }
        _buildingData = buildingData; 

        tileImage.sprite = (buildingData == null || buildingData.buildingSprite == null)
                            ? emptyTileSprite
                            : buildingData.buildingSprite;

        if (MyTileType == TileType.Normal)
        {
            tileImage.color = Color.white;
        }
    }

    public BuildingUpgradeData GetBuildingData()
    {
        return _buildingData;
    }

    public void UpdateStatusVisual()
    {
        if (X == 4 && Y == 1) return;
        if (MyTileType == TileType.Special) 
        {
            SetRarityStars(0);
            return;
        }

        TileStatus status = PlayerDataManager.Instance._TileDataHandler.TileStatusGrid[X, Y];
        int turnsRemaining = PlayerDataManager.Instance._TileDataHandler.TileRepairTurnsGrid[X, Y];
        const int totalTurns = 3;

        switch (status)
        {
            case TileStatus.Damaged:
            case TileStatus.Repairing:
                if (repairTurnText != null)
                {
                    repairTurnText.gameObject.SetActive(true);
                    repairTurnText.text = $"{turnsRemaining}T"; // "3T", "2T", "1T"
                }
                float progress = 1.0f - ((float)turnsRemaining / totalTurns);
                Color startColor = (status == TileStatus.Damaged) ? Color.red : Color.cyan;
                tileImage.color = Color.Lerp(startColor, Color.white, progress);
                SetRarityStars(0);
                break;

            case TileStatus.Normal:
                if (repairTurnText != null)
                {
                    repairTurnText.gameObject.SetActive(false);
                }
                if (_buildingData != null)
                {
                    SetRarityStars(_buildingData.level);
                    tileImage.color = Color.white;
                }

                else
                    SetRarityStars(0);
                break;
        }
    }

    public void UpdateCooldownStatus()
    {
        if (PlayerDataManager.Instance?._TileDataHandler == null) return;

        DateTime cooldownEndTime = PlayerDataManager.Instance._TileDataHandler.CooldownEndTimeGrid[X, Y];
        bool isCoolingDown = cooldownEndTime > DateTime.UtcNow;

        if (cooldownOverlay != null) cooldownOverlay.gameObject.SetActive(isCoolingDown);
        if (cooldownTimerText != null) cooldownTimerText.gameObject.SetActive(isCoolingDown);
        if (isCoolingDown)
        {
            TimeSpan remainingTime = cooldownEndTime - DateTime.UtcNow;
            UpdateTimerText(remainingTime);
        }
        if (isCoolingDown && repairTurnText != null)
        {
            repairTurnText.gameObject.SetActive(false);
        }
    }

    public void UpdateTimerText(TimeSpan remainingTime)
    {
        if (cooldownTimerText != null)
        {
            cooldownTimerText.text = remainingTime.ToString(@"mm\:ss");
        }
    }
    private void SetRarityStars(int level)
    {
        // (안전 장치)
        if (starContainer1 == null || starContainer2 == null || starContainer3 == null)
        {
            return;
        }

        // 1. 레벨에 따라 올바른 컨테이너만 켭니다.
        if (level >= 9) // 9레벨 이상: 별 3개
        {
            starContainer1.SetActive(false);
            starContainer2.SetActive(false);
            starContainer3.SetActive(true);
        }
        else if (level >= 5) // 5~8레벨: 별 2개
        {
            starContainer1.SetActive(false);
            starContainer2.SetActive(true);
            starContainer3.SetActive(false);
        }
        else if (level >= 1) // 1~4레벨: 별 1개
        {
            starContainer1.SetActive(true);
            starContainer2.SetActive(false);
            starContainer3.SetActive(false);
        }
        else // 0레벨 또는 건물 없음: 별 0개
        {
            starContainer1.SetActive(false);
            starContainer2.SetActive(false);
            starContainer3.SetActive(false);
        }
    }
}