using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 


public class SynergyOutlineManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private GameObject outlinePrefab;
    [SerializeField] private Transform outlineParent;
    [SerializeField] private MainScreenBuildingController buildingController;

    private List<GameObject> activeOutlines = new List<GameObject>();
    private IEventSubscriber<SynergyDataUpdatedEvent> _synergyUpdateSubscriber;

    private void Awake()
    {

    }
    private void OnEnable()
    {
        _synergyUpdateSubscriber = EventManager.GetSubscriber<SynergyDataUpdatedEvent>();
        _synergyUpdateSubscriber.Subscribe(HandleSynergyUpdate);

        UpdateOutlines(PlayerDataManager.Instance.ActiveSynergies);
    }
    private void OnDisable()
    {
        _synergyUpdateSubscriber?.Unsubscribe(HandleSynergyUpdate);
    }

 
    private void HandleSynergyUpdate(SynergyDataUpdatedEvent e)
    {
        UpdateOutlines(PlayerDataManager.Instance.ActiveSynergies);
    }
    public void UpdateOutlines(List<DetectedSynergy> activeSynergies)
    {
        ClearOutlines();
        BuildingTile[,] tiles = buildingController?.GetTiles();

        if (activeSynergies == null || tiles == null)
        {
            return;
        }

        foreach (var synergy in activeSynergies)
        {
            DrawOutlineForSynergy(synergy, tiles);
        }
    }

    private void ClearOutlines()
    {
        foreach (var outline in activeOutlines)
        {
            Destroy(outline);
        }
        activeOutlines.Clear();
    }

    private void DrawOutlineForSynergy(DetectedSynergy synergy, BuildingTile[,] tiles)
    {
        // 유효하지 않은 데이터면 함수 종료
        if (synergy.TilePositions == null || synergy.TilePositions.Count == 0) return;

        // 시너지 타입에 맞는 색상 결정
        Color outlineColor = GetColorForSynergyType(synergy.Type);

        // 시너지 타일들의 경계(최소/최대 좌표) 찾기
        int minX = 5, minY = 5, maxX = -1, maxY = -1;
        foreach (var pos in synergy.TilePositions)
        {
            minX = Mathf.Min(minX, pos.x);
            minY = Mathf.Min(minY, pos.y);
            maxX = Mathf.Max(maxX, pos.x);
            maxY = Mathf.Max(maxY, pos.y);
        }

        //경계 좌표가 유효한지 확인 (배열 범위 내)
        if (minX < 0 || minY < 0 || maxX >= 5 || maxY >= 5) // 5x5 그리드 기준
        {
            Debug.LogError($"[SynergyOutlineManager] Invalid boundary coordinates calculated: ({minX},{minY}) to ({maxX},{maxY})");
            return;
        }

        //경계에 해당하는 타일 객체 가져오기 
        BuildingTile minTile = tiles[minX, minY];
        BuildingTile maxTile = tiles[maxX, maxY];
        if (minTile == null || maxTile == null)
        {
            Debug.LogError($"[SynergyOutlineManager] 시너지 경계 타일({minX},{minY} or {maxX},{maxY})을 찾을 수 없습니다.");
            return;
        }

        // 아웃라인 UI의 위치와 크기 계산
        RectTransform minRect = minTile.GetComponent<RectTransform>();
        RectTransform maxRect = maxTile.GetComponent<RectTransform>();

        Vector3 centerPosition = (minRect.position + maxRect.position) / 2f;

        GridLayoutGroup gridLayout = buildingController?.GetGridLayoutGroup();

        float spacingX = gridLayout != null ? gridLayout.spacing.x : 0f;
        float spacingY = gridLayout != null ? gridLayout.spacing.y : 0f;
        float tileSizeX = minRect.sizeDelta.x; 
        float tileSizeY = minRect.sizeDelta.y;

        float width = (maxX - minX + 1) * tileSizeX + (maxX - minX) * spacingX;
        float height = (maxY - minY + 1) * tileSizeY + (maxY - minY) * spacingY;
        float padding = 10f; 
        width += padding * 2;
        height += padding * 2;

        GameObject outlineGO = Instantiate(outlinePrefab, outlineParent);
        RectTransform outlineRect = outlineGO.GetComponent<RectTransform>();
        Image outlineImage = outlineGO.GetComponent<Image>();

        outlineRect.position = centerPosition;
        outlineRect.sizeDelta = new Vector2(width, height); 
        if (outlineImage != null)
        {
            outlineImage.color = outlineColor; 
        }

        activeOutlines.Add(outlineGO); 
    }

    private Color GetColorForSynergyType(BuildingSynergyType type)
    {
        switch (type)
        {
            case BuildingSynergyType.Farm_Barracks:
            case BuildingSynergyType.Barracks_LumberMill:
            case BuildingSynergyType.Mine_LumberMill:
            case BuildingSynergyType.Farm_Mine:
            case BuildingSynergyType.Farm_LumberMill:
            case BuildingSynergyType.Barracks_Mine: // 인접
                return Color.blue;
            case BuildingSynergyType.Farm_Line:
            case BuildingSynergyType.LumberMill_Line:
            case BuildingSynergyType.Mine_Line: 
            case BuildingSynergyType.Barracks_Line:
                return Color.green;
            case BuildingSynergyType.Specialized_Block:
            case BuildingSynergyType.Balanced_Block: //블록
                return Color.red;
            default:
                return Color.white;
        }
    }
}