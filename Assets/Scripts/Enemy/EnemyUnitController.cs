using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnitController : BaseController
{
    EnemyUnit enemyUnit;
    Coroutine findTargetRoutine;
    Coroutine attackRoutine;
    protected override void Awake()
    {
        enemyUnit = GetComponent<EnemyUnit>();
        
        base.Awake();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        enemyUnit.OnDead += () =>
        {
            UnitManager.Instance.RemoveUnitFromList(enemyUnit, false);
        };
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
        enemyUnit.OnDead -= () =>
        {
            UnitManager.Instance.RemoveUnitFromList(enemyUnit, false);
        };
        if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
        if (attackRoutine != null) StopCoroutine(attackRoutine);
    }
    public override void Attack()
    {
        base.Attack();

        enemyUnit.TargetUnit.TakeDamage(enemyUnit.AtkPower);
        Debug.Log("적 유닛: 공격!");
    }
    IEnumerator TargetingRoutine()
    {
        // 0.2초마다 타겟 갱신
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            enemyUnit.TargetUnit = UnitManager.Instance.FindClosestTarget(enemyUnit, false);
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
            if(enemyUnit.TargetUnit != null)
            {
                Attack();
                yield return wait;
            }
            else yield return null;

        }
    }
}
