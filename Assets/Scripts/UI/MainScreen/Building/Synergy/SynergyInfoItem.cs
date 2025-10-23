using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SynergyInfoItem : BasePoolable 
{
    [Header("UI 연결")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI countText;

    [Header("아이콘 레이아웃 컨테이너")]
    [SerializeField] private GameObject singleIconLayout;
    [SerializeField] private GameObject horizontalLayout;
    [SerializeField] private GameObject gridIconLayout;

    [Header("아웃라인")]
    [SerializeField] private Image outlineImage;


    public void Initialize(Sprite icon, string title, string description, int count = 0)
    {
        PrepareForInitialization(title, description);
        DisableOutline(); 
        PopulateSingleIcon(icon, count);
    }

 
    public void Initialize(BuildingSynergyType synergyType, List<Sprite> icons, string title, string description)
    {
        PrepareForInitialization(title, description);
        SetOutline(synergyType); // 시너지 타입에 맞는 테두리 설정

        switch (synergyType)
        {
            case BuildingSynergyType.Farm_Line:
            case BuildingSynergyType.LumberMill_Line:
            case BuildingSynergyType.Mine_Line:
            case BuildingSynergyType.Barracks_Line:
                // 라인 시너지는 단일 아이콘 + 카운트로 표시
                PopulateSingleIcon(icons != null && icons.Count > 0 ? icons[0] : null, 4);
                break;
            // ---------------------------------

            case BuildingSynergyType.Specialized_Block:
                gridIconLayout.SetActive(true);
                PopulateGridIcons(icons, 4);
                break;

            case BuildingSynergyType.Balanced_Block:
                gridIconLayout.SetActive(true);
                PopulateGridIcons(icons);
                break;

            default: // 인접 시너지 (아이콘 2개가 겹치는 형태)
                horizontalLayout.SetActive(true);
                PopulateOverlapIcons(icons);
                break;
        }
    }

    // --- 아이콘 생성 및 배치 헬퍼 ---

    private void PopulateSingleIcon(Sprite icon, int count)
    {
        singleIconLayout.SetActive(true);
        var iconContainer = singleIconLayout.transform;
        ClearContainer(iconContainer); // 이전 아이콘 제거

        var iconGO = CreateIcon(icon, iconContainer);
        if (iconGO == null) return;

        var iconRect = iconGO.GetComponent<RectTransform>();
        var containerRect = iconContainer.GetComponent<RectTransform>();
        if (containerRect != null) // null 체크 추가
        {
            float containerHeight = containerRect.rect.height;
            iconRect.sizeDelta = new Vector2(containerHeight, containerHeight);
        }

        if (count > 1 && countText != null)
        {
            countText.text = $"x{count}";
            countText.gameObject.SetActive(true);
        }
    }

    private void PopulateGridIcons(List<Sprite> icons, int forceCount = 0)
    {
        var iconContainer = gridIconLayout.transform;
        ClearContainer(iconContainer); // 이전 아이콘 제거

        if (forceCount > 0)
        {
            if (icons != null && icons.Count > 0 && icons[0] != null)
            {
                for (int i = 0; i < forceCount; i++) CreateIcon(icons[0], iconContainer);
            }
        }
        else
        {
            if (icons != null)
            {
                foreach (var iconSprite in icons) CreateIcon(iconSprite, iconContainer);
            }
        }
    }

    private void PopulateOverlapIcons(List<Sprite> sprites)
    {
        var iconContainer = horizontalLayout.transform;
        ClearContainer(iconContainer); // 이전 아이콘 제거
        if (sprites == null || sprites.Count == 0) return;

        RectTransform containerRect = iconContainer.GetComponent<RectTransform>();
        if (containerRect == null) return; // null 체크 추가
        float containerHeight = containerRect.rect.height;
        float finalIconSize = containerHeight * 1f;
        float holderWidth = finalIconSize * 0.5f;

        var layoutGroup = iconContainer.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup != null)
        {
            float spacing = (holderWidth * 2) - finalIconSize;
            layoutGroup.spacing = spacing;
        }

        for (int i = 0; i < sprites.Count; i++)
        {
            Sprite sprite = sprites[i];
            if (sprite == null) continue;

            var holderGO = new GameObject($"IconHolder_{i}", typeof(RectTransform), typeof(RectMask2D));
            holderGO.transform.SetParent(iconContainer, false);
            var holderRect = holderGO.GetComponent<RectTransform>();
            holderRect.sizeDelta = new Vector2(holderWidth, finalIconSize);

            var iconGO = new GameObject("Icon", typeof(Image));
            iconGO.transform.SetParent(holderGO.transform, false);
            var image = iconGO.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;

            var iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(finalIconSize, finalIconSize);

            float shiftAmount = (finalIconSize - holderWidth) / 2f;
            iconRect.anchoredPosition = (i == 0) ? new Vector2(shiftAmount, 0) : new Vector2(-shiftAmount, 0);
        }
    }

    private GameObject CreateIcon(Sprite iconSprite, Transform parent)
    {
        if (iconSprite == null) return null;
        var iconGO = new GameObject("Icon", typeof(Image));
        iconGO.transform.SetParent(parent, false);
        var image = iconGO.GetComponent<Image>();
        image.sprite = iconSprite;
        image.preserveAspect = true;
        return iconGO;
    }

    // --- 테두리 제어 함수들 ---

  
    private void SetOutline(BuildingSynergyType type)
    {
        if (outlineImage == null) return;

        Color color = GetColorForSynergyType(type);
        // 색상이 투명이 아니면 테두리를 켜고 색상 적용
        if (color != Color.clear)
        {
            outlineImage.color = color;
            outlineImage.gameObject.SetActive(true);
        }
        else // 그 외의 경우는 끔
        {
            outlineImage.gameObject.SetActive(false);
        }
    }

   
    private void DisableOutline()
    {
        if (outlineImage != null)
        {
            outlineImage.gameObject.SetActive(false);
        }
    }


    private Color GetColorForSynergyType(BuildingSynergyType type)
    {
        switch (type)
        {
            // 인접 (파랑)
            case BuildingSynergyType.Farm_Barracks:
            case BuildingSynergyType.Barracks_Mine:
            case BuildingSynergyType.Barracks_LumberMill:
            case BuildingSynergyType.Mine_LumberMill:
            case BuildingSynergyType.Farm_Mine:
            case BuildingSynergyType.Farm_LumberMill:
                return Color.blue;
            // 라인 (노랑)
            case BuildingSynergyType.Farm_Line:
            case BuildingSynergyType.LumberMill_Line:
            case BuildingSynergyType.Mine_Line:
            case BuildingSynergyType.Barracks_Line:
                return Color.green;
            // 블록 (빨강)
            case BuildingSynergyType.Specialized_Block:
            case BuildingSynergyType.Balanced_Block:
                return Color.red;
            default:
                return Color.clear; // 시너지 타입이 아니면 투명색 반환
        }
    }

    // --- 공통 준비 및 정리 함수 ---

    private void PrepareForInitialization(string title, string description)
    {
        singleIconLayout.SetActive(false);
        horizontalLayout.SetActive(false);
        gridIconLayout.SetActive(false);

        ClearContainer(singleIconLayout.transform);
        ClearContainer(horizontalLayout.transform);
        ClearContainer(gridIconLayout.transform);

        DisableOutline(); 

        titleText.text = title;
        descriptionText.text = description;
        if (countText != null) countText.gameObject.SetActive(false);
    }

    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            if (container == horizontalLayout.transform && child.childCount > 0)
            {
                foreach (Transform grandChild in child)
                {
                    Destroy(grandChild.gameObject);
                }
            }
            Destroy(child.gameObject);
        }
    }
    public override void ReleaseSelf()
    {
        DisableOutline(); 

        ClearContainer(singleIconLayout.transform);
        ClearContainer(horizontalLayout.transform);
        ClearContainer(gridIconLayout.transform);

        base.ReleaseSelf(); 
    }
}