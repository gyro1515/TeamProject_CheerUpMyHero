using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LayoutElement))]
public class UIArtifactStatPlayerPage : MonoBehaviour
{
    [Header("PlayerAtk 스탯 UI")]
    [SerializeField] private Image[] _playerAtkSegments;
    [SerializeField] private TextMeshProUGUI _playerAtkText;

    [Header("PlayerHp 스탯 UI")]
    [SerializeField] private Image[] _PlayerHpSegments;
    [SerializeField] private TextMeshProUGUI _PlayerHpText;

    [Header("PlayerSpd 스탯 UI")]
    [SerializeField] private Image[] _PlayerSpdSegments;
    [SerializeField] private TextMeshProUGUI _PlayerSpdText;

    [Header("PlayerAura 스탯 UI")]
    [SerializeField] private Image[] _PlayerAuraSegments;
    [SerializeField] private TextMeshProUGUI _PlayerAuraText;

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
        UpdateStatSegment(_playerAtkSegments, _playerAtkText, "공격력", vm.PlayerAtk);
        UpdateStatSegment(_PlayerHpSegments, _PlayerHpText, "체력", vm.PlayerHp);
        UpdateStatSegment(_PlayerSpdSegments, _PlayerSpdText, "이동 속도", vm.PlayerSpd);
        UpdateStatSegment(_PlayerAuraSegments, _PlayerAuraText, "오라 크기", vm.PlayerAura);
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
