using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class Modifiercalculator
{
    private static Dictionary<(EffectTarget, StatType), float> _multiplierCache = new Dictionary<(EffectTarget, StatType), float>();

    private const float RewardBonusPerPoint = 3.0f;

    public static void EndBattle()
    {
        _multiplierCache?.Clear();
    }

    public static float GetMultiplier(EffectTarget target, StatType type, BaseUnitData unitData)
    {
        if (HasCondition(target, type))
        {
            float bonus = CalculateStatBonus(target, type, unitData);
            return bonus / 100f;
        }

        if (_multiplierCache.TryGetValue((target, type), out float value))
            return value;

        float bonusPer = CalculateStatBonus(target, type);
        float multiplier = bonusPer / 100f;

        _multiplierCache[(target, type)] = multiplier;

        return multiplier;
    }

    //public static float GetSetValue(EffectTarget target, StatType type)
    //{
    //    StageDestinyData destiny = PlayerDataManager.Instance.currentDestiny;

    //    if (destiny != null && destiny.modifiers != null)
    //    {
    //        foreach (var modifier in destiny.modifiers)
    //        {
    //            if (modifier.effectTarget == target && modifier.statType == type && modifier.valueModificationType == ValueModificationType.Set)
    //            {
    //                return modifier.value;
    //            }
    //        }
    //    }

    //    Dictionary<int, int> challenges = PlayerDataManager.Instance.activeChallenges;

    //    if (challenges != null)
    //    {
    //        foreach (var challenge in challenges)
    //        {
    //            int id = challenge.Key;
    //            int iv = challenge.Value;

    //            StageChallengeData challengeData = DataManager.Instance.StageModifierData.GetData(id) as StageChallengeData;

    //            if (challengeData != null && challengeData.effectTarget == target && challengeData.statType == type && challengeData.valueModificationType == ValueModificationType.Set)
    //            {
    //                return 
    //            }
    //        }
    //    }
    //}

    #region 효과별 보너스 값 계산
    private static float CalculateStatBonus(EffectTarget target, StatType type, BaseUnitData unitData = null)
    {
        float totalBonus = 0f;

        totalBonus += CalculateDestinyBonus(target, type, unitData);
        totalBonus += CalculateChallengeBonus(target, type);

        return totalBonus;
    }

    private static float CalculateDestinyBonus(EffectTarget target, StatType type, BaseUnitData unitData)
    {
        StageDestinyData destiny = PlayerDataManager.Instance.currentDestiny;

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

            if (!CheckConditionType(modifier, unitData))
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

    public static float GetRewardMultiplier()
    {
        Dictionary<int, int> challenges = PlayerDataManager.Instance.activeChallenges;

        if (challenges == null || challenges.Count == 0)
            return 1f;

        float totalPoint = 0f;

        foreach (var challenge in challenges)
        {
            int id = challenge.Key;
            int level = challenge.Value;

            if (level <= 0)
                continue;

            StageChallengeData challengeData = DataManager.Instance.StageModifierData.GetData(id) as StageChallengeData;

            if (challengeData == null)
                continue;

            totalPoint += challengeData.pointPerLevel * level;
        }

        float bonusPercent = totalPoint * RewardBonusPerPoint;
        return 1f + bonusPercent / 100f;
    }
    #endregion

        #region 조건 체크
    private static bool HasCondition(EffectTarget target, StatType type)
    {
        StageDestinyData destiny = PlayerDataManager.Instance.currentDestiny;

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

    private static bool CheckConditionType(StageDestinyModifier modifier, BaseUnitData unitData)
    {
        if (modifier.conditionType == ConditionType.None)
            return true;

        if (unitData == null)
        {
            Debug.LogWarning($"유닛 캐릭터 null이라 효과 도전 운명 적용 안 돼용 {unitData.unitName}");
            return false;
        }

        switch (modifier.conditionType)
        {
            case ConditionType.IsDifferentNation:
                return CheckIsDiffrentNation(unitData);

            case ConditionType.SameNationCount:
                return CheckSameNationCount(unitData, modifier.valueConditionOperater, modifier.conditionValue);
            
            default:
                return false;
        }
    }

    private static bool CheckIsDiffrentNation(BaseUnitData unitData)
    {
        if (unitData != null)
        {
            // 국가? 팩션? 관련 로직 생기면 구현해야 함
            return false;
        }

        return false;
    }

    private static bool CheckSameNationCount(BaseUnitData unitData, ValueConditionOperater operater, float conditionValue)
    {
        if (unitData != null)
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

    // 현재 덱 유닛 데이터 리스트 
    private static List<BaseUnitData> GetcurrentDeckUnits()
    {
        if (PlayerDataManager.Instance == null)
            return new List<BaseUnitData>();

        int activeDeckIndex = PlayerDataManager.Instance.ActiveDeckIndex;
        if (PlayerDataManager.Instance.DeckPresets.TryGetValue(activeDeckIndex, out DeckData deckData))
        {
            return deckData.BaseUnitDatas.Where(data => data != null).ToList();
        }
        return new List<BaseUnitData>();
    }

    // 유닛 시너지 추출용 함수
    private static UnitSynergyType GetNationSynergy(BaseUnitData unitData)
    {
        if (unitData == null) return UnitSynergyType.None;

        if ((unitData.synergyType & UnitSynergyType.Kingdom) != 0)
        {
            return UnitSynergyType.Kingdom;
        }

        if ((unitData.synergyType & UnitSynergyType.Empire) != 0)
        {
            return UnitSynergyType.Empire;
        }

        return UnitSynergyType.None;
    }
    #endregion
}
