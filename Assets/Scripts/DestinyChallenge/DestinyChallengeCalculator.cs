using System.Collections.Generic;
using UnityEngine;

public struct DestinyChallengeUnitCache
{
    public float HpBonusPercent;
    public float AtkBonusPercent;
    public float MoveSpeedBonusPercent;
    public float SpawnCostBonusPercent;
    public float AttackCooldownBonusPercent;
}

public class DestinyChallengeCalculator
{
    private readonly PlayerDataManager _data;
    private readonly StageDestinyData _destiny;
    private readonly Dictionary<int, int> _challenges;

    private List<BaseUnitData> _deckList;
    private Dictionary<UnitSynergyType, int> _nationCounts;

    private const float rangedUnitStandardCognizeRange = 2f;
    private const float rewardBonusPerPoint = 3.0f;

    public DestinyChallengeCalculator(PlayerDataManager data)
    {
        _data = data;
        _destiny = data.currentDestiny;
        _challenges = data.activeChallenges;
    }

    #region public 메서드 -> 직접 호출되는 메서드들
    // 운명, 도전 효과 계산해서 유닛별 보너스 스탯 구조체 만들어서 반환하는 메서드
    public Dictionary<int, DestinyChallengeUnitCache> Calculate()
    {
        Dictionary<int, DestinyChallengeUnitCache> result = new Dictionary<int, DestinyChallengeUnitCache> ();

        // 1. 덱 구성 분석 -> 덱 내의 유닛 리트스 받아오고 + 유닛 소속 개수 받아옴
        _deckList = GetCurrentDeckUnits();
        _nationCounts = UnitFactionAnalyze(_deckList);

        // 2. 조건부 효과 분석 -> 운명에 조건부 효과 있는지 + 있으면 각 유닛이 조건 만족하는지
        ConditionType conditionType = GetDestinyConditionType();
        Dictionary<int, bool> conditionResults = EvaluateConditions(conditionType);

        // 3. 유닛별 보너스 계산해서 result 딕셔너리에 저장
        for (int i = 0; i < _deckList.Count; i++)
        {
            BaseUnitData unit = _deckList[i];

            bool conditionMet = false;
            if (conditionResults.ContainsKey(unit.idNumber))
                conditionMet = conditionResults[unit.idNumber];

            DestinyChallengeUnitCache cache = CalculateUnitBonus(unit, conditionMet);
            result[unit.idNumber] = cache;
        }

        // 4. 결과 딕셔너리 반환
        return result;
    } 

    // 도전 포인트로 보너스 배율 도출하는 메서드
    public float CalculateRewardMultiplier()
    {
        float totalPoint = 0;

        Dictionary<int, int> challenges = _data.activeChallenges;
        if (challenges == null || challenges.Count == 0)
            return 1f;

        foreach (KeyValuePair<int, int> challenge in challenges)
        {
            int id = challenge.Key;
            int level = challenge.Value;

            if (level <= 0) continue;

            StageChallengeData challengeData = DataManager.Instance.StageModifierData.GetData(id) as StageChallengeData;
            if (challengeData == null) continue;

            totalPoint += challengeData.valuePerLevel * level;
        }

        float bonusPercent = totalPoint * rewardBonusPerPoint;
        return 1f + bonusPercent / 100f;
    }
    #endregion

    #region 유닛별 보너스 계산
    // 유닛별 최종 스탯 보너스 계산용 메서드
    // 보너스 스탯 구조체 생성 -> 운명 및 도전 효과 보너스 스탯 적용 -> 구조체 리턴
    private DestinyChallengeUnitCache CalculateUnitBonus(BaseUnitData unit, bool conditionCheckResult)
    {
        DestinyChallengeUnitCache cache = new DestinyChallengeUnitCache();

        ApplyDestinyBonus(ref cache, unit, conditionCheckResult);
        ApplyChallengeBonus(ref cache, unit);

        return cache;
    }

    // 유닛에 운명 효과 적용해도 되는 지 체크하는 예외처리 -> 스탯 더하는 메서드 호출
    private void ApplyDestinyBonus(ref DestinyChallengeUnitCache cache, BaseUnitData unit, bool conditionCheckResult)
    {
        if (_destiny == null || _destiny.modifiers == null) return;

        List<StageDestinyModifier> modifiers = _destiny.modifiers;

        for (int i = 0; i < modifiers.Count; i++)
        {
            // 조건부 효과인데 조건 충족 안 하는 경우 -> 스킵
            if (modifiers[i].conditionType != ConditionType.None && !conditionCheckResult)
                continue;

            // 조건부 계산 결과 Set 타입은 별도 처리(영웅 타이머) -> 스킵
            if (modifiers[i].valueModificationType == ValueModificationType.Set)
                continue;

            // 현재 효과 effectTarget에 부합하는 지 확인함. 부합 안 하면 -> 스킵
            if (!CheckTargetMatch(modifiers[i].effectTarget, unit))
                continue;

            // 스탯별 값 누적
            AddBonusToCache(ref cache, modifiers[i].statType, modifiers[i].value);
        }
    }

