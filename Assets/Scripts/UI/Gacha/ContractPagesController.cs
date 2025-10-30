using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
public struct GachaPageChangedEvent
{
    public int NewPageIndex;
}
public class ContractPagesController : MonoBehaviour, IEndDragHandler
{
    [Header("스크롤 및 페이지 참조")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private RectTransform[] pages;

    [Header("페이지 표시기 (Pagination Dots)")]
    [SerializeField] private Image[] dots;
    [SerializeField] private Sprite activeDotSprite;
    [SerializeField] private Sprite inactiveDotSprite;

    [Header("상세 설명 버튼")]
    [SerializeField] private Button detailsButton;
    [SerializeField] private LoginConfirmPopup laterUpdatePopup;

    private int currentPageIndex = 0;
    private int totalPages;
    private float pageWidth = 0f; // 초기값을 0으로 설정
    private Coroutine _snapCoroutine = null;
    private Vector2 previousViewportSize = Vector2.zero;
    public int CurrentPageIndex => currentPageIndex;

    private IEventPublisher<GachaPageChangedEvent> _pageChangedPublisher;

    void Start()
    {
        totalPages = pages.Length;
        if (totalPages < 1 || scrollRect == null)
        {
            if (scrollRect != null) scrollRect.horizontal = false;
            Debug.LogError("페이지가 없거나 ScrollRect가 연결되지 않았습니다.");
            return;
        }

        if (detailsButton != null)
        {
            detailsButton.onClick.RemoveAllListeners();
            detailsButton.onClick.AddListener(OnDetailsButtonClicked);
        }

        _pageChangedPublisher = EventManager.GetPublisher<GachaPageChangedEvent>();
        StartCoroutine(InitializePagination());
    }

    void LateUpdate()
    {
        if (pageWidth <= 0) return;

        ResizePages();
    }

    // 초기화 코루틴
    private IEnumerator InitializePagination()
    {
        yield return null;

        RectTransform viewportRect = scrollRect?.viewport;
        if (viewportRect == null)
        {
            Debug.LogError("Viewport 없음!");
            yield break;
        }

        // ResizePages 함수를 호출하여 초기 크기 설정
        ResizePages();

        // 레이아웃 계산이 확실히 끝날 때까지 기다림
        float expectedWidth = pageWidth * totalPages;
        // Content Rect가 null이 아니고, 너비가 계산될 때까지 기다림
        yield return new WaitUntil(() => contentRect != null && (Mathf.Approximately(contentRect.rect.width, expectedWidth) || contentRect.rect.width > pageWidth));
        Debug.Log($"[ContractPages] Content 너비 계산 완료: {contentRect.rect.width} (기대값: {expectedWidth})");

        scrollRect.horizontalNormalizedPosition = 0f;
        currentPageIndex = 0;
        UpdatePaginationDots();

        Debug.Log("[ContractPages] 초기화 완료.");
    }

    private void ResizePages()
    {
        RectTransform viewportRect = scrollRect?.viewport;
        if (viewportRect == null) return;

        Vector2 currentViewportSize = viewportRect.rect.size;
        if (previousViewportSize == currentViewportSize) return;

        previousViewportSize = currentViewportSize;
        pageWidth = currentViewportSize.x; 
        Debug.Log($"[ContractPages] Viewport 크기 변경됨: {pageWidth}");

        if (pages == null) return;

        foreach (var pageRect in pages)
        {
            if (pageRect != null)
            {
                pageRect.sizeDelta = new Vector2(pageWidth, currentViewportSize.y);
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        SnapToPage(currentPageIndex, true);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (pageWidth <= 0 || _snapCoroutine != null || totalPages <= 1) return;

        float currentPositionRatio = scrollRect.horizontalNormalizedPosition;
        int newPageIndex = Mathf.RoundToInt(currentPositionRatio * (totalPages - 1));
        newPageIndex = Mathf.Clamp(newPageIndex, 0, totalPages - 1);

        bool pageChanged = newPageIndex != currentPageIndex;
        currentPageIndex = newPageIndex;
        SnapToPage(currentPageIndex);

        if (pageChanged)
        {
            UpdatePaginationDots();
            Debug.Log($"페이지 변경됨: {currentPageIndex + 1}");

            _pageChangedPublisher?.Publish(new GachaPageChangedEvent { NewPageIndex = currentPageIndex });
        }
    }

    private void UpdatePaginationDots()
    {
        if (dots == null) return;
        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] != null)
            {
                dots[i].sprite = (i == currentPageIndex) ? activeDotSprite : inactiveDotSprite;
            }
        }
    }

    private void OnDetailsButtonClicked()
    {
        if (pageWidth <= 0) return;
        Debug.Log($"상세 설명 버튼 클릭됨 - 현재 페이지: {currentPageIndex + 1}");
        if (laterUpdatePopup != null)
        {
            laterUpdatePopup.Show();
        }
        else
        {
            Debug.LogError("laterUpdatePopup MainScreenBuildingController에 연결되지 않았습니다!");
        }
        if (currentPageIndex == 0) ShowLimitedCharacterDetails();
        else ShowStandardCharacterDetails();
    }

    private void ShowLimitedCharacterDetails()
    {
        Debug.Log("한정/이벤트 캐릭터 상세 정보 표시!");
    }

    private void ShowStandardCharacterDetails()
    {
        Debug.Log("상시 캐릭터 상세 정보 표시!");
    }

    private void SnapToPage(int pageIndex, bool immediate = false)
    {
        if (totalPages <= 1) return;
        float targetNormalizedPos = (totalPages > 1) ? (float)pageIndex / (totalPages - 1) : 0f;

        if (immediate)
        {
            if (_snapCoroutine != null)
            {
                StopCoroutine(_snapCoroutine);
                _snapCoroutine = null;
            }
            scrollRect.horizontalNormalizedPosition = targetNormalizedPos;
        }
        else
        {
            if (_snapCoroutine != null)
            {
                StopCoroutine(_snapCoroutine);
            }
            _snapCoroutine = StartCoroutine(SmoothSnapCoroutine(targetNormalizedPos));
        }
    }

    private IEnumerator SmoothSnapCoroutine(float targetPosition)
    {

        float startPosition = scrollRect.horizontalNormalizedPosition;
        float timer = 0f;
        float duration = 0.2f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            progress = progress * progress * (3 - 2 * progress);
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(startPosition, targetPosition, progress);
            yield return null;
        }

        scrollRect.horizontalNormalizedPosition = targetPosition;

        _snapCoroutine = null;
    }
}