using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRangedSplashController : BaseUnitController
{
    private EnemyUnit enemyUnit;

    private Coroutine findTargetRoutine;
    private Coroutine attackRoutine;
    private Coroutine atkAnimRoutine;
    private bool isAttacking = false;
    Transform targetPos = null;

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

    /// 타겟의 위치를 중심으로 범위 피해를 입히는 공격 함수
    public override void Attack()
    {
        base.Attack();

        List<BaseCharacter> allPlayers = UnitManager.Instance.PlayerUnitList;
        List<IDamageable> takeDamages = new List<IDamageable>();
        int hitCount = 0;

        foreach (BaseCharacter player in allPlayers)
        {
            if (player == null || player.IsDead) continue;

            float distance = Mathf.Abs(targetPos.position.x - player.transform.position.x);
            if (distance <= enemyUnit.AttackRange / 2)
            {
                takeDamages.Add(player.Damageable);
                hitCount++;
            }
        }
        foreach (IDamageable player in takeDamages)
        {
            player.TakeDamage(enemyUnit.AtkPower);
        }
        if (hitCount > 0)
        {
            Debug.Log($"{gameObject.name}이(가) {hitCount}명의 아군에게 원거리 범위 공격!");
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
            ResetEnemyUnitController();
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
            enemyUnit.TargetUnit = UnitManager.Instance.FindClosestTarget(enemyUnit, false, out targetPos);

            enemyUnit.MoveDir = enemyUnit.TargetUnit != null ? Vector3.zero : Vector3.left;
            if (animator) animator.SetFloat(
                enemyUnit.AnimationData.SpeedParameterHash,
                Mathf.Abs((float)enemyUnit.MoveDir.x));
            yield return wait;
        }
    }
    private IEnumerator AttackRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(10f / enemyUnit.AttackRate);
        while (true)
        {
            // 타겟이 있고, 사거리 안에 있을 때만 공격 시도
            if (enemyUnit.TargetUnit != null)
            {
                if (isAttacking) { yield return null; continue; }

                // 현재 스트라이프, 애니메이션 없는 캐릭터도 있으므로
                if (animator == null)
                {
                    Attack(); // 바로 공격
                    yield return wait;
                    continue;
                }
                // 적 인식했다면 공격 시작
                animator.SetTrigger(enemyUnit.AnimationData.AttackParameterHash);
                // 적 인식 루틴 정지(움직임 중지)
                if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
                // 어택 애니메이션 루틴 시작
                isAttacking = true;
                atkAnimRoutine = StartCoroutine(AtkAnimRoutine());
                yield return wait;
            }
            else yield return null;
        }
    }

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
        while (normalizedTime >= 0f && normalizedTime < 1f)
        {
            normalizedTime = GetNormalizedTime(attackStateHash);
            yield return null;
        }

        findTargetRoutine = StartCoroutine(TargetingRoutine());
        isAttacking = false;
    }

    private void ResetEnemyUnitController()
    {
        enemyUnit.TargetUnit = null;
        enemyUnit.MoveDir = Vector3.zero;
        if (animator) animator.speed = 1f;
        isAttacking = false;
    }
    #endregion

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.cyan;
        Vector3 pos = transform.position;
        pos.x -= enemyUnit.CognizanceRange / 2; // 적은 왼쪽으로 인식
        pos.y += 0.75f;
        Gizmos.DrawWireCube(pos, new Vector3(enemyUnit.CognizanceRange, 2f));
        if (!isAttacking) return;
        Gizmos.color = Color.red;
        pos = targetPos.position;
        pos.y += 0.75f;
        Gizmos.DrawWireCube(pos, new Vector3(enemyUnit.AttackRange, 2f));
    }
}