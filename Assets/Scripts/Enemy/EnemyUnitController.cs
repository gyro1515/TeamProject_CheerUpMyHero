using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnitController : BaseController
{
    EnemyUnit enemyUnit;
    protected override void Awake()
    {
        enemyUnit = GetComponent<EnemyUnit>();
        enemyUnit.OnDead += () =>
        {
            UnitManager.Instance.RemoveUnitFromList(enemyUnit, false);
        };
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        StartCoroutine(TargetingRoutine());
        StartCoroutine(AttackRoutine());
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        gameObject.transform.position += enemyUnit.MoveDir * enemyUnit.MoveSpeed * Time.fixedDeltaTime;
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