    // 유닛에 도전 효과 적용해도 되는 지 체크하는 예외처리 -> 스탯 더하는 메서드 호출
    private void ApplyChallengeBonus(ref DestinyChallengeUnitCache cache, BaseUnitData unit)
    {
        if (_challenges == null || _challenges.Count == 0) return;

        foreach (KeyValuePair<int, int> challenge in _challenges)
        {
            int id = challenge.Key;
            int level = challenge.Value;

            StageChallengeData challengeData = DataManager.Instance.StageModifierData.GetData(id) as StageChallengeData;

            // 특수 챌린지 효과는 이 방식으로 처리 X (덱 슬롯 봉인이나 영지 비활성화)
            if (challengeData.modifierSpecialEffect != ModifierSpecialEffect.None)
                continue;

            // 유닛이 도전 효과의 effect Target과 일치하는 지 확인함. 안 일치하면 스킵
            if (!CheckTargetMatch(challengeData.effectTarget, unit))
                continue;

            float value = challengeData.valuePerLevel * level;
            AddBonusToCache(ref cache, challengeData.statType, value);
        }
    }

    // 스탯 보너스 계산하는 메서드
    private void AddBonusToCache(ref DestinyChallengeUnitCache cache, StatType statType, float value)
    {
        switch (statType)
        {
            case StatType.MaxHp:
                cache.HpBonusPercent += value; break;

            case StatType.AtkPower:
                cache.AtkBonusPercent += value; break;

            case StatType.MoveSpeed:
                cache.MoveSpeedBonusPercent += value; break;

            case StatType.SpawnCost:
                cache.SpawnCostBonusPercent += value; break;

            case StatType.SpawnCooldown:
                cache.SpawnCostBonusPercent += value; break;
        }
    }
    #endregion

    #region 타겟 매칭
    // 유닛 데이터 받아서 현재 운명 / 도전 타겟에 해당하는 지 검사
    private bool CheckTargetMatch(EffectTarget effectTarget, BaseUnitData unitData)
    {
        if (unitData == null)
            return false;

        switch (effectTarget)
        {
            case EffectTarget.PlayerUnit:
                return true;

            case EffectTarget.MeleeUnit:
                return unitData.cognizanceRange <= rangedUnitStandardCognizeRange;

            case EffectTarget.RangedUnit:
                return unitData.cognizanceRange >= rangedUnitStandardCognizeRange;

            case EffectTarget.KnightUnit:
                return CheckTargetKnightUnit(unitData);

            case EffectTarget.Hero:
            case EffectTarget.Player:
            case EffectTarget.System:
            case EffectTarget.None:
                return false;   // 덱 안에 넣을 수 없는 타겟들은 모두 false 처리.

            case EffectTarget.SameNation:
            case EffectTarget.DifferentNation:
                return true;    // 어차피 조건부 효과 계산할 때 계산하니까 true

            default:
                return false;
        }
    }

    private bool CheckTargetKnightUnit(BaseUnitData unitData)
    {
        // return ((unitData.synergyType & UnitSynergyType.knight) != 0);

        // ↑ 지금 기사 유닛 시너지 타입이 없어서 나중에 생기면 저런 코드지 않을까 하고 만들어둠.
        // 기사 태그가 없으니 지금은 무조건 false 도출

        return false;
    }
    #endregion

    // 조건부 효과 평가 -> 조건부 결과 산출 : 조건 도출하고 + 유닛별 조건 부합 여부 딕셔너리 도출
    #region 조건부 효과 평가
    // 조건 평가해서 덱 리스트 내 유닛별 조건 부합 여부 딕셔너리 산출함
    private Dictionary<int, bool> EvaluateConditions(ConditionType conditionType)
    {
        Dictionary<int, bool> results = new Dictionary<int, bool>();

        if (conditionType == ConditionType.None)
            return results;

        switch (conditionType)
        {
            case ConditionType.SameNationCount:
                EvaluateSameNationCount(results);
                break;

            case ConditionType.IsDifferentNation:
                EvaluateDifferentNation(results);
                break;
        }

        return results;
    }

