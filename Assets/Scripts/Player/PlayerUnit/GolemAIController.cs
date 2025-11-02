using System.Collections;
using UnityEngine;

public class GolemAIController : BaseUnitController
{
   protected BaseUnit summonUnit;
    Coroutine findTargetRoutine;
    Coroutine attackRoutine;
    Coroutine atkAnimRoutine;
    bool isAttacking = false;
    protected override void Awake()
    {
        base.Awake();

        summonUnit = GetComponent<BaseUnit>();

        if (summonUnit == null)
        {
            Debug.LogError("GolemAIController가 BaseUnit 컴포넌트를 찾지 못했습니다!");
        }
    }
    protected override void OnEnable()
    {
        base.OnEnable();

        ResetSummonUnitController();
        findTargetRoutine = StartCoroutine(TargetingRoutine());
        attackRoutine = StartCoroutine(AttackRoutine());
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        if (atkAnimRoutine != null) StopCoroutine(atkAnimRoutine);
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        gameObject.transform.position += summonUnit.MoveDir * summonUnit.MoveSpeed * Time.fixedDeltaTime;

    }
    public override void Attack()
    {
        base.Attack();

        summonUnit.TargetUnit?.TakeDamage(summonUnit.AtkPower);
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
            ResetSummonUnitController();
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
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        yield return null;
        while (true)
        {
            summonUnit.TargetUnit = UnitManager.Instance.FindClosestTarget(summonUnit, true);
            summonUnit.MoveDir = summonUnit.TargetUnit != null ? Vector3.zero : Vector3.right;
            if (animator) animator.SetFloat(
                summonUnit.AnimationData.SpeedParameterHash,
                Mathf.Abs((float)summonUnit.MoveDir.x));
            yield return wait;
        }
    }
    IEnumerator AttackRoutine()
    {
        // 0.2초마다 타겟 갱신
        WaitForSeconds wait = new WaitForSeconds(summonUnit.AttackRate);
        while (true)
        {
            if (summonUnit.TargetUnit != null)
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
                animator?.SetTrigger(summonUnit.AnimationData.AttackParameterHash);
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
        } while (!summonUnit.IsAttackAnimPlaying && normalizedTime < 0f);
        // 현재 기준 예시:
        // 공격 애니메이션 총 길이 0.25초
        // 0.36지점까지 = 0.09초에 해당
        // 0.09초를 딜레이 초로 늘리려면
        animator.speed = summonUnit.StartAttackTime / summonUnit.UnitData.attackDelayTime;

        while (summonUnit.IsAttackAnimPlaying && normalizedTime < summonUnit.StartAttackNormalizedTime)
        {
            if (summonUnit.TargetUnit == null || summonUnit.TargetUnit.IsDead()) // 공격 중에 죽었다면 브레이크
            {
                ResetSummonUnitController();
                findTargetRoutine = StartCoroutine(TargetingRoutine());
                yield break;
            }
            normalizedTime = GetNormalizedTime(attackStateHash);
            yield return null;
        }
        Attack();
        summonUnit.TargetUnit = null; 
        animator.speed = 1f;
        while (summonUnit.IsAttackAnimPlaying && normalizedTime >= 0f && normalizedTime < 1f)
        {
            normalizedTime = GetNormalizedTime(attackStateHash);
            yield return null;
        }
        // 공격 재생이 끝났다면 다시 적 찾기
        findTargetRoutine = StartCoroutine(TargetingRoutine());
        isAttacking = false;
    }

    void ResetSummonUnitController()
    {
        summonUnit.TargetUnit = null;
        summonUnit.MoveDir = Vector3.zero;
        if (animator) animator.speed = 1f;
        isAttacking = false;
    }
}