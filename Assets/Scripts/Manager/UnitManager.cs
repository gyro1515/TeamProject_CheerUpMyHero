using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : SingletonMono<UnitManager>
{
    public List<BaseCharacter> PlayerUnitList { get; private set; } = new List<BaseCharacter>();
    public List<BaseCharacter> EnemyUnitList { get; private set; } = new List<BaseCharacter>();

    /*public BaseCharacter FindClosestTarget(BaseUnit caller, bool isPlayer)
    {
        if (caller == null || caller.gameObject == null) return null;

        Vector3 callerPos = caller.gameObject.transform.position;
        List<BaseCharacter> unitList = isPlayer ? EnemyUnitList : PlayerUnitList;

        BaseCharacter minDistUnit = null;
        float minDist = float.MaxValue;

        foreach (var unit in unitList)
        {
            if (unit == null || !unit.gameObject.activeSelf) continue;

            // 거리 계산 (제곱 거리)
            float dist = (unit.gameObject.transform.position - callerPos).sqrMagnitude;
            if (dist > caller.AttackRange) continue; // 공격 범위 초과하면 다음
            if (dist > minDist) continue; // 최소 거리보다 멀다면 다음

            minDist = dist;
            minDistUnit = unit;
        }
        return minDistUnit;
    }*/
    public IDamageable FindClosestTarget(BaseUnit caller, bool isPlayer)
    {
        if (caller == null || caller.gameObject == null) return null;

        Vector3 callerPos = caller.gameObject.transform.position;
        List<BaseCharacter> unitList = isPlayer ? EnemyUnitList : PlayerUnitList;

        IDamageable minDistUnit = null;
        float minDist = float.MaxValue;

        foreach (var unit in unitList)
        {
            if (unit == null || !unit.gameObject.activeSelf) continue;

            // 거리 계산
            float dist = Mathf.Abs(unit.gameObject.transform.position.x - callerPos.x);
            if (dist > caller.AttackRange) continue; // 공격 범위 초과하면 다음
            if (dist > minDist) continue; // 최소 거리보다 멀다면 다음
            IDamageable tmp = unit.gameObject.GetComponent<IDamageable>();
            if (tmp == null) continue;
            minDist = dist;
            minDistUnit = tmp;
        }
        return minDistUnit;
    }
}
