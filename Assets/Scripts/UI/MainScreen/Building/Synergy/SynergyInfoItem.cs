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


    // 일반 건물 효과
    public void Initialize(Sprite icon, string title, string description)
    {
        ClearLayoutComponents();
        titleText.text = title;
        descriptionText.text = description;

        var iconGO = CreateIcon(icon);
        var iconRect = iconGO.GetComponent<RectTransform>();
        float containerHeight = iconContainer.rect.height;
        iconRect.sizeDelta = new Vector2(containerHeight, containerHeight);
    }
    //시너지 효과
    public void Initialize(BuildingSynergyType synergyType, List<Sprite> icons, string title, string description)
    {
        ClearLayoutComponents();
        titleText.text = title;
        descriptionText.text = description;

        //시너지 타입에 따라 레이아웃과 아이콘을 다르게 설정
        switch (synergyType)
        {
            // 인접 시너지 (아이콘 2개 가로 정렬)
            case BuildingSynergyType.Farm_Barracks:
            case BuildingSynergyType.Barracks_Mine:
            case BuildingSynergyType.Barracks_LumberMill:
            case BuildingSynergyType.Mine_LumberMill:
            case BuildingSynergyType.Farm_Mine:
            case BuildingSynergyType.Farm_LumberMill:
                SetupHorizontalLayout(icons.Count);
                foreach (var icon in icons) CreateIcon(icon);
                break;

            // 라인 시너지 (아이콘 4개 가로 정렬)
            case BuildingSynergyType.Farm_Line:
            case BuildingSynergyType.LumberMill_Line:
            case BuildingSynergyType.Mine_Line:
            case BuildingSynergyType.Barracks_Line:
                SetupHorizontalLayout(4); // 라인 시너지는 아이콘 4개로 고정
                for (int i = 0; i < 4; i++) CreateIcon(icons[0]);
                break;

            // 블록 시너지 (2x2 그리드 정렬)
            case BuildingSynergyType.Specialized_Block:
            case BuildingSynergyType.Balanced_Block:
                SetupGridLayout();
                // Specialized는 아이콘 1개를 4번, Balanced는 아이콘 4개를 1번씩
                if (icons.Count == 1)
                {
                    for (int i = 0; i < 4; i++) CreateIcon(icons[0]);
                }
                else
                {
                    foreach (var icon in icons) CreateIcon(icon);
                }
                break;
        }
    }
    void SetupHorizontalLayout(int iconCount)
    {
        if (iconCount == 0) return;
        var layout = iconContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.spacing = 5;

        // 컨테이너 너비에 맞춰 아이콘 크기 계산
        float totalSpacing = layout.spacing * (iconCount - 1);
        float iconSize = (iconContainer.rect.width - totalSpacing) / iconCount;

        // 모든 자식 아이콘에 LayoutElement를 추가하여 크기 지정
        var iconChildren = iconContainer.GetComponentsInChildren<Image>();
        foreach (var icon in iconChildren)
        {
            var le = icon.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = iconSize;
            le.preferredHeight = iconSize;
        }
    }

     private void SetupGridLayout()
     {
        var layout = iconContainer.gameObject.AddComponent<GridLayoutGroup>();
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 2; // 2열 고정
        layout.spacing = new Vector2(5, 5);

        // 컨테이너 크기에 맞춰 셀 크기 자동 계산
        float cellSize = (iconContainer.rect.width - layout.spacing.x) / 2;
        layout.cellSize = new Vector2(cellSize, cellSize);
     }




    // --- 유틸리티 함수 ---

    GameObject CreateIcon(Sprite iconSprite)
    {
        var iconGO = new GameObject("Icon", typeof(Image));
        iconGO.transform.SetParent(iconContainer, false);
        var image = iconGO.GetComponent<Image>();
        image.sprite = iconSprite;
        image.preserveAspect = true;
        return iconGO;
    }

    void ClearLayoutComponents()
    {
        if (iconContainer.GetComponent<LayoutGroup>() is LayoutGroup group)
        {
            Destroy(group);
        }
        foreach (Transform child in iconContainer)
        {
            Destroy(child.gameObject);
        }
    }
}