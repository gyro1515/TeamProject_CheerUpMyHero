using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class UnitManager : SingletonMono<UnitManager>
{
    //[Header("유닛 확인용")]
    List<BaseCharacter> playerUnitList = new List<BaseCharacter>();
    List<BaseCharacter> enemyUnitList = new List<BaseCharacter>();
    // 테스트로 여기서 아군HQ가져와서 스폰

    LayerMask playerLayerMask;
    LayerMask enemyLayerMask;


    protected override void Awake()
    {
        base.Awake();
        playerLayerMask = LayerMask.GetMask("Player");
        enemyLayerMask = LayerMask.GetMask("Enemy");
    } 

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
        if (index < last) // **추후 수정: if 체크가 비용이 더 클 거 같다면 if빼기
        {
            unitList[index] = unitList[last];
            unitList[index].ListIndex = index; // 교체된 녀석도 인덱스 갱신
        }
        unitList.RemoveAt(last);
    }

    public IDamageable FindClosestTarget(BaseUnit target, bool isPlayer, out Vector2 targetPos)
    {
        targetPos = Vector2.zero; // NaN: 유효하지 않은 숫자

        if (target == null || target.gameObject == null) return null;

        Vector3 callerPos = target.gameObject.transform.position;
        List<BaseCharacter> unitList = isPlayer ? enemyUnitList : playerUnitList;

        IDamageable minDistUnit = null;
        float minDist = float.MaxValue;

        foreach (var unit in unitList)
        {
            if (unit == null || unit == target || unit.IsDead ) continue;

            // 거리 계산
            Vector3 unitPos = unit.gameObject.transform.position;
            //float dist = Mathf.Abs(unitPos.x - callerPos.x);
            float dist = isPlayer ? unitPos.x - callerPos.x : callerPos.x - unitPos.x;
            if (dist < 0f) continue; // 반대 방향 공격 x
            if (dist > target.AttackRange) continue; // 공격 범위 초과하면 다음
            if (dist > minDist) continue; // 최소 거리보다 멀다면 다음
            IDamageable tmp = unit.Damageable;
            if (tmp == null) continue;
            minDist = dist;
            minDistUnit = tmp;
            targetPos = new Vector2(unitPos.x, unitPos.y);
        }
        return minDistUnit;
    }

    //out 없는 버전. 위치 반환 필요없을때 사용
    public IDamageable FindClosestTarget(BaseUnit target, bool isPlayer)
    {
        return FindClosestTarget(target, isPlayer, out _);
    }


    #region Legacy Attack Methods
    ////위치(x좌표) 중심 폭발형 범위공격
    ////나중엔 공격자의 정보를 넣어서 매개변수를 한번에 처리 예정
    ////사실 이 친구는 매니저에 위치할 필요가 없긴 함
    //public void ExplosiveAttackTarget(Vector2 targetPos, bool isPlayer, float atkPower, float bound)
    //{
    //    Vector2 boxSize = new Vector2(bound, 6); //6은 일단 아무 숫자 넣음. 적절한 세로 박스 크기는 얼마??
        
    //    //일단 awake로 레이어마스크 캐싱으로 불러옴
    //    LayerMask targetLayerMask = isPlayer ? enemyLayerMask : playerLayerMask;

    //    Collider2D[] hitColliders = Physics2D.OverlapBoxAll(targetPos, boxSize, 0f, targetLayerMask);

    //    foreach (Collider2D hit in hitColliders)
    //    {
    //        if (hit.TryGetComponent<BaseCharacter>(out BaseCharacter unit))
    //            unit.Damageable.TakeDamage(atkPower);
    //    }

    //}

    ////new 관통형 범위공격. 로직은 폭발과 동일, 대신 범위만 사거리 전체
    //public void PenetrativeAttackTarget(BaseUnit target, bool isPlayer)
    //{
    //    float leftOrRight = isPlayer ? target.AttackRange : -target.AttackRange;
    //    Vector2 rangeCenter = new Vector2(target.transform.position.x + (leftOrRight / 2), target.transform.position.y);
        
    //    Vector2 boxSize = new Vector2(target.AttackRange, 6); //6은 일단 아무 숫자 넣음. 적절한 세로 박스 크기는 얼마??

    //    LayerMask targetLayerMask = isPlayer ? enemyLayerMask : playerLayerMask;

    //    Collider2D[] hitColliders = Physics2D.OverlapBoxAll(rangeCenter, boxSize, 0f, targetLayerMask);

    //    foreach (Collider2D hit in hitColliders)
    //    {
    //        if (hit.TryGetComponent<BaseCharacter>(out BaseCharacter unit))
    //            unit.Damageable.TakeDamage(target.AtkPower);
    //    }

    //}




    ////타겟수 제한 있는 범위공격 (가까운 순 정렬)
    //public void AttackClosestMultipleTarget(BaseUnit target, bool isPlayer, int targetLimit)
    //{
    //    float leftOrRight = isPlayer ? target.AttackRange : -target.AttackRange;
    //    Vector2 rangeCenter = new Vector2(target.transform.position.x + (leftOrRight / 2), target.transform.position.y);

    //    Vector2 boxSize = new Vector2(target.AttackRange, 6); //6은 일단 아무 숫자 넣음. 적절한 세로 박스 크기는 얼마??

    //    LayerMask targetLayerMask = isPlayer ? enemyLayerMask : playerLayerMask;

    //    //실행 위치를 컨트롤러로 옮기고 배열을 만들어주면, nonAlloc도 가능할듯?
    //    Collider2D[] hitColliders = Physics2D.OverlapBoxAll(rangeCenter, boxSize, 0f, targetLayerMask);
    //    //얘도 리스트를 미리 만들기 가능
    //    List<BaseCharacter> vaildList = new();

    //    foreach (Collider2D hit in hitColliders)
    //    {
    //        if (hit.TryGetComponent<BaseCharacter>(out BaseCharacter unit))
    //            vaildList.Add(unit);
    //    }

    //    //리스트 정렬
    //    vaildList.Sort((a, b) =>
    //    {
    //        float distA = Mathf.Abs(a.transform.position.x - target.transform.position.x);
    //        float distB = Mathf.Abs(b.transform.position.x - target.transform.position.x);

    //        // distA가 더 작으면(가까우면) 앞으로 오도록 정렬
    //        return distA.CompareTo(distB);
    //    });

    //    int attackCount = Mathf.Min(targetLimit, vaildList.Count);
    //    for (int i = 0; i < attackCount; i++)
    //    {
    //        // 정렬된 리스트의 앞에서부터 순서대로 공격
    //        vaildList[i].Damageable.TakeDamage(target.AtkPower);
    //    }
    //}

    #endregion

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
