using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChallengeElement : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _effectValue;
    [SerializeField] private TextMeshProUGUI _effectTargetText;
    [SerializeField] private TextMeshProUGUI _StatTypeText;
    [SerializeField] private TextMeshProUGUI _points;
    [SerializeField] private TextMeshProUGUI _level;
    [SerializeField] private Button _plusButton;
    [SerializeField] private Button _minusButton;
    [SerializeField] private Image _challengeIcon;
    [SerializeField] private Image _statIcon;

    private StageChallengeData _challengeData;
    private int _curLv = 0;

    public event Action<int, int> OnElementsLevelChanged;

    public void SetElements(StageChallengeData data)
    {
        _challengeData = data;
        _name.text = _challengeData.name;
        
        _StatTypeText.text = TranslateStatType(_challengeData.statType);
        _effectTargetText.text = TranslateEffectTarget(_challengeData.effectTarget);

        _plusButton.onClick.AddListener(OnPlusButtonClicked);
        _minusButton.onClick.AddListener(OnMinusButtonClicked);

        RefreshUI();
    }

    private void OnPlusButtonClicked()
    {
        if (_curLv < _challengeData.maxLevel)
        {
            _curLv++;
            OnElementsLevelChanged?.Invoke(_challengeData.idNumber, _curLv);
            RefreshUI();
        }
    }

    private void OnMinusButtonClicked()
    {
        if (_curLv > 0)
        {
            _curLv--;
            OnElementsLevelChanged?.Invoke(_challengeData.idNumber, _curLv);
            RefreshUI() ;
        }
    }

    private void RefreshUI()
    {
        float effectValue = _challengeData.valuePerLevel * _curLv;
        int pointValue = _challengeData.pointPerLevel * _curLv;

        _effectValue.text = effectValue.ToString("+#;-#;+0") + "%";
        _points.text = $"+{pointValue}";
        _level.text = $"{_curLv}";

        _minusButton.interactable = _curLv > 0;
        _plusButton.interactable = _curLv < _challengeData.maxLevel;
    }

    public void ResetLevel()
    {
        _curLv = 0;
        RefreshUI();
    }

    private string TranslateEffectTarget(EffectTarget target)
    {
        switch (target)
        {
            case EffectTarget.Player:
                return "플레이어";
            case EffectTarget.PlayerUnit:
                return "아군 유닛";
            case EffectTarget.EnemyUnit:
                return "적 유닛";
            case EffectTarget.KnightUnit:
                return "기사 유닛";
            case EffectTarget.RangedUnit:
                return "원거리 유닛";
            case EffectTarget.DifferentNation:
                return "다른 국가 유닛";
            case EffectTarget.MeleeUnit:
                return "근거리 유닛";
            case EffectTarget.Hero:
                return "용사";
            case EffectTarget.SameNation:
                return "같은 국가 유닛";
            case EffectTarget.System:
                return "특수 효과;";
            default:
                return "미구현";
        }
    }

    private string TranslateStatType(StatType type)
    {
        switch (type)
        {
            case StatType.MaxHp:
                return "최대 체력";
            case StatType.AtkPower:
                return "공격력";
            case StatType.SpawnCost:
                return "소환 비용";
            case StatType.MoveSpeed:
                return "이동 속도";
            case StatType.SpawnCooldown:
                return "소환 쿨타임";
            case StatType.AuraRange:
                return "오라 크기";
            case StatType.MaxSpawnCount:
                return "소환 가능 유닛";
            case StatType.Timer:
                return "영웅 타이머";
            default:
                return "미구현";
        }
    }
}
