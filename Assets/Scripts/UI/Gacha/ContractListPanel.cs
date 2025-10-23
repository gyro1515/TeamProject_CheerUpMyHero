using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ContractListPanel : MonoBehaviour, IEndDragHandler
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

    private int currentPageIndex = 0;
    private int totalPages;
    private float pageWidth;
    private bool isSnapping = false; // 스냅 애니메이션 중인지 확인
    private Vector2 previousViewportSize = Vector2.zero; // 이전 뷰포트 크기 (크기 변경 감지용)
    private bool isInitialized = false; // 초기화 완료 여부

    void Start()
    {
        totalPages = pages.Length;
        if (totalPages < 1 || scrollRect == null)
        {
            if (scrollRect != null) scrollRect.horizontal = false;
            Debug.LogError("페이지가 없거나 ScrollRect가 연결되지 않았습니다.");
            return;
        }

        // 버튼 리스너 연결
        if (detailsButton != null)
        {
            detailsButton.onClick.RemoveAllListeners();
            detailsButton.onClick.AddListener(OnDetailsButtonClicked);
        }

        // 첫 프레임 이후 초기화 실행 (레이아웃 계산 시간 확보)
        StartCoroutine(InitializePagination());
    }

    void LateUpdate()
    {
        // 초기화 전이면 실행 안 함
        if (!isInitialized) return;

        // 화면 크기 변경 감지 및 페이지 크기 재조정
        ResizePages();
    }

    // 초기화 코루틴
    private IEnumerator InitializePagination()
    {
        // UI 요소 준비 기다림
        yield return null;

        // 페이지 크기 첫 설정 및 레이아웃 강제 업데이트
        ResizePages();

        // 레이아웃 계산이 확실히 끝날 때까지 기다림
        float expectedWidth = pageWidth * totalPages;
        yield return new WaitUntil(() => contentRect != null && (Mathf.Approximately(contentRect.rect.width, expectedWidth) || contentRect.rect.width > pageWidth));

        // 첫 페이지로 즉시 스냅
        scrollRect.horizontalNormalizedPosition = 0f;
        currentPageIndex = 0;
        UpdatePaginationDots();

        isInitialized = true;
        Debug.Log("[ContractPages] 초기화 완료.");
    }

    private void ResizePages()
    {
        RectTransform viewportRect = scrollRect?.viewport;
        if (viewportRect == null) return;

        Vector2 currentViewportSize = viewportRect.rect.size;

        // 크기가 변경되지 않았으면 실행 안 함 (최적화)
        if (previousViewportSize == currentViewportSize) return;

        previousViewportSize = currentViewportSize;
        pageWidth = currentViewportSize.x;
        Debug.Log($"[ContractPages] Viewport 크기 변경됨: {pageWidth}");

        if (pages == null) return;

        foreach (var pageRect in pages)
        {
            if (pageRect != null)
            {
                //RectTransform의 sizeDelta 직접 조절
                pageRect.sizeDelta = new Vector2(pageWidth, currentViewportSize.y);

                //Layout Element는 제거했으므로 관련 코드 삭제
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        // 변경된 크기에 맞춰 현재 페이지 위치 유지 (선택 사항)
        SnapToPage(currentPageIndex, true);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isInitialized || isSnapping || totalPages <= 1) return;

        // 현재 스크롤 위치를 기반으로 가장 가까운 페이지 인덱스 계산
        float currentPositionRatio = scrollRect.horizontalNormalizedPosition; // 0 ~ 1
        int newPageIndex = Mathf.RoundToInt(currentPositionRatio * (totalPages - 1)); // 0 또는 1

        newPageIndex = Mathf.Clamp(newPageIndex, 0, totalPages - 1);

        // 페이지가 실제로 변경되었는지 확인하는 로직 수정
        bool pageChanged = newPageIndex != currentPageIndex;
        currentPageIndex = newPageIndex; // 계산된 페이지로 먼저 업데이트

        // 스냅 시작
        SnapToPage(currentPageIndex);

        // 페이지가 변경되었을 때만 점 업데이트
        if (pageChanged)
        {
            UpdatePaginationDots();
            Debug.Log($"페이지 변경됨: {currentPageIndex + 1}");
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
        if (!isInitialized) return;
        Debug.Log($"상세 설명 버튼 클릭됨 - 현재 페이지: {currentPageIndex + 1}");
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
            scrollRect.horizontalNormalizedPosition = targetNormalizedPos;
        }
        else
        {
            // 이미 진행 중인 스냅 코루틴이 있다면 중지
            StopCoroutine("SmoothSnapCoroutine");
            StartCoroutine(SmoothSnapCoroutine(targetNormalizedPos));
        }
    }

    private IEnumerator SmoothSnapCoroutine(float targetPosition)
    {
        isSnapping = true;
        float startPosition = scrollRect.horizontalNormalizedPosition;
        float timer = 0f;
        float duration = 0.2f; // 스냅 시간

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            progress = progress * progress * (3 - 2 * progress); // SmoothStep
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(startPosition, targetPosition, progress);
            yield return null;
        }

        scrollRect.horizontalNormalizedPosition = targetPosition; // 정확히 맞춤
        isSnapping = false;
    }
}