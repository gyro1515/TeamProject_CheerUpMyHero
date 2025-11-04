using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseHQSkill : BasePoolable
{
    [SerializeField] protected float detectRange = 20f;
    [SerializeField] protected float attackRange = 10f;
    [SerializeField] protected int maxTargetCount = 10;
    [SerializeField] protected float atkPower = 20f;

    public abstract void ActivateSkill(Vector3 start);

    Vector3 tmpTarget = new Vector3(0f, -100f, 0f);
    PriorityQueue<BaseCharacter, float> selectedUnitPQ = new PriorityQueue<BaseCharacter, float>(isMinHeap: false);

    protected bool FindTarget(Vector3 start, out Vector3 target)
    {
        target = tmpTarget;

        List<BaseCharacter> enemyList = UnitManager.EnemyUnitList;

        float minDist = float.MaxValue;

        foreach (var unit in enemyList)
        {
            if (unit == null || unit.IsDead) continue;

            // 거리 계산
            Vector3 unitPos = unit.gameObject.transform.position;
            //float dist = Mathf.Abs(unitPos.x - callerPos.x);
            float dist =  unitPos.x - start.x;
            if (dist < 0f) continue; // 반대 방향 공격 x
            if (dist > detectRange) continue; // 공격 범위 초과하면 다음
            if (dist > minDist) continue; // 최소 거리보다 멀다면 다음
            minDist = dist;
            target = unit.gameObject.transform.position;
        }
        if(target != tmpTarget)
        {
            return true;
        }

        return false;
    }

    protected virtual void AttackRange()
    {
        // TODO: 이펙트

        // 범위 데미지 처리
        List<BaseCharacter> allEnemies = UnitManager.EnemyUnitList;
        int hitCount = 0;
        // 우선 큐 비우기
        selectedUnitPQ.Clear();
        // 모든 적을 순회하며 폭발 지점과의 거리를 비교
        foreach (BaseCharacter enemy in allEnemies)
        {
            // 적이 유효한지 검사
            if (enemy == null || enemy.IsDead) continue;
            float distance = Mathf.Abs(gameObject.transform.position.x - enemy.transform.position.x);
            // 공격 범위 내에 있는 적만 우선순위 큐 체크
            if (distance > attackRange / 2) continue;

            float priority = enemy.transform.position.x; // x 좌표가 작을수록 우선순위 높음
            // 최대 타겟 수보다 적게 선택된 경우 무조건 추가
            if (selectedUnitPQ.Count < maxTargetCount)
            {
                selectedUnitPQ.Enqueue(enemy, priority);
            }
            // 최대 타겟 수에 도달한 경우 우선순위 비교 후 교체
            else if (priority < selectedUnitPQ.Peek().Priority)
            {
                selectedUnitPQ.Dequeue(); // 가장 오른쪽 유닛 제거
                selectedUnitPQ.Enqueue(enemy, priority); // 새 유닛 추가
            }
        }
        hitCount = selectedUnitPQ.Count;
        while (selectedUnitPQ.Count > 0)
        {
            BaseCharacter target = selectedUnitPQ.Dequeue().Element;
            target.Damageable.TakeDamage(atkPower);
        }
        ReleaseSelf();
    }
}
