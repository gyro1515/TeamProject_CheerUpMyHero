using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/*public abstract class UnitPatten: MonoBehaviour
{
    BaseUnit unit;
    public void Init(BaseUnit _unit)
    {
        unit = _unit;
    }
    public abstract void OnEnable();
    public abstract void Attack();
    public abstract void Dead();
    public abstract void HitBack();
}*/

public class EnemyUnitController : BaseUnitController
{
    EnemyUnit enemyUnit;
    Coroutine findTargetRoutine;
    Coroutine attackRoutine;
    Coroutine atkAnimRoutine;
    bool isAttacking = false;

    //UnitPatten unitPatten;
    protected override void Awake()
    {
        enemyUnit = GetComponent<EnemyUnit>();
        base.Awake();

        // 데이터에 따라 패턴 달라지게 하기
        //unitPatten = 팩토리 패턴으로 유닛 타입에 따른 패턴 생성 후 ;
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        //unitPatten?.OnEnable();


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
        gameObject.transform.position += enemyUnit.MoveDir * enemyUnit.MoveSpeed * Time.fixedDeltaTime;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        // Dead()로 이동
        /*if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
        if (attackRoutine != null) StopCoroutine(attackRoutine);*/
    }
    public override void Attack()
    {
        base.Attack();
        enemyUnit.TargetUnit?.TakeDamage(enemyUnit.AtkPower);
        //Debug.Log($"적 유닛 {gameObject.name}: 공격!");
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
            ResetEnemyUnitController();
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
        // 한 프레임 쉬어가기, 소환 후 -> OnEnable -> TargetingRoutine() 한 프레임에 실행
        // 위치 갱신 하기 전 실행하기 때문에 바로 0,0,0 좌표 값 기준에서 타겟 가져옴
        yield return null; 

        while (true)
        {
            //Debug.Log("타겟 갱신");
            enemyUnit.TargetUnit = UnitManager.Instance.FindClosestTarget(enemyUnit, false);
            enemyUnit.MoveDir = enemyUnit.TargetUnit != null ? Vector3.zero : Vector3.left;
            if(animator) animator.SetFloat(
                enemyUnit.AnimationData.SpeedParameterHash, Mathf.Abs((float)enemyUnit.MoveDir.x));
            yield return wait;
        }
    }
    IEnumerator AttackRoutine()
    {
        // 공격 간격 계산
        WaitForSeconds wait = new WaitForSeconds(enemyUnit.AttackRate);
        while (true)
        {
            if (enemyUnit.TargetUnit != null)
            {
                // 혹시라도 공격 재생이 안 끝났는데 공격을 시작해야 한다면 일단은 공격 안되게 하기
                if (isAttacking) { yield return null; continue; }
                // 현재 스트라이프, 애니메이션 없는 캐릭터도 있으므로
                if(animator == null)
                {
                    Attack(); // 바로 공격
                    yield return wait;
                    continue;
                }

                // 적 인식했다면 공격 시작
                animator?.SetTrigger(enemyUnit.AnimationData.AttackParameterHash);
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
        float normalizedTime = 0f;
        while (!enemyUnit.IsAttackAnimPlaying)
        {
            yield return null;
        }

        // 현재 기준 예시:
        // 공격 애니메이션 총 길이 0.25초
        // 0.36지점까지 = 0.09초에 해당
        // 0.09초를 딜레이 초로 늘리려면
        animator.speed = enemyUnit.StartAttackTime / enemyUnit.UnitData.attackDelayTime;

        while (enemyUnit.IsAttackAnimPlaying && normalizedTime < enemyUnit.StartAttackNormalizedTime)
        {
            if(enemyUnit.TargetUnit == null || enemyUnit.TargetUnit.IsDead()) // 공격 중에 죽었다면 브레이크
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
        // 공격 재생이 끝났다면 다시 적 찾기
        findTargetRoutine = StartCoroutine(TargetingRoutine());
        isAttacking = false;
    }
    void ResetEnemyUnitController()
    {
        enemyUnit.TargetUnit = null;
        enemyUnit.MoveDir = Vector3.zero;
        animator.speed = 1f;
        isAttacking = false;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.cyan; // 색상 지정
        Vector3 pos = transform.position;
        pos.x -= enemyUnit.AttackRange / 2;
        pos.y += 0.75f;
        Gizmos.DrawWireCube(pos, new Vector3(enemyUnit.AttackRange, 2f));
    }
}
