using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitController : BaseUnitController
{
    PlayerUnit playerUnit;
    Coroutine findTargetRoutine;
    Coroutine attackRoutine;
    Coroutine atkAnimRoutine;
    bool isAttacking = false;

    protected override void Awake()
    {
        playerUnit = GetComponent<PlayerUnit>();
        
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
        gameObject.transform.position += playerUnit.MoveDir * playerUnit.MoveSpeed * Time.fixedDeltaTime;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        
        if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
        if (attackRoutine != null) StopCoroutine(attackRoutine);
    }
    public override void Attack()
    {
        base.Attack();
        playerUnit.TargetUnit.TakeDamage(playerUnit.AtkPower);
        //Debug.Log("아군 유닛: 공격!");
    }
    public override void Dead()
    {
        base.Dead();
        if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
        if (attackRoutine != null) StopCoroutine(attackRoutine);
    }
    protected override void HitBackActive(bool active)
    {
        if (active) // 히트백 활성화되면
        {
            // 실행 중인 모든 코루틴 중지
            if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
            if (attackRoutine != null) StopCoroutine(attackRoutine);
            if (atkAnimRoutine != null) StopCoroutine(atkAnimRoutine);
            playerUnit.TargetUnit = null;
            playerUnit.MoveDir = Vector3.zero;
            animator.speed = 1f;
            isAttacking = false;
        }
        else
        {
            // 기존처럼 찾기 실행
            findTargetRoutine = StartCoroutine(TargetingRoutine());
            attackRoutine = StartCoroutine(AttackRoutine());
        }
    }
    IEnumerator TargetingRoutine()
    {
        // 0.2초마다 타겟 갱신
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            playerUnit.TargetUnit = UnitManager.Instance.FindClosestTarget(playerUnit, true);
            playerUnit.MoveDir = playerUnit.TargetUnit != null ? Vector3.zero : Vector3.right;
            if (animator) animator.SetFloat("Speed", Mathf.Abs((float)playerUnit.MoveDir.x));
            yield return wait;
        }
    }
    IEnumerator AttackRoutine()
    {
        // 0.2초마다 타겟 갱신
        WaitForSeconds wait = new WaitForSeconds(10f / playerUnit.AttackRate);
        while (true)
        {
            if (playerUnit.TargetUnit != null)
            {
                if (isAttacking) { yield return null; continue; }
                // 적 인식했다면 공격 시작
                if (animator) animator.SetTrigger("Attack");
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
    IEnumerator AtkAnimRoutine()
    {
        // Attack 상태 진입 대기
        float normalizedTime = -1f;
        do
        {
            normalizedTime = GetNormalizedTime(attackStateHash);
            yield return null;
        } while (normalizedTime < 0f);
        // 현재 기준 예시:
        // 공격 애니메이션 총 길이 0.25초
        // 0.36지점까지 = 0.09초에 해당
        // 0.09초를 딜레이 초로 늘리려면
        animator.speed = playerUnit.StartAttackTime / playerUnit.AttackDelayTime;

        while (normalizedTime < playerUnit.StartAttackNormalizedTime)
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
    
}
