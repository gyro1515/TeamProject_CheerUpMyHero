using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum AttackTypeTest
{ 
    Single,
    ExplosiveRange,
    Penetrative,
    Multiple
}

public class TestEnemySplashController : BaseController
{
    EnemyUnit enemyUnit;
    Coroutine findTargetRoutine;
    Coroutine attackRoutine;


    private Vector2 targetPos;
    //테스트용, 데이터로 이전 예정
    [SerializeField] float attackBound = 3;
    [SerializeField] AttackTypeTest attackType;
    [SerializeField] int targetLimit = 2;
    private Vector2 boxSize;

    protected override void Awake()
    {
        enemyUnit = GetComponent<EnemyUnit>();
        enemyUnit.OnDead += () =>
        {
            UnitManager.Instance.RemoveUnitFromList(enemyUnit, false);
        };
        base.Awake();
        if (attackType == AttackTypeTest.ExplosiveRange)
            boxSize = new Vector2(attackBound, 6);
        else if (attackType == AttackTypeTest.Penetrative || attackType == AttackTypeTest.Multiple)
            boxSize = new Vector2(enemyUnit.AttackRange, 6);
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
        if (attackType == AttackTypeTest.Single)
        {
            enemyUnit.TargetUnit.TakeDamage(enemyUnit.AtkPower);
        }
        else if (attackType == AttackTypeTest.ExplosiveRange)
        {
            UnitManager.Instance.ExplosiveAttackTarget(targetPos, false, enemyUnit.AtkPower, attackBound);
        }
        else if (attackType == AttackTypeTest.Penetrative)
        {
            UnitManager.Instance.PenetrativeAttackTarget(enemyUnit, false);
        }
        else if (attackType == AttackTypeTest.Multiple)
        {
            UnitManager.Instance.AttackClosestMultipleTarget(enemyUnit, false, targetLimit);
        }
        Debug.Log($"적 유닛 {gameObject.name}: 공격!");

    }

    IEnumerator TargetingRoutine()
    {
        // 0.2초마다 타겟 갱신
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            enemyUnit.TargetUnit = UnitManager.Instance.FindClosestTarget(enemyUnit, false, out targetPos);
            enemyUnit.MoveDir = enemyUnit.TargetUnit != null ? Vector3.zero : Vector3.left;
            yield return wait;
        }
    }
    IEnumerator AttackRoutine()
    {
        // 0.2초마다 타겟 갱신
        WaitForSeconds wait = new WaitForSeconds(10f / enemyUnit.AttackRate);
        while (true)
        {
            if (enemyUnit.TargetUnit != null)
            {
                Attack();
                yield return wait;
            }
            else yield return null;

        }
    }

    private void OnDrawGizmosSelected()
    {
        // 기즈모 색상 설정
        Gizmos.color = Color.red;

        if (attackType == AttackTypeTest.ExplosiveRange)
            Gizmos.DrawWireCube(targetPos, boxSize);
        else if (attackType == AttackTypeTest.Penetrative || attackType == AttackTypeTest.Multiple)
        {
            Debug.Log("그리시오!");
            Gizmos.DrawWireCube(new Vector2(transform.position.x - (enemyUnit.AttackRange / 2), transform.position.y), boxSize);
        }
    }
}
