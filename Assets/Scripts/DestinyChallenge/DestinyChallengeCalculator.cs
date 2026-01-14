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

    private List<BaseUnitData> _deckUnits;
    private Dictionary<UnitSynergyType, int> _nationCounts;

    private const float RangedUnitStandardCognizeRange = 2f;

    public DestinyChallengeCalculator(PlayerDataManager data)
    {
        _data = data;
    }

    #region 계산용 public 메서드
    public Dictionary<int, DestinyChallengeUnitCache> Calculate()
    {
        Dictionary<int, DestinyChallengeUnitCache> result = new Dictionary<int, DestinyChallengeUnitCache> ();

        // 1. 덱 유닛 구성 받아오고 + 소속 구성 받아옴
        _deckUnits = GetCurrentDeckUnits();
        _nationCounts = GetDeckComposition(_deckUnits);

        // 2. 조건부 효과 체크하고 유닛마다 조건에 해당되는 지 체크
        ConditionType conditionType = GetDestinyConditionType();
        Dictionary<int, bool> conditionResults = CheckConditionType(conditionType);

        // 3. 덱에 있는 유닛마다 스탯 보너스 구조체 만들어서 딕셔너리 구성
        foreach (BaseUnitData unit in _deckUnits)
        {
            bool ConditionMet = false;

            if (conditionResults.ContainsKey(unit.idNumber))
                ConditionMet = conditionResults[unit.idNumber];

            DestinyChallengeUnitCache cache = CalculateUnitBonus(unit, ConditionMet);
            result[unit.idNumber] = cache;
        }

        return result;
    }

    public float CalculateRewaldMultiplier()
    {
        const float RewardBonusPerPoint = 3.0f;
    }
    #endregion

    #region 스탯 보너스 계산 메서드
    private DestinyChallengeUnitCache CalculateUnitBonus(BaseUnitData unit, bool ConditionMet)
    {
        DestinyChallengeUnitCache cache = new DestinyChallengeUnitCache ();

        ApplyDestinyStatBonus(ref cache, unit, ConditionMet);
    }

    private void ApplyDestinyStatBonus(ref DestinyChallengeUnitCache cache, BaseUnitData unit, bool ConditionMet)
    {
        StageDestinyData destiny = _data.currentDestiny;
        
        foreach (StageDestinyModifier modifier in destiny.modifiers)
        {
            // 조건부 효과인데 조건 충족 못 했으면 continue.
            if (modifier.conditionType != ConditionType.None && !ConditionMet)
                continue;

            // Set 타입(영웅 타이머) 따로 처리함.
            if (modifier.valueModificationType == ValueModificationType.Set)
                continue;

            // 대상
        }
    }
    #endregion

    #region 조건부 효과 조건 체크
    // 운명 효과 중에 조건부 효과 있으면 반환함. 없으면 none 반환.
    private ConditionType GetDestinyConditionType()
    {
        StageDestinyData destiny = _data.currentDestiny;
        
        if (destiny == null || destiny.modifiers == null)
            return ConditionType.None;

        foreach (StageDestinyModifier modifier in destiny.modifiers)
        {
            if (modifier.conditionType != ConditionType.None)
                return modifier.conditionType;
        }

        return ConditionType.None;
    }

    // 운명 조건 넣어서 <유닛 idNumber, 조건 충족 여부> 딕셔너리 반환하는 메서드
    private Dictionary<int, bool> CheckConditionType(ConditionType conditionType)
    {
        Dictionary<int, bool> results = new Dictionary<int, bool> ();

        if (conditionType == ConditionType.None)
            return results;

        switch (conditionType)
        {
            case ConditionType.IsDifferentNation:
                CheckDiffrentNation(results);
                break;

            case ConditionType.SameNationCount:
                CheckSameNationCount(results);
                break;
        }

        return results;
    }
    #endregion

    #region 조건별 체크 메서드
    // 운명이 내부분열 : 소속이 두 개 이상이면 모든 유닛에게 true.
    private void CheckDiffrentNation(Dictionary<int, bool> results)
    {
        bool conditionMet = _nationCounts.Count >= 2;

        foreach (BaseUnitData unit in _deckUnits)
        {
            results[unit.idNumber] = conditionMet;
        }
    }

    // 운명이 권력다툼 : 같은 소속 유닛이 두 개 이상이면 해당 소속 유닛들에 true.
    private void CheckSameNationCount(Dictionary<int, bool> results)
    {
        // 선언부 : 임시값으로 할당함. 반복문 제대로 작동 안할 때 사용함.
        float conditionValue = 2f;
        ValueConditionOperater operater = ValueConditionOperater.GreaterThanOrEqual;
        
        StageDestinyData destiny = _data.currentDestiny;

        if (destiny == null || destiny.modifiers == null)
            return;

        // 운명 데이터 내에 있는 연산용 데이터 가져옴.
        // modifier가 List 형식이라 반복문 한 번 돌면서 할당해줘야 함. 
        foreach (StageDestinyModifier modifier in destiny.modifiers)
        {
            if (modifier.conditionType == ConditionType.SameNationCount)
            {
                conditionValue = modifier.conditionValue;
                operater = modifier.valueConditionOperater;
                break;
            }
        }

        // 덱 리스트에 있는 유닛 가져와서 겹치는 소속 있는지 찾고 있으면 true.
        foreach (BaseUnitData unit in _deckUnits)
        {
            UnitSynergyType unitNation = GetUnitNation(unit);

            if (unitNation == UnitSynergyType.None)
            {
                results[unit.idNumber] = false;
                continue;
            }

            int count = 0;

            if (_nationCounts.ContainsKey(unitNation))
            {
                count = _nationCounts[unitNation];
            }

            bool conditionMet = CheckOperator(count, operater, conditionValue);
            results[unit.idNumber] = conditionMet;
        }
    }
    #endregion

    #region 타겟 매칭 확인
    private bool IsTargetMatch(EffectTarget effectTarget, BaseUnitData unitData)
    {
        if (unitData == null)
            return false;

        switch (effectTarget)
        {
            case EffectTarget.PlayerUnit:
                return true;

            case EffectTarget.MeleeUnit:
                return unitData.cognizanceRange < RangedUnitStandardCognizeRange;

            case EffectTarget.RangedUnit:
                return unitData.cognizanceRange >= RangedUnitStandardCognizeRange;

            case EffectTarget.KnightUnit:
                // TODO: 기사 유닛 판단 기준 정해지면 구현
                return IsKnightUnit(unitData);

            case EffectTarget.Hero:
                // TODO: 영웅 판단 기준 정해지면 구현
                return IsHeroUnit(unitData);

            case EffectTarget.EnemyUnit:
                // 적 유닛은 덱에 없으므로 false
                return false;

            case EffectTarget.SameNation:
            case EffectTarget.DifferentNation:
                // 조건부 효과에서 별도 처리됨
                return true;

            case EffectTarget.Player:
            case EffectTarget.System:
            case EffectTarget.None:
                return false;

            default:
                return false;
        }
    }

    private bool IsKnightUnit(BaseUnitData unitData)
    {
        // TODO: 기사 유닛 판단 기준 정해지면 구현
        return false;
    }

    private bool IsHeroUnit(BaseUnitData unitData)
    {
        // TODO: 영웅 판단 기준 정해지면 구현
        return false;
    }
    #endregion

    #region 헬퍼 메서드
    // 덱 구성 받아오는 메서드
    private List<BaseUnitData> GetCurrentDeckUnits()
    {
        List<BaseUnitData> result = new List<BaseUnitData>();

        int activeDeckIndex = _data.ActiveDeckIndex;

        if (_data.DeckPresets.TryGetValue(activeDeckIndex, out DeckData deckData))
        {
            foreach (BaseUnitData data in deckData.BaseUnitDatas)
            {
                if (data != null)
                {
                    result.Add(data);
                }
            }
        }

        return result;
    }

    // 덱 유닛들의 소속? 팩션 구성 분석해서 딕셔너리로 반환
    private Dictionary<UnitSynergyType, int> GetDeckComposition(List<BaseUnitData> units)
    {
        Dictionary<UnitSynergyType, int> nationCounts = new Dictionary<UnitSynergyType, int>();

        foreach (BaseUnitData unit in units)    // 유닛 소속 받아서 중복이면 ++, 중복 아니면 새로 할당하고 value 1로.
        {
            UnitSynergyType nation = GetUnitNation(unit);
            if (nation == UnitSynergyType.None)
                continue;

            if (!nationCounts.ContainsKey(nation))
                nationCounts[nation] = 1;
            else
                nationCounts[nation]++;
        }

        return nationCounts;
    }

    // 유닛 소속 반환하는 메서드
    // -> 시너지와 소속이 따로 분리되어 있지 않아서 일단 이렇게 구성함.
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

    // 조건 수식 체크해서 조건 만족하는지 참, 거짓 산출하는 헬퍼 메서드
    // 없어도 되는데 없으면 하드코딩
    private bool CheckOperator(float value, ValueConditionOperater op, float compose)
    {
        switch (op)
        {
            case ValueConditionOperater.Equals:
                return Mathf.Approximately(value, compose);

            case ValueConditionOperater.NotEquals:
                return !Mathf.Approximately(value, compose);

            case ValueConditionOperater.Greater:
                return value > compose;

            case ValueConditionOperater.Less:
                return value < compose;

            case ValueConditionOperater.GreaterThanOrEqual:
                return value >= compose;

            case ValueConditionOperater.LessThanOrEqual:
                return value <= compose;

            default:
                return false;
        }
    }

    private bool IsTargetMatch(EffectTarget effectTarget, BaseUnitData unit)
    {
        // TODO: 실제 대상 매칭 로직 구현 필요
        // 예: PlayerUnit, EnemyUnit, 특정 UnitClass 등

        switch (effectTarget)
        {
            case EffectTarget.PlayerUnit:
                return true; // 일단 플레이어 유닛으로 가정
            case EffectTarget.AllUnit:
                return true;
            default:
                return true;
        }
    }
    #endregion
}
