using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnitController : BaseController
{
    EnemyUnit enemyUnit;
    Coroutine findTargetRoutine;
    Coroutine attackRoutine;
    bool isAttacking = false;
    protected override void Awake()
    {
        enemyUnit = GetComponent<EnemyUnit>();
        enemyUnit.OnDead += () =>
        {
            UnitManager.Instance.RemoveUnitFromList(enemyUnit, false);
        };
        base.Awake();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        
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
        gameObject.transform.position += enemyUnit.MoveDir * enemyUnit.MoveSpeed * Time.fixedDeltaTime;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        /*enemyUnit.OnDead -= () =>
        {
            UnitManager.Instance.RemoveUnitFromList(enemyUnit, false);
        };*/
        if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
        if (attackRoutine != null) StopCoroutine(attackRoutine);
    }
    public override void Attack()
    {
        base.Attack();
        enemyUnit.TargetUnit.TakeDamage(enemyUnit.AtkPower);
        Debug.Log($"적 유닛 {gameObject.name}: 공격!");
    }
    IEnumerator TargetingRoutine()
    {
        // 0.2초마다 타겟 갱신
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            Debug.Log("타겟 갱신");
            enemyUnit.TargetUnit = UnitManager.Instance.FindClosestTarget(enemyUnit, false);
            enemyUnit.MoveDir = enemyUnit.TargetUnit != null ? Vector3.zero : Vector3.left;
            if(animator) animator.SetFloat("Speed", Mathf.Abs((float)enemyUnit.MoveDir.x));
            yield return wait;
        }
    }
    IEnumerator AttackRoutine()
    {
        // 공격 간격 계산
        WaitForSeconds wait = new WaitForSeconds(10f / enemyUnit.AttackRate);
        while (true)
        {
            if(enemyUnit.TargetUnit != null)
            {
                // 혹시라도 공격 재생이 안 끝났는데 공격을 시작해야 한다면 일단은 공격 안되게 하기
                if(isAttacking) yield return null;
                // 적 인식했다면 공격 시작
                if (animator) animator.SetTrigger("Attack");
                // 적 인식 루틴 정지(움직임 중지)
                if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
                // 어택 애니메이션 루틴 시작
                isAttacking = true;
                StartCoroutine(AtkAnimRoutine());
                yield return wait;
            }
            else yield return null;

        }
    }
    IEnumerator AtkAnimRoutine()
    {
        // Attack 상태 진입 대기
        float normalizedTime = 1f;
        do
        {
            normalizedTime = GetNormalizedTime(attackStateHash);
            yield return null;
        } while (normalizedTime < 0f);

        animator.speed = 0.13f;

        while (normalizedTime < 0.4f)
        {
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
        // 공격 재생이 끝났다면 다시 적 찾기
        findTargetRoutine = StartCoroutine(TargetingRoutine());
        isAttacking = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan; // 색상 지정
        Vector3 pos = transform.position;
        pos.x -= enemyUnit.AttackRange / 2;
        pos.y += 0.75f;
        Gizmos.DrawWireCube(pos, new Vector3(enemyUnit.AttackRange, 2f));
    }
}