    // 현재 운명에서 조건 타입 추출 -> 조건 있는 지? 있으면 어떤 조건인지?
    private ConditionType GetDestinyConditionType()
    {
        if (_destiny == null || _destiny.modifiers == null)
            return ConditionType.None;

        List<StageDestinyModifier> modifiers = _destiny.modifiers;

        for (int i = 0; i < modifiers.Count; i++)
        {
            if (modifiers[i].conditionType != ConditionType.None)
                return modifiers[i].conditionType;
        }

        return ConditionType.None;
    }
    #endregion

    #region 조건 타입별 결과 산출
    // 내부분열 조건 검사 -> 각 유닛별 효과 적용 여부 캐싱
    private void EvaluateDifferentNation(Dictionary<int, bool> results)
    {
        bool conditionMet = _nationCounts.Count >= 2;

        for (int i = 0; i < _deckList.Count; i++)
        {
            results[_deckList[i].idNumber] = conditionMet;
        }
    }

    // 권력다툼 조건 검사 -> 유닛별 효과 적용 여부 캐싱
    private void EvaluateSameNationCount(Dictionary<int, bool> results)
    {
        float conditionValue = 2f;
        ValueConditionOperater conditionOp = ValueConditionOperater.GreaterThanOrEqual;

        // 1. 
        if (_destiny != null && _destiny.modifiers != null)
        {
            List<StageDestinyModifier> modifiers = _destiny.modifiers;
            for (int i = 0; i < modifiers.Count; i++)
            {
                if (modifiers[i].conditionType == ConditionType.SameNationCount)
                {
                    conditionValue = modifiers[i].conditionValue;
                    conditionOp = modifiers[i].valueConditionOperater;
                    break;
                }
            }
        }

        for (int i = 0; _deckList.Count > 0; i++)
        {
            BaseUnitData unit = _deckList[i];
            UnitSynergyType unitNation = GetUnitNation(unit);

            if (unitNation == UnitSynergyType.None)
            {
                results[unit.idNumber] = false;
                continue;
            }

            int count = 0;
            if (_nationCounts.ContainsKey(unitNation))
                count = _nationCounts[unitNation];

            bool conditionMet = CheckOperator(count, conditionOp, conditionValue);
            results[unit.idNumber] = conditionMet;
        }
    }
    #endregion

    #region 헬퍼 메서드
    // 현재 선택한 덱 리스트 뽑아오는 메서드
    private List<BaseUnitData> GetCurrentDeckUnits()
    {
        List<BaseUnitData> result = new List<BaseUnitData>();

        int activeDeckIndex = _data.ActiveDeckIndex;
        if (_data.DeckPresets.TryGetValue(activeDeckIndex, out DeckData deckData))
        {
            List<BaseUnitData> unitDatas = deckData.BaseUnitDatas;
            for (int i = 0; i < unitDatas.Count; i++)
            {
                if (unitDatas[i] != null)
                    result.Add(unitDatas[i]);
            }
        }

        return result;
    }

    // 유닛 데이터 받아서 팩션 도출하는 메서드
    private UnitSynergyType GetUnitNation(BaseUnitData unitData)
    {
        if (unitData == null)
            return UnitSynergyType.None;

        if ((unitData.synergyType & UnitSynergyType.Kingdom) != 0)
            return UnitSynergyType.Kingdom;

        if ((unitData.synergyType & UnitSynergyType.Empire) != 0)
            return UnitSynergyType.Empire;

        return UnitSynergyType.None;
    }

    // 뽑아온 덱 리스트 분석해서 각 팩션에 유닛 몇 개나 있는 지 확읺는 메서드
    private Dictionary<UnitSynergyType, int> UnitFactionAnalyze(List<BaseUnitData> units)
    {
        Dictionary<UnitSynergyType, int> nationCounts = new Dictionary<UnitSynergyType, int>();

        for (int i = 0; i < units.Count; i++)
        {
            UnitSynergyType nation = GetUnitNation(units[i]);

            if (nation == UnitSynergyType.None)
                continue;

            if (!nationCounts.ContainsKey(nation))
                nationCounts[nation] = 1;
            else
                nationCounts[nation]++;
        }

        return nationCounts;
    }

    // 비교 연산자로 비교하는 메서드 (조건 충족하는 지 확인할 때)
    private bool CheckOperator(float actual, ValueConditionOperater op, float expected)
    {
        switch (op)
        {
            case ValueConditionOperater.Equals:
                return Mathf.Approximately(actual, expected);

            case ValueConditionOperater.NotEquals:
                return !Mathf.Approximately(actual, expected);

            case ValueConditionOperater.Greater:
                return actual > expected;

            case ValueConditionOperater.Less:
                return actual < expected;

            case ValueConditionOperater.GreaterThanOrEqual:
                return actual >= expected;

            case ValueConditionOperater.LessThanOrEqual:
                return actual <= expected;

            default:
                return false;
        }
    }
    #endregion
}
