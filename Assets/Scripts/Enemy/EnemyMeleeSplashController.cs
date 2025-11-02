using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class EnemyMeleeSplashController : BaseUnitController
{
    private EnemyUnit enemyUnit;

    private Coroutine findTargetRoutine;
    private Coroutine attackRoutine;
    private Coroutine atkAnimRoutine;
    private bool isAttacking = false;

    // 자세한 설명은 PlayerRangedSplashController.cs 참고
    PriorityQueue<BaseCharacter, float> selectedUnitPQ = new PriorityQueue<BaseCharacter, float>(isMinHeap: true);
    // 시간 비교용
    Stopwatch sw = new Stopwatch();
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

        /*List<BaseCharacter> allPlayers = UnitManager.Instance.PlayerUnitList;
        List<BaseCharacter> playersInRange = new List<BaseCharacter>();
        int hitCount = 0;

        foreach (BaseCharacter player in allPlayers)
        {
            if (player == null || player.IsDead) continue;

            float distance = Mathf.Abs(transform.position.x - player.transform.position.x);
            if (distance <= enemyUnit.CognizanceRange)
            {
                playersInRange.Add(player);
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
            //UnityEngine.Debug.Log($"{gameObject.name}이(가) {hitCount}명의 아군에게 범위 공격!");
        }
    }

    public override void Dead()
    {
        base.Dead();
        if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        if (atkAnimRoutine != null) StopCoroutine(atkAnimRoutine);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        /*
        if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
        if (attackRoutine != null) StopCoroutine(attackRoutine);*/
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
    private IEnumerator TargetingRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        yield return null;
        while (true)
        {
            enemyUnit.TargetUnit = UnitManager.Instance.FindClosestTarget(enemyUnit, false);

            enemyUnit.MoveDir = enemyUnit.TargetUnit != null ? Vector3.zero : Vector3.left;

            animator.SetFloat(enemyUnit.AnimationData.SpeedParameterHash, Mathf.Abs(enemyUnit.MoveDir.x));
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

                if (animator == null)
                {
                    Attack();
                    yield return wait;
                    continue;
                }

                animator.SetTrigger(enemyUnit.AnimationData.AttackParameterHash);
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
        enemyUnit.TargetUnit = null;

        animator.speed = 1f;
        while (enemyUnit.IsAttackAnimPlaying && normalizedTime >= 0f && normalizedTime < 1f)
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

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.cyan;
        Vector3 pos = transform.position;
        pos.x -= enemyUnit.CognizanceRange / 2; // 적은 왼쪽으로 인식
        pos.y += 0.75f;
        Gizmos.DrawWireCube(pos, new Vector3(enemyUnit.CognizanceRange, 2f));
    }
}