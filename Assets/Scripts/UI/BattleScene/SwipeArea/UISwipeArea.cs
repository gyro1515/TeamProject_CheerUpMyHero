using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class UISwipeArea : MonoBehaviour, IEndDragHandler, IDragHandler
{
    [Header("스와이프 UI 세팅")]
    [SerializeField] RectTransform viewport; // 스와이프 영역
    [SerializeField] RectTransform[] pages;  // 자식 페이지들
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] UISwipeIndex swipeIdx;
    Vector2 preSize = Vector2.zero;
    int pageCount;
    float[] pagePositions;
    int currentPage = 0; // 0부터 시작
    Coroutine SmoothMoveRoutine;
    List<int> pageToIdx = new List<int>();
    bool forceEnded = false;
    private void Awake()
    {
        ResizePages();
        pageCount = pages.Length;
        pagePositions = new float[pageCount];
        for (int i = 0; i < pageCount; i++)
        {
            pagePositions[i] = (float)i / (pageCount - 1); // 0 ~ 1 구간 분할
            pageToIdx.Add(i);
        }
        // 0이 가운데로 오게 하기
        //ShiftPageRight();
        
    }
    private void Start()
    {
        //scrollRect.horizontalNormalizedPosition = 1f / (pageCount - 1);
        //StartCoroutine(SetScrollPositionToCenter());
    }
    void LateUpdate()
    {
        // 기기 회전/리사이즈 대응, 테스트때 모든 기종 변화에 대응하도록
        ResizePages();
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (forceEnded) { scrollRect.OnEndDrag(eventData); return; }
        bool inside = RectTransformUtility.RectangleContainsScreenPoint(viewport, eventData.position, eventData.pressEventCamera);
        if (!inside)
        {
            // 뷰포트 밖이면 드래그 무효 처리
            scrollRect.OnEndDrag(eventData);
            MoveToNext();
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (forceEnded) return;
        //Debug.Log("위치 조정");

        MoveToNext();
    }
    void MoveToNext()
    {
        forceEnded = true;

        float pos = scrollRect.horizontalNormalizedPosition;
        float closest = float.MaxValue;
        int targetPageIdx = -1;
        for (int i = 0; i < pageCount; i++)
        {
            float dist = Mathf.Abs(pos - pagePositions[i]);
            if (dist < closest)
            {
                closest = dist;
                targetPageIdx = i;
            }
        }

        if (SmoothMoveRoutine != null) StopCoroutine(SmoothMoveRoutine);
        SmoothMoveRoutine = StartCoroutine(SmoothMoveTo(targetPageIdx));
    }

    IEnumerator SmoothMoveTo(int targetIdx)
    {
        if (targetIdx == -1) { Debug.LogWarning("로직 오류"); yield break ; }

        float elapsed = 0f;
        float start = scrollRect.horizontalNormalizedPosition;
        float duration = 0.2f;
        float targetPagePos = pagePositions[targetIdx];
        int nextIdx = pageToIdx[targetIdx];

        if(Mathf.Abs(start - targetPagePos) >= 0.05f)
        {
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t *= t; // 점점 빨라지도록
                scrollRect.horizontalNormalizedPosition = Mathf.Lerp(start, targetPagePos, t);
                yield return null;
            }
        }
        // 이 값이하라면 바로 세팅하기
        
        scrollRect.horizontalNormalizedPosition = targetPagePos;
        forceEnded = false;

        //Debug.Log($"move: {currentPage}->{pageToIdx[targetIdx]}");
        // 무한 스와이프 처리
        if (currentPage == nextIdx) yield break; // 바뀐게 없다면 그대로

        int diff = (nextIdx - currentPage + pageCount) % pageCount;

        if (diff == 1)
            ShiftPageLeft();
        else if (diff == pageCount - 1)
            ShiftPageRight();

        currentPage = nextIdx;
        swipeIdx.SetIndexOn(currentPage);

        scrollRect.horizontalNormalizedPosition = 1f / (pageCount - 1); // 중앙으로 리셋
    }
    IEnumerator SetScrollPositionToCenter()
    {
        yield return null; // 다음 프레임까지 기다림
        scrollRect.horizontalNormalizedPosition = 1f / (pageCount - 1);
    }
    void ResizePages()
    {
        Vector2 curSize = viewport.rect.size;
        if (preSize == curSize) return; // 사이즈 바뀔때만 갱신
        foreach (var page in pages)
        {
            page.sizeDelta = curSize;
        }

    }
    void ShiftPageLeft()
    {
        // 맨 앞을 맨 뒤로 보내기
        Transform first = scrollRect.content.GetChild(0);
        first.SetAsLastSibling();

        // 인덱스 갱신
        int tmp = pageToIdx[0];
        for (int i = 0; i < pageCount - 1; i++)
        {
            pageToIdx[i] = pageToIdx[i + 1];
        }
        pageToIdx[pageCount - 1] = tmp;
    }

    void ShiftPageRight()
    {
        // 맨 뒤를 맨 앞으로 가져오기
        Transform last = scrollRect.content.GetChild(scrollRect.content.childCount - 1);
        last.SetAsFirstSibling();
        
        // 인덱스 갱신
        int tmp = pageToIdx[pageCount - 1];
        for (int i = pageCount - 1; i > 0; i--)
        {
            pageToIdx[i] = pageToIdx[i - 1];
        }
        pageToIdx[0] = tmp;
    }

    
}
