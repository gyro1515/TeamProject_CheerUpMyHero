using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyHealerSplashController : BaseUnitController
{
    private EnemyUnit enemyUnit;

    // 코루틴 관리 변수
    private Coroutine findTargetRoutine;
    private Coroutine attackRoutine;
    private Coroutine atkAnimRoutine;
    private Coroutine healAnimRoutine;
    private bool isAttacking = false;
    private Transform targetPos = null;
    private BaseCharacter HealTarget;
    private float healCognizanceRange = 2f;

    // 자세한 설명은 PlayerRangedSplashController.cs 참고
    PriorityQueue<BaseCharacter, float> selectedUnitPQ = new PriorityQueue<BaseCharacter, float>(isMinHeap: true);

    protected override void Awake()
    {
        enemyUnit = GetComponent<EnemyUnit>();
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        ResetEnemyUnitController();
        findTargetRoutine = StartCoroutine(TargetingRoutine());
        attackRoutine = StartCoroutine(AttackRoutine());
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (enemyUnit.MoveDir != Vector3.zero)
        {
            transform.position += enemyUnit.MoveDir * enemyUnit.MoveSpeed * Time.fixedDeltaTime;
        }
    }

    public override void Dead()
    {
        base.Dead();
        if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        if (atkAnimRoutine != null) StopCoroutine(atkAnimRoutine);
        if (healAnimRoutine != null) StopCoroutine(healAnimRoutine);
    }

    protected override void HitBackActive(bool active)
    {
        if (active)
        {
            if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
            if (attackRoutine != null) StopCoroutine(attackRoutine);
            if (atkAnimRoutine != null) StopCoroutine(atkAnimRoutine);
            if (healAnimRoutine != null) StopCoroutine(healAnimRoutine);
            ResetEnemyUnitController();
        }
        else
        {
            findTargetRoutine = StartCoroutine(TargetingRoutine());
            attackRoutine = StartCoroutine(AttackRoutine());
        }
    }

    // 광역 공격을 수행하는 함수
    public override void Attack()
    {
        base.Attack();
        if (targetPos == null) return;

        /*List<BaseCharacter> allPlayers = UnitManager.Instance.PlayerUnitList;
        List<BaseCharacter> playersInRange = new List<BaseCharacter>();
        int hitCount = 0;
        foreach (BaseCharacter player in allPlayers)
        {
            if (player == null || player.IsDead) continue;

            float distance = Mathf.Abs(targetPos.position.x - player.transform.position.x);
            if (distance <= enemyUnit.AttackRange / 2)
            {
                playersInRange.Add(player);
                hitCount++;
            }
        }
        List<BaseCharacter> hitPlayers = playersInRange
            .OrderByDescending(player => player.transform.position.x)
            .Take(5)
            .ToList();
        foreach (BaseCharacter enemy in hitPlayers)
        {
            enemy.Damageable.TakeDamage(enemyUnit.AtkPower);
            hitCount++;
        }*/
        List<BaseCharacter> allPlayers = UnitManager.PlayerUnitList;
        int hitCount = 0;
        selectedUnitPQ.Clear();
        foreach (BaseCharacter player in allPlayers)
        {
            if (player == null || player.IsDead) continue;

            float distance = Mathf.Abs(transform.position.x - player.transform.position.x);
            if (distance > enemyUnit.AttackRange) continue;
            float priority = player.transform.position.x; // x 좌표가 클수록 우선순위 높음
            // 최대 타겟 수보다 적게 선택된 경우 무조건 추가
            if (selectedUnitPQ.Count < enemyUnit.UnitData.maxTargetCount)
            {
                selectedUnitPQ.Enqueue(player, priority);
            }
            // 최대 타겟 수에 도달한 경우 우선순위 비교 후 교체
            else if (priority < selectedUnitPQ.Peek().Priority)
            {
                selectedUnitPQ.Dequeue(); // 가장 오른쪽 유닛 제거
                selectedUnitPQ.Enqueue(player, priority); // 새 유닛 추가
            }
        }
        hitCount = selectedUnitPQ.Count;
        while (selectedUnitPQ.Count > 0)
        {
            BaseCharacter target = selectedUnitPQ.Dequeue().Element;
            target.Damageable.TakeDamage(enemyUnit.AtkPower);
        }
        if (hitCount > 0)
        {
            //Debug.Log($"{gameObject.name}이(가) {hitCount}명의 플레이어 유닛에게 원거리 범위 공격!");
        }
    }

    #region Coroutines

    private IEnumerator TargetingRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        yield return null;
        while (true)
        {
            // 공격 대상: 가장 가까운 '플레이어' 유닛
            enemyUnit.TargetUnit = UnitManager.Instance.FindClosestTarget(enemyUnit, false, out targetPos);

            // 이동 방향: 타겟이 없으면 왼쪽으로 전진
            enemyUnit.MoveDir = enemyUnit.TargetUnit != null ? Vector3.zero : Vector3.left;

            if (animator) animator.SetFloat(
                enemyUnit.AnimationData.SpeedParameterHash,
                Mathf.Abs((float)enemyUnit.MoveDir.x));

            yield return wait;
        }
    }

    private IEnumerator AttackRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(enemyUnit.AttackRate);
        while (true)
        {
            if (enemyUnit.TargetUnit != null)
            {
                if (isAttacking) { yield return null; continue; }

                // 1순위로 힐 대상을 먼저 찾음
                HealTarget = FindClosestInjuredAlly();

                if (HealTarget != null) // 힐 대상이 있으면 힐 실행
                {
                    animator.SetTrigger(enemyUnit.AnimationData.AttackParameterHash);
                    if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
                    isAttacking = true;
                    healAnimRoutine = StartCoroutine(HealAnimRoutine());
                    yield return wait;
                }
                else // 힐 대상이 없으면 공격 실행
                {
                    animator.SetTrigger(enemyUnit.AnimationData.AttackParameterHash);
                    if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
                    isAttacking = true;
                    atkAnimRoutine = StartCoroutine(AtkAnimRoutine());
                    yield return wait;
                }
            }
            yield return null;
        }
    }

    private IEnumerator HealAnimRoutine()
    {
        float normalizedTime = 0f;
        while (!enemyUnit.IsAttackAnimPlaying)
        {
            yield return null;
        }

        animator.speed = enemyUnit.StartAttackTime / enemyUnit.UnitData.attackDelayTime;

        while (enemyUnit.IsAttackAnimPlaying && normalizedTime < enemyUnit.StartAttackNormalizedTime)
        {
            if (HealTarget == null || HealTarget.IsDead)
            {
                ResetEnemyUnitController();
                findTargetRoutine = StartCoroutine(TargetingRoutine());
                yield break;
            }
            normalizedTime = GetNormalizedTime(attackStateHash);
            yield return null;
        }

        HealTarget.Damageable.TakeHeal(enemyUnit.UnitData.healAmount);
        GameObject fxHeal = ObjectPoolManager.Instance.Get(PoolType.FXHealEffect);
        //fxHeal.transform.SetParent(HealTarget.transform);
        fxHeal.transform.position = HealTarget.transform.position + new Vector3(0f, 0.7f, 0f);
        AudioManager.PlayOneShotByCameraDistance(DataManager.AudioData.unitHealSE, HealTarget.transform);
        animator.speed = 1f;

        while (enemyUnit.IsAttackAnimPlaying && normalizedTime >= 0f && normalizedTime < 1f)
        {
            normalizedTime = GetNormalizedTime(attackStateHash);
            yield return null;
        }
        findTargetRoutine = StartCoroutine(TargetingRoutine());
        isAttacking = false;
        HealTarget = null;
    }

    private IEnumerator AtkAnimRoutine()
    {
        float normalizedTime = 0f;
        while (!enemyUnit.IsAttackAnimPlaying)
        {
            yield return null;
        }

        animator.speed = enemyUnit.StartAttackTime / enemyUnit.UnitData.attackDelayTime;

        while (enemyUnit.IsAttackAnimPlaying && normalizedTime < enemyUnit.StartAttackNormalizedTime)
        {
            if (enemyUnit.TargetUnit == null || enemyUnit.TargetUnit.IsDead())
            {
                ResetEnemyUnitController();
                findTargetRoutine = StartCoroutine(TargetingRoutine());
                yield break;
            }
            normalizedTime = GetNormalizedTime(attackStateHash);
            yield return null;
        }
        Attack();
        animator.speed = 1f;
        enemyUnit.TargetUnit = null;

        while (enemyUnit.IsAttackAnimPlaying && normalizedTime >= 0f && normalizedTime < 1f)
        {
            normalizedTime = GetNormalizedTime(attackStateHash);
            yield return null;
        }
        findTargetRoutine = StartCoroutine(TargetingRoutine());
        isAttacking = false;
    }
    #endregion

    private BaseCharacter FindClosestInjuredAlly()
    {
        float healthThreshold = 0.8f;
        List<BaseCharacter> allAllies = UnitManager.EnemyUnitList;

        BaseCharacter closestTanker = null;
        BaseCharacter closestHealer = null;
        BaseCharacter closestDealer = null;
        float minDistTanker = float.MaxValue;
        float minDistHealer = float.MaxValue;
        float minDistDealer = float.MaxValue;

        foreach (BaseCharacter ally in allAllies)
        {
            if (ally == this.enemyUnit || ally == null || ally.IsDead || (ally.CurHp / ally.MaxHp) >= healthThreshold) continue;

            float distance = Mathf.Abs(transform.position.x - ally.transform.position.x);
            if (distance > healCognizanceRange || distance > enemyUnit.CognizanceRange) continue;

            BaseUnit unit = ally as BaseUnit;
            if (unit != null)
            {
                if (unit.UnitType == UnitType.Tanker && distance < minDistTanker)
                {
                    minDistTanker = distance;
                    closestTanker = ally;
                }
                else if (unit.UnitType == UnitType.Healer && distance < minDistHealer)
                {
                    minDistHealer = distance;
                    closestHealer = ally;
                }
                else if (unit.UnitType == UnitType.Dealer && distance < minDistDealer)
                {
                    minDistDealer = distance;
                    closestDealer = ally;
                }
            }
        }

        if (closestTanker != null) return closestTanker;
        if (closestHealer != null) return closestHealer;
        if (closestDealer != null) return closestDealer;

        return null;
    }

    private void ResetEnemyUnitController()
    {
        enemyUnit.TargetUnit = null;
        enemyUnit.MoveDir = Vector3.zero;
        if (animator) animator.speed = 1f;
        isAttacking = false;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // 공격 범위 (왼쪽)
        Gizmos.color = Color.red;
        Vector3 atkPos = transform.position;
        atkPos.x -= enemyUnit.CognizanceRange / 2;
        atkPos.y += 0.75f;
        Gizmos.DrawWireCube(atkPos, new Vector3(enemyUnit.CognizanceRange, 2f));

        // 힐 범위 (중앙 기준)
        Gizmos.color = Color.green;
        Vector3 healPos = transform.position;
        healPos.y += 0.75f;
        Gizmos.DrawWireCube(healPos, new Vector3(healCognizanceRange * 2, 2f));
    }
}