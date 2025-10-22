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

    private int _pageCount = 2;
    private int _currentPageIndex = 0;
    private Coroutine _snapCoroutine;

    private void Awake()
    {
        if (_scrollRect != null)
        {
            _scrollRect.onValueChanged.AddListener(OnScrollChanged);
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
        _currentPageIndex = Mathf.RoundToInt(pos.x * (_pageCount - 1));

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
        float targetNormalizedPos = (float)_currentPageIndex / (_pageCount - 1);
        float startPos = _scrollRect.horizontalNormalizedPosition;

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float newPos = Mathf.Lerp(startPos, targetNormalizedPos, t);

            _scrollRect.horizontalNormalizedPosition = newPos;
            yield return null;
        }

        _scrollRect.horizontalNormalizedPosition = targetNormalizedPos;
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
