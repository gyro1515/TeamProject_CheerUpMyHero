using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseHQSkill : BasePoolable
{
    [SerializeField] protected float detectRange = 20f;
    [SerializeField] protected float attackRange = 10f;
    [SerializeField] protected int maxTargetCount = 10;
    [SerializeField] protected float atkPower = 20f;
    [SerializeField] protected float coolTime = 20f;
    [field: SerializeField] public SpriteRenderer SkillIconRenderer { get; private set; }
    public float CoolTime { get { return coolTime; } }
    public float DetectRange { get { return detectRange; } }

    PriorityQueue<BaseCharacter, float> selectedUnitPQ = new PriorityQueue<BaseCharacter, float>(isMinHeap: false);

    public abstract void ActivateSkill(Vector3 start, Vector3 to);
    
    protected virtual void AttackRange()
    {
        // TODO: 이펙트
        AudioManager.PlayOneShotByCameraDistance(DataManager.AudioData.hqSkillSound, gameObject.transform);

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
