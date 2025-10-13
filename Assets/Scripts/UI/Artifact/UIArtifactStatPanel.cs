using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
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

public class UIArtifactStatPanel : BaseUI
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

    //[Header("MeleeAtk 스탯 UI")]
    //[SerializeField] private Image[] _MeleeAtkSegments;
    //[SerializeField] private TextMeshProUGUI _MeleeAtkText;

    //[Header("MeleeHp 스탯 UI")]
    //[SerializeField] private Image[] _MeleeHpSegments;
    //[SerializeField] private TextMeshProUGUI _MeleeHpText;

    //[Header("RangedAtk 스탯 UI")]
    //[SerializeField] private Image[] _RangedAtkSegments;
    //[SerializeField] private TextMeshProUGUI _RangedAtkText;

    //[Header("RangedHp 스탯 UI")]
    //[SerializeField] private Image[] _RangedHpSegments;
    //[SerializeField] private TextMeshProUGUI _RangedHpText;

    public void RefreshStatPanelUI(StatPanelViewModel vm)
    {
        UpdateStatSegment(_playerAtkSegments, _playerAtkText, "공격력", vm.PlayerAtk);
        UpdateStatSegment(_PlayerHpSegments, _PlayerHpText, "체력", vm.PlayerHp);
        UpdateStatSegment(_PlayerSpdSegments, _PlayerSpdText, "이동 속도", vm.PlayerSpd);
        UpdateStatSegment(_PlayerAuraSegments, _PlayerAuraText, "오라 크기", vm.PlayerAura);

        //UpdateStatSegment(_MeleeAtkSegments, _MeleeAtkText, "근거리 유닛 공격력", vm.MeleeAtk);
        //UpdateStatSegment(_MeleeHpSegments, _MeleeHpText, "근거리 유닛 체력", vm.MeleeHp);

        //UpdateStatSegment(_RangedAtkSegments, _RangedAtkText, "원거리 유닛 공격력", vm.RangedAtk);
        //UpdateStatSegment(_RangedHpSegments, _RangedHpText, "원거리 유닛 체력", vm.RangedHp);
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
