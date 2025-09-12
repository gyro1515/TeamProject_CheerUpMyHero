using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : SingletonMono<UnitManager>
{
    //[Header("유닛 확인용")]
    List<BaseCharacter> playerUnitList = new List<BaseCharacter>();
    List<BaseCharacter> enemyUnitList = new List<BaseCharacter>();

    public void AddUnitList(BaseCharacter unit, bool isPlayer)
    {
        List<BaseCharacter> unitList = isPlayer ? playerUnitList : enemyUnitList;

        unit.ListIndex = unitList.Count;
        unitList.Add(unit);
    }
    public void RemoveUnitFromList(BaseCharacter unit, bool isPlayer)
    {
        List<BaseCharacter> unitList = isPlayer ? playerUnitList : enemyUnitList;

        // List삭제가 O(1)이 되도록
        int index = unit.ListIndex;
        int last = unitList.Count - 1;
        //unit.ListIndex = -1; // 리스트에 삭제된 애는 -1로 -> 굳이...?
        // swap-back 방식, 맨 뒤에 것을 삭제할 것에 덮어쓰고, 맨 뒤 삭제
        if (index < last)
        {
            unitList[index] = unitList[last];
            unitList[index].ListIndex = index; // 교체된 녀석도 인덱스 갱신
        }
        unitList.RemoveAt(last);
    }
    public IDamageable FindClosestTarget(BaseUnit caller, bool isPlayer)
    {
        if (caller == null || caller.gameObject == null) return null;

        Vector3 callerPos = caller.gameObject.transform.position;
        List<BaseCharacter> unitList = isPlayer ? enemyUnitList : playerUnitList;

        IDamageable minDistUnit = null;
        float minDist = float.MaxValue;

        foreach (var unit in unitList)
        {
            if (unit == null || unit == caller || !unit.gameObject.activeSelf ) continue;

            // 거리 계산
            float dist = Mathf.Abs(unit.gameObject.transform.position.x - callerPos.x);
            if (dist > caller.AttackRange) continue; // 공격 범위 초과하면 다음
            if (dist > minDist) continue; // 최소 거리보다 멀다면 다음
            IDamageable tmp = unit.Damageable;
            if (tmp == null) continue;
            minDist = dist;
            minDistUnit = tmp;
        }
        return minDistUnit;
    }
    
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
}
