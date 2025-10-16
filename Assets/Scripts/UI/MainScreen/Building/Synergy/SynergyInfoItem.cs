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

    // --- Public 초기화 함수들 ---

    public void Initialize(Sprite icon, string title, string description, int count = 0)
    {
        PrepareForInitialization(title, description);

        // 단일 아이콘 레이아웃 활성화
        singleIconLayout.SetActive(true);
        var iconContainer = singleIconLayout.transform;

        // 아이콘 생성 및 설정
        var iconGO = CreateIcon(icon, iconContainer);
        if (iconGO == null) return;

        // 컨테이너 크기에 맞춰 아이콘 크기 조절
        var iconRect = iconGO.GetComponent<RectTransform>();
        var containerRect = iconContainer.GetComponent<RectTransform>();
        float containerHeight = containerRect.rect.height;
        iconRect.sizeDelta = new Vector2(containerHeight, containerHeight);

        // 카운트 텍스트 설정
        if (count > 1 && countText != null)
        {
            countText.text = $"x{count}";
            countText.gameObject.SetActive(true);
        }
    }

    public void Initialize(BuildingSynergyType synergyType, List<Sprite> icons, string title, string description)
    {
        PrepareForInitialization(title, description);

        switch (synergyType)
        {
            case BuildingSynergyType.Specialized_Block:
                gridIconLayout.SetActive(true);
                PopulateGridIcons(icons, 4); // 2x2 그리드, 같은 아이콘 4개
                break;

            case BuildingSynergyType.Balanced_Block:
                gridIconLayout.SetActive(true);
                PopulateGridIcons(icons); // 2x2 그리드, 리스트의 모든 아이콘
                break;

            default: // 인접 시너지 (아이콘 2개가 겹치는 형태)
                horizontalLayout.SetActive(true);
                PopulateOverlapIcons(icons);
                break;
        }
    }

    // --- 아이콘 생성 및 배치 헬퍼 ---

    private void PopulateGridIcons(List<Sprite> icons, int forceCount = 0)
    {
        var iconContainer = gridIconLayout.transform;

        // Specialized_Block: 같은 아이콘을 지정된 횟수만큼 생성
        if (forceCount > 0)
        {
            if (icons != null && icons.Count > 0 && icons[0] != null)
            {
                for (int i = 0; i < forceCount; i++)
                {
                    CreateIcon(icons[0], iconContainer);
                }
            }
        }
        // Balanced_Block: 리스트에 있는 모든 아이콘 생성
        else
        {
            if (icons != null)
            {
                foreach (var iconSprite in icons)
                {
                    CreateIcon(iconSprite, iconContainer);
                }
            }
        }
    }

    private void PopulateOverlapIcons(List<Sprite> sprites)
    {
        var iconContainer = horizontalLayout.transform;
        if (sprites == null || sprites.Count == 0) return;

        float containerHeight = iconContainer.GetComponent<RectTransform>().rect.height;
        float finalIconSize = containerHeight * 1f; // 아이콘의 실제 크기
        float holderWidth = finalIconSize * 0.5f;   // 아이콘이 절반만 보이도록 마스킹 홀더 너비 설정

        // HorizontalLayoutGroup 컴포넌트에 겹치도록 음수 spacing 설정
        var layoutGroup = iconContainer.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup != null)
        {
            // (홀더 너비 * 2) - 전체 아이콘 너비 = 겹치는 양
            float spacing = (holderWidth * 2) - finalIconSize;
            layoutGroup.spacing = spacing;
        }

        for (int i = 0; i < sprites.Count; i++)
        {
            Sprite sprite = sprites[i];
            if (sprite == null) continue;

            // 아이콘을 마스킹할 홀더(RectMask2D) 생성
            var holderGO = new GameObject($"IconHolder_{i}", typeof(RectTransform), typeof(RectMask2D));
            holderGO.transform.SetParent(iconContainer, false);
            var holderRect = holderGO.GetComponent<RectTransform>();
            holderRect.sizeDelta = new Vector2(holderWidth, finalIconSize);

            // 실제 아이콘 이미지 생성 후 홀더의 자식으로 설정
            var iconGO = new GameObject("Icon", typeof(Image));
            iconGO.transform.SetParent(holderGO.transform, false);
            var image = iconGO.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;

            // 아이콘 크기 및 위치 조절
            var iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(finalIconSize, finalIconSize);

            // 홀더 안에서 아이콘을 반대 방향으로 밀어 원하는 부분이 보이게 설정
            float shiftAmount = (finalIconSize - holderWidth) / 2f;
            if (i == 0) // 왼쪽 아이콘은 오른쪽으로 밀어서 왼쪽 절반을 보여줌
            {
                iconRect.anchoredPosition = new Vector2(shiftAmount, 0);
            }
            else // 오른쪽 아이콘은 왼쪽으로 밀어서 오른쪽 절반을 보여줌
            {
                iconRect.anchoredPosition = new Vector2(-shiftAmount, 0);
            }
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

    // --- 공통 준비 및 정리 함수 ---

    private void PrepareForInitialization(string title, string description)
    {
        // 모든 레이아웃 컨테이너를 비우고 비활성화
        ClearAllContainers();

        // 텍스트 설정
        titleText.text = title;
        descriptionText.text = description;

        // 카운트 텍스트 초기화
        if (countText != null) countText.gameObject.SetActive(false);
    }

    private void ClearAllContainers()
    {
        // 모든 레이아웃 비활성화
        singleIconLayout.SetActive(false);
        horizontalLayout.SetActive(false);
        gridIconLayout.SetActive(false);

        // 각 레이아웃의 자식 오브젝트(이전에 생성된 아이콘)들 모두 삭제
        foreach (Transform child in singleIconLayout.transform) Destroy(child.gameObject);
        foreach (Transform child in horizontalLayout.transform) Destroy(child.gameObject);
        foreach (Transform child in gridIconLayout.transform) Destroy(child.gameObject);
    }
}