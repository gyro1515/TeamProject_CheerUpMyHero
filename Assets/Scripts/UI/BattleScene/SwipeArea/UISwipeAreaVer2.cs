using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISwipeAreaVer2 : MonoBehaviour
{
    [Header("스와이프 UI 세팅")]
    [SerializeField] RectTransform viewport; // 스와이프 영역
    [SerializeField] RectTransform[] pages;  // 자식 페이지들
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] GameObject swipeBgGO; // 스와이프시 클릭 방지용 배경
    [SerializeField] Button toNextArea;  // 스와이프 버튼
    [SerializeField] Image toNextAreaImg;
    [SerializeField] Sprite toRightSprite;
    [SerializeField] Sprite toLeftSprite;
    [SerializeField] TextMeshProUGUI toNextAreaText;  // 스와이프 버튼 텍스트, 나중에는 이미지?
    Vector2 preSize = Vector2.zero;
    int pageCount;
    float[] pagePositions;
    int curIdx = 0;
    private void Awake()
    {
        ResizePages();
        toNextArea.onClick.AddListener(MoveToNext);
        pageCount = pages.Length;
        pagePositions = new float[pageCount];
        for (int i = 0; i < pageCount; i++)
        {
            pagePositions[i] = (float)i / (pageCount - 1); // 0 ~ 1 구간 분할
        }
        swipeBgGO.SetActive(false);
    }
    void LateUpdate()
    {
        // 기기 회전/리사이즈 대응, 테스트때 모든 기종 변화에 대응하도록
        ResizePages();
    }
    void MoveToNext()
    {
        int nextIdx = (curIdx + 1) % pageCount; // 가독성을 위해 선언
        StartCoroutine(SmoothMove(curIdx, nextIdx)); 
        curIdx = nextIdx;
    }
    IEnumerator SmoothMove(int from, int to)
    {
        toNextArea.enabled = false; // 이동 중 버튼 비활성화
        swipeBgGO.SetActive(true); // 이동 중 다른 영역 클릭 방지

        float elapsed = 0f;
        float start = pagePositions[from];
        float duration = 0.2f;
        float end = pagePositions[to];

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            //t *= t; // 점점 빨라지도록
            //t = 1f - Mathf.Pow((1f - t), 2); // 점점 느려지도록
            t = t * t * (3 - 2 * t); // 빨라졌다 느려짐
            // 4가지 버전: 등속, 점점 빨라짐, 점점 느려짐, 빨라졌다 느려짐
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(start, end, t);
            yield return null;
        }
        toNextArea.enabled = true; // 이동 후 버튼 활성화
        toNextAreaText.text = curIdx == 0 ? "→" : "←"; // 첫 페이지면 오른쪽, 아니면 왼쪽 화살표
        toNextAreaImg.sprite = curIdx == 0 ? toRightSprite : toLeftSprite;
        swipeBgGO.SetActive(false); // 이동 후 다른 영역 클릭 가능
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
}
