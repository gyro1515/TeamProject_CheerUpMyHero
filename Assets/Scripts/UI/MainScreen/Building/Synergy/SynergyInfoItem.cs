using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SynergyInfoItem : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private RectTransform iconContainer;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI countText;

    // 일반 건물 효과 (아이콘 1개)
    public void Initialize(Sprite icon, string title, string description)
    {
        ClearLayoutComponents();
        if (countText != null) countText.gameObject.SetActive(false);
        titleText.text = title;
        descriptionText.text = description;

        // 아이콘이 1개일 때는 컨테이너 높이에 맞춰 크게 생성
        var iconGO = CreateIcon(icon);
        if (iconGO == null) return;

        var iconRect = iconGO.GetComponent<RectTransform>();
        float containerHeight = iconContainer.rect.height;
        iconRect.sizeDelta = new Vector2(containerHeight, containerHeight);

        // 중앙 정렬을 위해 Layout Group 추가
        var layout = iconContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
    }

    // 시너지 효과 (아이콘 여러 개)
    public void Initialize(BuildingSynergyType synergyType, List<Sprite> icons, string title, string description)
    {
        ClearLayoutComponents();
        if (countText != null) countText.gameObject.SetActive(false);
        titleText.text = title;
        descriptionText.text = description;

        switch (synergyType)
        {
            // 라인 시너지는 아이콘 4개로 고정
            case BuildingSynergyType.Farm_Line:
            case BuildingSynergyType.LumberMill_Line:
            case BuildingSynergyType.Mine_Line:
            case BuildingSynergyType.Barracks_Line:
                // 아이콘 1개를 크게 생성
                var iconGO = CreateIcon(icons[0]);
                var iconRect = iconGO.GetComponent<RectTransform>();
                float containerHeight = iconContainer.rect.height;
                iconRect.sizeDelta = new Vector2(containerHeight, containerHeight);
                // 중앙 정렬
                var layout = iconContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
                layout.childAlignment = TextAnchor.MiddleCenter;
                // "x4" 텍스트 활성화
                if (countText != null)
                {
                    countText.text = "X4";
                    countText.gameObject.SetActive(true);
                }
                break;

            // 블록 시너지 (2x2 그리드)
            case BuildingSynergyType.Specialized_Block:
            case BuildingSynergyType.Balanced_Block:
                SetupGridLayout();
                // Specialized는 아이콘 1개를 4번, Balanced는 아이콘 4개를 1번씩
                if (icons.Count == 1) for (int i = 0; i < 4; i++) CreateIcon(icons[0]);
                else foreach (var i in icons) CreateIcon(i);
                break;

            // 인접 시너지 등 나머지 (가로 정렬)
            default:
                SetupHorizontalLayoutAndCreateIcons(icons, icons.Count); // 아이콘 개수만큼 생성
                break;
        }
    }

    // 가로 레이아웃 설정과 아이콘 생성을 함께 처리하는 새로운 함수
    void SetupHorizontalLayoutAndCreateIcons(List<Sprite> sprites, int totalCount)
    {
        if (sprites == null || sprites.Count == 0 || totalCount == 0) return;

        var layout = iconContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.spacing = 5;

        // 컨테이너 너비에 맞춰 아이콘 크기 동적 계산
        float totalSpacing = layout.spacing * (totalCount - 1);
        float iconSize = (iconContainer.rect.width - totalSpacing) / totalCount;

        for (int i = 0; i < totalCount; i++)
        {
            // 라인 시너지처럼 아이콘이 1개만 제공될 경우를 대비
            Sprite spriteToShow = sprites[i % sprites.Count];
            var iconGO = CreateIcon(spriteToShow);

            // 모든 아이콘에 LayoutElement를 추가하여 계산된 크기를 강제 적용
            var le = iconGO.AddComponent<LayoutElement>();
            le.preferredWidth = iconSize;
            le.preferredHeight = iconSize;
        }
    }

    void SetupGridLayout()
    {
        var layout = iconContainer.gameObject.AddComponent<GridLayoutGroup>();
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 2;
        layout.spacing = new Vector2(5, 5);
        float cellSize = (iconContainer.rect.width - layout.spacing.x) / 2;
        layout.cellSize = new Vector2(cellSize, cellSize);
    }

    GameObject CreateIcon(Sprite iconSprite)
    {
        if (iconSprite == null) return null;
        var iconGO = new GameObject("Icon", typeof(Image));
        iconGO.transform.SetParent(iconContainer, false);
        var image = iconGO.GetComponent<Image>();
        image.sprite = iconSprite;
        image.preserveAspect = true;
        return iconGO;
    }

    void ClearLayoutComponents()
    {
        if (iconContainer.GetComponent<LayoutGroup>() is LayoutGroup group) Destroy(group);
        foreach (Transform child in iconContainer) Destroy(child.gameObject);
    }
}