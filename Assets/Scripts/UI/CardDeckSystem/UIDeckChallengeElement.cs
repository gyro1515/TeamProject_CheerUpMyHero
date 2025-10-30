using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIDeckChallengeElement : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _effectValue;
    [SerializeField] private TextMeshProUGUI _effectTargetText;
    [SerializeField] private TextMeshProUGUI _StatTypeText;
    [SerializeField] private TextMeshProUGUI _level;
    public void SetElement(StageChallengeData _challenge, int lv)
    {
        _name.text = _challenge.name;

        float effectValue = _challenge.valuePerLevel * lv;
        _effectValue.text = effectValue.ToString("+#;-#;+0") + "%";

        _effectTargetText.text = TranslateEffectTarget(_challenge.effectTarget);
        _StatTypeText.text = TranslateStatType(_challenge.statType);
        _level.text = $"Lv.{lv}";
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
                return target.ToString();
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
                return type.ToString();
        }
    }
}
