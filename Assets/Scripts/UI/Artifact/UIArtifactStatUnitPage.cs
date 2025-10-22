using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(LayoutElement))]
public class UIArtifactUnitStatPage : MonoBehaviour
{
    [Header("MeleeAtk 스탯 UI")]
    [SerializeField] private Image[] _MeleeAtkSegments;
    [SerializeField] private TextMeshProUGUI _MeleeAtkText;

    [Header("MeleeHp 스탯 UI")]
    [SerializeField] private Image[] _MeleeHpSegments;
    [SerializeField] private TextMeshProUGUI _MeleeHpText;

    [Header("RangedAtk 스탯 UI")]
    [SerializeField] private Image[] _RangedAtkSegments;
    [SerializeField] private TextMeshProUGUI _RangedAtkText;

    [Header("RangedHp 스탯 UI")]
    [SerializeField] private Image[] _RangedHpSegments;
    [SerializeField] private TextMeshProUGUI _RangedHpText;

    private LayoutElement _layoutElement;
    private ViewportSizeNotifier _viewportSizeNotifier;

    private void Awake()
    {
        _layoutElement = GetComponent<LayoutElement>();
        _viewportSizeNotifier = GetComponentInParent<ViewportSizeNotifier>();
    }

    private void Start()
    {
        if (_viewportSizeNotifier != null)
        {
            _viewportSizeNotifier.OnViewportChanged += HandleViewPortSizeChanged;

            _viewportSizeNotifier.NotifyCurrentSize();
        }
    }

    private void OnDestroy()
    {
        _viewportSizeNotifier.OnViewportChanged -= HandleViewPortSizeChanged;
    }

    private void HandleViewPortSizeChanged(Vector2 newSize)
    {
        if (_layoutElement != null)
        {
            _layoutElement.preferredHeight = newSize.y;
            _layoutElement.preferredWidth = newSize.x;
        }
    }

    public void Refresh(StatPanelViewModel vm)
    {
        UpdateStatSegment(_MeleeAtkSegments, _MeleeAtkText, "공격력", vm.MeleeAtk);
        UpdateStatSegment(_MeleeHpSegments, _MeleeHpText, "체력", vm.MeleeHp);
        UpdateStatSegment(_RangedAtkSegments, _RangedAtkText, "공격력", vm.RangedAtk);
        UpdateStatSegment(_RangedHpSegments, _RangedHpText, "체력", vm.RangedHp);
    }

    public void UpdateStatSegment(Image[] segments, TextMeshProUGUI text, string statName, StatBarViewModel barVm)
    {
        text.text = $"{statName} {barVm.Bonus}% 증가";

        for (int i = 0; i < segments.Length; i++)
        {
            if (i < barVm.SegmentColors.Count)
            {
                segments[i].gameObject.SetActive(true);
                segments[i].color = barVm.SegmentColors[i];
            }
            else
            {
                segments[i].gameObject.SetActive(false);
            }
        }
    }
}
