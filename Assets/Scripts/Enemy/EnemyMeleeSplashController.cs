using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMeleeSplashController : BaseUnitController
{
    private EnemyUnit enemyUnit;

    private Coroutine findTargetRoutine;
    private Coroutine attackRoutine;
    private Coroutine atkAnimRoutine;
    private bool isAttacking = false;

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

    protected override void Start()
    {
        base.Start();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (enemyUnit.MoveDir != Vector3.zero)
        {
            transform.position += enemyUnit.MoveDir * enemyUnit.MoveSpeed * Time.fixedDeltaTime;
        }
    }

    public override void Attack()
    {
        base.Attack();

        // UnitManager가 관리하는 전체 '아군' 리스트를 가져옴
        List<BaseCharacter> allPlayers = UnitManager.Instance.PlayerUnitList;
        int hitCount = 0;

        // 모든 아군을 순회하며 거리와 공격 범위를 비교
        foreach (BaseCharacter player in allPlayers)
        {
            if (player == null || player.IsDead) continue;

            float distance = Mathf.Abs(transform.position.x - player.transform.position.x);

            if (distance <= enemyUnit.AttackRange)
            {
                // 아군의 컨트롤러를 찾아 데미지를 입힘
                BaseUnitController targetController = player.GetComponent<BaseUnitController>();
                if (targetController != null)
                {
                    targetController.TakeDamage(enemyUnit.AtkPower);
                }
                hitCount++;
            }
        }

        if (hitCount > 0)
        {
            Debug.Log($"{gameObject.name}이(가) {hitCount}명의 아군에게 범위 공격!");
        }
    }

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
            ResetEnemyUnitController();
        }
        else
        {
            findTargetRoutine = StartCoroutine(TargetingRoutine());
            attackRoutine = StartCoroutine(AttackRoutine());
        }
    }

    #region Coroutines
    // 타겟을 찾는 코루틴
    private IEnumerator TargetingRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);
        yield return null;
        while (true)
        {
            enemyUnit.TargetUnit = UnitManager.Instance.FindClosestTarget(enemyUnit, false);

            enemyUnit.MoveDir = enemyUnit.TargetUnit != null ? Vector3.zero : Vector3.left;

            animator?.SetFloat(enemyUnit.AnimationData.SpeedParameterHash, Mathf.Abs(enemyUnit.MoveDir.x));
            yield return wait;
        }
    }

    // 공격을 시작하는 코루틴
    private IEnumerator AttackRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(10f / enemyUnit.AttackRate);
        while (true)
        {
            if (enemyUnit.TargetUnit != null)
            {
                if (isAttacking) { yield return null; continue; }

                animator?.SetTrigger(enemyUnit.AnimationData.AttackParameterHash);
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

    // 공격 애니메이션을 제어하는 코루틴
    private IEnumerator AtkAnimRoutine()
    {
        float normalizedTime = -1f;
        do
        {
            normalizedTime = GetNormalizedTime(attackStateHash);
            yield return null;
        } while (normalizedTime < 0f);

        animator.speed = enemyUnit.StartAttackTime / enemyUnit.AttackDelayTime;

        while (normalizedTime < enemyUnit.StartAttackNormalizedTime)
        {
            if (enemyUnit.TargetUnit.IsDead())
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

        while (normalizedTime >= 0f && normalizedTime < 1f)
        {
            normalizedTime = GetNormalizedTime(attackStateHash);
            yield return null;
        }

        findTargetRoutine = StartCoroutine(TargetingRoutine());
        isAttacking = false;
    }
    #endregion

    private void ResetEnemyUnitController()
    {
        enemyUnit.TargetUnit = null;
        enemyUnit.MoveDir = Vector3.zero;
        if (animator) animator.speed = 1f;
        isAttacking = false;
    }
}