using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerMeleeSplashController : BaseUnitController
{
    private PlayerUnit playerUnit;

    private Coroutine findTargetRoutine;
    private Coroutine attackRoutine;
    private Coroutine atkAnimRoutine;
    private bool isAttacking = false;

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

    protected override void Start()
    {
        base.Start();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (playerUnit.MoveDir != Vector3.zero)
        {
            transform.position += playerUnit.MoveDir * playerUnit.MoveSpeed * Time.fixedDeltaTime;
        }
    }
    public override void Attack()
    {
        base.Attack();

        // UnitManager가 관리하는 전체 적 리스트를 가져옴
        List<BaseCharacter> allEnemies = UnitManager.Instance.EnemyUnitList;
        List<BaseCharacter> enemiesInRange = new List<BaseCharacter>();
        int hitCount = 0;

        // 모든 적을 순회하며 거리와 공격 범위를 비교
        foreach (BaseCharacter enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            // 적 사이의 거리를 계산
            float distance = Mathf.Abs(transform.position.x - enemy.transform.position.x);
            if (distance <= playerUnit.CognizanceRange)
            {
                enemiesInRange.Add(enemy);
            }
        }
        // 거리 가까운 5명의 적을 선별

        List<BaseCharacter> hitEnemies = enemiesInRange
            .OrderBy(enemy => enemy.transform.position.x)
            .Take(5)
            .ToList();
        foreach (BaseCharacter enemy in hitEnemies)
        {
            enemy.Damageable.TakeDamage(playerUnit.AtkPower);
            hitCount++;
        }

        if (hitCount > 0)
        {
            Debug.Log($"{gameObject.name}이(가) {hitCount}명의 적에게 범위 공격!");
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
            ResetPlayerUnitController();
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
        WaitForSeconds wait = new WaitForSeconds(0.2f);
        yield return null;
        while (true)
        {
            playerUnit.TargetUnit = UnitManager.Instance.FindClosestTarget(playerUnit, true);
            playerUnit.MoveDir = playerUnit.TargetUnit != null ? Vector3.zero : Vector3.right;
            animator.SetFloat(playerUnit.AnimationData.SpeedParameterHash, Mathf.Abs(playerUnit.MoveDir.x));
            yield return wait;
        }
    }

    private IEnumerator AttackRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(playerUnit.AttackRate);
        while (true)
        {
            if (playerUnit.TargetUnit != null)
            {
                if (isAttacking) { yield return null; continue; }

                if (animator == null)
                {
                    Attack();
                    yield return wait;
                    continue;
                }

                animator.SetTrigger(playerUnit.AnimationData.AttackParameterHash);
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
        // Attack 상태에 진입할 때까지 대기
        float normalizedTime = -1f;
        do
        {
            normalizedTime = GetNormalizedTime(attackStateHash);
            yield return null;
        } while (normalizedTime < 0f);
        // 선딜 설정
        animator.speed = playerUnit.StartAttackTime / playerUnit.UnitData.attackDelayTime;



        while (normalizedTime < playerUnit.StartAttackNormalizedTime)
        {
            if (playerUnit.TargetUnit == null || playerUnit.TargetUnit.IsDead())
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
        playerUnit.TargetUnit = null; // 다른 컨트롤러도 추가 필요@@@@
        findTargetRoutine = StartCoroutine(TargetingRoutine());
        isAttacking = false;
    }
    #endregion

    private void ResetPlayerUnitController()
    {
        playerUnit.TargetUnit = null;
        playerUnit.MoveDir = Vector3.zero;
        if (animator) animator.speed = 1f;
        isAttacking = false;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.cyan; // 색상 지정
        Vector3 pos = transform.position;
        pos.x += playerUnit.CognizanceRange / 2;
        pos.y += 0.75f;
        Gizmos.DrawWireCube(pos, new Vector3(playerUnit.CognizanceRange, 2f));
    }
}