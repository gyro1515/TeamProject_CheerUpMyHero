using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class Modifiercalculator
{
    private static Dictionary<(EffectTarget, StatType), float> _multiplierCache;
    private static bool _isBattleActive = false;

    public static void StartBattle()
    {
        Debug.Log("배틀 시작");
        _multiplierCache = new Dictionary<(EffectTarget, StatType), float>();
        _isBattleActive = true;
    }

    public static void EndBattle()
    {
        _multiplierCache?.Clear();
        _isBattleActive = false;
    }

    public static float GetMultiplier(EffectTarget target, StatType type, BaseCharacter character)
    {
        if (!_isBattleActive) return 1f;

        if (HasCondition(target, type))
        {
            float bonus = CalculateStatBonus(target, type, character);
            return bonus / 100f;
        }

        if (_multiplierCache.TryGetValue((target, type), out float value))
            return value;

        float bonusPer = CalculateStatBonus(target, type);
        float multiplier = bonusPer / 100f;

        _multiplierCache[(target, type)] = multiplier;

        return multiplier;
    }

    #region 효과별 보너스 값 계산
    private static float CalculateStatBonus(EffectTarget target, StatType type, BaseCharacter character = null)
    {
        float totalBonus = 0f;

        totalBonus += CalculateDestinyBonus(target, type, character);
        totalBonus += CalculateChallengeBonus(target, type);

        return totalBonus;
    }

    private static float CalculateDestinyBonus(EffectTarget target, StatType type, BaseCharacter character)
    {
        StageDestinyData destiny = PlayerDataManager.Instance.currentDastiny;

        if (destiny == null)
        {
            Debug.Log("운명 효과 null임");
            return 0f;
        }
        
        if (destiny.modifiers == null || destiny.modifiers.Count == 0)
        {
            Debug.Log("운명 효과 상세 데이터 없음. SO나 엑셀 점검 필요해요");
            return 0f;
        }

        float bonusValue = 0f;

        foreach (var modifier in destiny.modifiers)
        {
            if (modifier.effectTarget != target || modifier.statType != type)
                continue;

            if (!CheckConditionType(modifier, character))
                continue;

            bonusValue += GetDestinyValue(modifier);
        }

        return bonusValue;
    }

    private static float CalculateChallengeBonus(EffectTarget target, StatType type)
    {
        Dictionary<int, int> challenges = PlayerDataManager.Instance.activeChallenges;

        if (challenges == null || challenges.Count == 0)
            return 0f;
        
        float bonusValue = 0f;

        foreach (var challenge in challenges)
        {
            int id = challenge.Key;
            int level = challenge.Value;

            if (level <= 0)
                continue;

            StageChallengeData challengeData = DataManager.Instance.StageModifierData.GetData(id) as StageChallengeData;

            if (challengeData == null)
                continue;

            if (challengeData.effectTarget != target || challengeData.statType != type)
                continue;

            float value = challengeData.valuePerLevel * level;

            bonusValue += GetChallengeValue(challengeData.valueModificationType, value);
        }

        return bonusValue;
    }
    #endregion

    #region 조건 체크
    private static bool HasCondition(EffectTarget target, StatType type)
    {
        StageDestinyData destiny = PlayerDataManager.Instance.currentDastiny;

        if (destiny != null && destiny.modifiers != null)
        {
            foreach (StageDestinyModifier modifier in destiny.modifiers)
            {
                if (modifier.effectTarget == target &&
                    modifier.statType == type &&
                    modifier.conditionType != ConditionType.None)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool CheckConditionType(StageDestinyModifier modifier, BaseCharacter character)
    {
        if (modifier.conditionType == ConditionType.None)
            return true;

        if (character == null)
        {
            Debug.LogWarning($"유닛 캐릭터 null이라 효과 도전 운명 적용 안 돼용 {character.name}");
            return false;
        }

        switch (modifier.conditionType)
        {
            case ConditionType.IsDifferentNation:
                return CheckIsDiffrentNation(character);

            case ConditionType.SameNationCount:
                return CheckSameNationCount(character, modifier.valueConditionOperater, modifier.conditionValue);
            
            default:
                return false;
        }
    }

    private static bool CheckIsDiffrentNation(BaseCharacter character)
    {
        if (character is BaseUnit unit && unit.UnitData != null)
        {
            // 국가? 팩션? 관련 로직 생기면 구현해야 함
            return false;
        }

        return false;
    }

    private static bool CheckSameNationCount(BaseCharacter character, ValueConditionOperater operater, float conditionValue)
    {
        if (character is BaseUnit unit && unit.UnitData != null)
        {
            // 국가? 팩션? 관련 로직 생기면 구현해야 함
            return false;
        }
        
        return false;
    }
    #endregion

    #region 값 추출
    private static float GetDestinyValue(StageDestinyModifier modifier)
    {
        switch (modifier.valueModificationType)
        {
            case ValueModificationType.Percentage:
                return modifier.value;

            case ValueModificationType.Absolute:
                return modifier.value;

            case ValueModificationType.Set:
                return 0f;

            default:
                return 0f;
        }
    }

    private static float GetChallengeValue(ValueModificationType modificationType, float value)
    {
        switch (modificationType)
        {
            case ValueModificationType.Percentage:
                return value;

            case ValueModificationType.Absolute:
                return value;

            default:
                return 0f;
        }
    }
    #endregion
}
