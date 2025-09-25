using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRangedSplashController : BaseUnitController
{
    private PlayerUnit playerUnit;

    private Coroutine findTargetRoutine;
    private Coroutine attackRoutine;
    private Coroutine atkAnimRoutine;
    private bool isAttacking = false;
    Vector2 targetPos = Vector2.zero;

    protected override void Awake()
    {
        playerUnit = GetComponent<PlayerUnit>();
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        ResetPlayerUnitController();
        findTargetRoutine = StartCoroutine(TargetingRoutine());
        attackRoutine = StartCoroutine(AttackRoutine());
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (playerUnit.MoveDir != Vector3.zero)
        {
            transform.position += playerUnit.MoveDir * playerUnit.MoveSpeed * Time.fixedDeltaTime;
        }
    }

    /// 타겟의 위치를 중심으로 범위 피해를 입히는 공격 함수
    public override void Attack()
    {
        base.Attack();

        // UnitManager가 관리하는 전체 적 리스트를 가져옴
        List<BaseCharacter> allEnemies = UnitManager.Instance.EnemyUnitList;
        int hitCount = 0;

        // 모든 적을 순회하며 폭발 지점과의 거리를 비교
        foreach (BaseCharacter enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;

            float distance = Mathf.Abs(targetPos.x - enemy.transform.position.x);
            if (distance <= playerUnit.AttackRange) 
            {
                enemy.Damageable.TakeDamage(playerUnit.AtkPower);
                hitCount++;
            }
        }

        if (hitCount > 0)
        {
            Debug.Log($"{gameObject.name}이(가) {hitCount}명의 적에게 원거리 범위 공격!");
        }
    }

    #region Coroutines & State Management
    public override void Dead()
    {
        base.Dead();
        if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        if (atkAnimRoutine != null) StopCoroutine(atkAnimRoutine);
    }

    protected override void HitBackActive(bool active)
    {
        if (active)
        {
            if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
            if (attackRoutine != null) StopCoroutine(attackRoutine);
            if (atkAnimRoutine != null) StopCoroutine(atkAnimRoutine);
            ResetPlayerUnitController();
        }
        else
        {
            findTargetRoutine = StartCoroutine(TargetingRoutine());
            attackRoutine = StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator TargetingRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);
        yield return null;
        while (true)
        {
            playerUnit.TargetUnit = UnitManager.Instance.FindClosestTarget(playerUnit, true, out targetPos);

            // 원거리 유닛의 이동/정지 로직
            if (playerUnit.TargetUnit != null)
            {
                //TargetUnit을 BaseCharacter로 변환하여 사용
                BaseCharacter targetCharacter = playerUnit.TargetUnit as BaseCharacter;
                if (targetCharacter != null)
                {
                    float distance = Mathf.Abs(transform.position.x - targetCharacter.transform.position.x);
                    playerUnit.MoveDir = (distance > playerUnit.AttackRange) ? Vector3.right : Vector3.zero;
                }
            }
            else
            {
                // 타겟이 없으면 전진
                playerUnit.MoveDir = Vector3.right;
            }

            animator?.SetFloat(playerUnit.AnimationData.SpeedParameterHash, Mathf.Abs(playerUnit.MoveDir.x));
            yield return wait;
        }
    }

    private IEnumerator AttackRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(10f / playerUnit.AttackRate);
        while (true)
        {
            // 타겟이 있고, 사거리 안에 있을 때만 공격 시도
            if (playerUnit.TargetUnit != null)
            {
                //TargetUnit을 BaseCharacter로 변환하여 사용
                BaseCharacter targetCharacter = playerUnit.TargetUnit as BaseCharacter;
                if (targetCharacter != null &&
                    Vector3.Distance(transform.position, targetCharacter.transform.position) <= playerUnit.AttackRange)
                {
                    if (isAttacking) { yield return null; continue; }

                    animator?.SetTrigger(playerUnit.AnimationData.AttackParameterHash);
                    if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
                    isAttacking = true;
                    atkAnimRoutine = StartCoroutine(AtkAnimRoutine());
                    yield return wait;
                }
                else
                {
                    yield return null;
                }
            }
        }
    }

    private IEnumerator AtkAnimRoutine()
    {
        // Attack 상태에 진입할 때까지 대기
        float normalizedTime = -1f;
        do
        {
            normalizedTime = GetNormalizedTime(attackStateHash);
            yield return null;
        } while (normalizedTime < 0f);

        animator.speed = playerUnit.StartAttackTime / playerUnit.AttackDelayTime;

        while (normalizedTime < playerUnit.StartAttackNormalizedTime)
        {
            if (playerUnit.TargetUnit.IsDead())
            {
                ResetPlayerUnitController();
                findTargetRoutine = StartCoroutine(TargetingRoutine());
                yield break;
            }
            normalizedTime = GetNormalizedTime(attackStateHash);
            yield return null;
        }

        Attack();

        animator.speed = 1f;
        while (normalizedTime >= 0f && normalizedTime < 1f)
        {
            normalizedTime = GetNormalizedTime(attackStateHash);
            yield return null;
        }

        findTargetRoutine = StartCoroutine(TargetingRoutine());
        isAttacking = false;
    }
    private void ResetPlayerUnitController()
    {
        playerUnit.TargetUnit = null;
        playerUnit.MoveDir = Vector3.zero;
        if (animator) animator.speed = 1f;
        isAttacking = false;
    }
    #endregion
}