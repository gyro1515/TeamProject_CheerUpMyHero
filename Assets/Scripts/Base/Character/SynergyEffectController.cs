using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum ConditionForSynergy
{
    None            = 0,
    Poison          = 1 << 0,
    Burn            = 1 << 1,
    Frost           = 1 << 2,
}
public class SynergyEffectController : MonoBehaviour
{
    ConditionForSynergy condition = ConditionForSynergy.None;
    ConditionTick[] conditionTicks; // 수용 가능한 상태이상 틱 배열
    int poisonStack = 0; // 독 중첩 수
    BaseUnit baseUnit;
    BaseUnitController baseUnitController;

    private void Awake()
    {
        baseUnit = GetComponent<BaseUnit>();
        baseUnitController = GetComponent<BaseUnitController>();
        // 상태이상 틱 배열 초기화
        conditionTicks = new ConditionTick[ConditionIdx.COUNT];
        // 인덱스 0 : 독
        conditionTicks[ConditionIdx.POISON] = new ConditionTick(1f); // 1초마다 독 데미지
        // 인덱스 1 : 화상
        conditionTicks[ConditionIdx.BURN] = new ConditionTick(1f);   // 1초마다 화상 데미지
    }

    private void Update()
    {

        ApplyConditionEffect();
    }
    #region 상태이상
    void ApplyConditionEffect()
    {
        if (condition == ConditionForSynergy.None) return;

        if (CheckCondition(ConditionForSynergy.Poison) && conditionTicks[ConditionIdx.POISON].UpdateTick(Time.deltaTime))
        {
            // 독 효과 적용
        }
        if (CheckCondition(ConditionForSynergy.Burn) && conditionTicks[ConditionIdx.BURN].UpdateTick(Time.deltaTime))
        {
            // 화상 효과 적용
        }
    }
    bool CheckCondition(ConditionForSynergy conditionType)
    {
        if((condition & conditionType) == conditionType) return true;

        return false;
    }
    #endregion

    #region 상태이상 틱
    private class ConditionTick
    {
        public float tickInterval;
        public float tickTimer;
        public ConditionTick(float interval)
        {
            tickInterval = interval;
            tickTimer = interval; // 첫 틱 바로 적용 위해 초기값을 인터벌로
        }
        public bool UpdateTick(float deltaTime)
        {
            tickTimer += deltaTime;
            if (tickTimer < tickInterval) return false;

            tickTimer -= tickInterval;
            return true;
        }
        public void ResetTick()
        {
            tickTimer = tickInterval;
        }
    }
    // 상태이상 인덱스 관리용 정적 클래스 (C++ #define 느낌)
    private static class ConditionIdx
    {
        public const int POISON = 0;
        public const int BURN = 1;

        public const int COUNT = 2; // 리스트/배열 초기화용 크기
    }
    #endregion
}
