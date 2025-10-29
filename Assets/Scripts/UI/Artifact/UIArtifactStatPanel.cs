using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public struct StatBarViewModel
{
    public float Bonus;
    public List<Color> SegmentColors;
}

public struct StatPanelViewModel
{
    public StatBarViewModel PlayerAtk;
    public StatBarViewModel PlayerHp;
    public StatBarViewModel PlayerSpd;
    public StatBarViewModel PlayerAura;
    public StatBarViewModel MeleeAtk;
    public StatBarViewModel MeleeHp;
    public StatBarViewModel RangedAtk;
    public StatBarViewModel RangedHp;
}

public class UIArtifactStatPanel : MonoBehaviour, IEndDragHandler
{
    [Header("페이지 참조")]
    [SerializeField] private UIArtifactStatPlayerPage _playerStatPage;
    [SerializeField] private UIArtifactUnitStatPage _unitStatPage;

    [Header("스와이프 UI 요소")]
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private Image[] _paginationDots;

    //private int _pageCount = 2;
    private int _currentPageIndex = 0;
    private Coroutine _snapCoroutine;

    private RectTransform _contentRect;
    private HorizontalLayoutGroup _horizentalLayoutGroup;

    private void Awake()
    {
        if (_scrollRect != null)
        {
            _scrollRect.onValueChanged.AddListener(OnScrollChanged);

            if (_scrollRect.content != null )
            {
                _contentRect = _scrollRect.content;
                _horizentalLayoutGroup = _contentRect.GetComponent<HorizontalLayoutGroup>();
            }
            else
            {
                Debug.LogError("ScrollRect content null임");
            }
        }
        else
        {
            Debug.LogError("scrollRect null임");
        }

        UpdatePage(0);
    }

    public void RefreshStatPanelUI(StatPanelViewModel vm)
    {
        _playerStatPage.Refresh(vm);
        _unitStatPage.Refresh(vm);
    }

    private void OnScrollChanged(Vector2 pos)
    {
        if (_snapCoroutine != null)
        {
            UpdatePage(_currentPageIndex);
            return;
        }

        if (_contentRect == null || _scrollRect.viewport == null) return;

        float contentPosX = _contentRect.anchoredPosition.x;
        float viewportWidth = _scrollRect.viewport.rect.width;

        float spacing = (_horizentalLayoutGroup != null) ? _horizentalLayoutGroup.spacing : 0f;

        float PageStartX0 = 0;
        float PageStartX1 = -(viewportWidth + spacing);

        if (Mathf.Abs(contentPosX - PageStartX0) <= Mathf.Abs(contentPosX - PageStartX1))
        {
            _currentPageIndex = 0;
        }
        else
        {
            _currentPageIndex = 1;
        }

        UpdatePage(_currentPageIndex);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("드래그 끝나는 거 정상적으로 확인");
        if (_snapCoroutine != null)
        {
            StopCoroutine(_snapCoroutine);
        }
        _snapCoroutine = StartCoroutine(PageSwap());
    }

    private IEnumerator PageSwap()
    {
        Debug.Log("페이지 넘어가는 코루틴 잘 시작함");

        if (_contentRect == null || _scrollRect.viewport == null)
        {
            Debug.LogError("Content RectTransform 아니면 Viewport RectTransform null임");
            yield break;
        }

        float viewportWidth = _scrollRect.viewport.rect.width;
        float spacing = (_horizentalLayoutGroup != null) ? _horizentalLayoutGroup.spacing : 0f;

        float targetX = -_currentPageIndex * (viewportWidth + spacing);

        float startX = _contentRect.anchoredPosition.x;

        Debug.Log($"변수 계산까지 잘 작동함. {_currentPageIndex} 페이지, {targetX} 타겟, {startX} 시작 지점");

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            t = 1f - (1f - t) * (1f - t);

            float newX = Mathf.Lerp(startX, targetX, t);

            Vector2 newPos = _contentRect.anchoredPosition;
            newPos.x = newX;
            _contentRect.anchoredPosition = newPos;

            yield return null;
        }

        Vector2 finalPos = _contentRect.anchoredPosition;
        finalPos.x = targetX;
        _contentRect.anchoredPosition = finalPos;
        Debug.Log("페이지 넘기는 코루틴 잘 끝남");
        _snapCoroutine = null;
    }

    private void UpdatePage(int activeIndex)
    {
        for (int i = 0; i < _paginationDots.Length; i++)
        {
            if (_paginationDots[i] != null)
            {
                _paginationDots[i].color = (i == activeIndex) ? Color.white : Color.gray;
            }
        }
    }
}
