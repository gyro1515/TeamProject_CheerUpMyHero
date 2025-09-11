using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitController : BaseController
{
    PlayerUnit playerUnit;
    Coroutine findTargetRoutine;
    Coroutine attackRoutine;
    protected override void Awake()
    {
        playerUnit = GetComponent<PlayerUnit>();
        playerUnit.OnDead += () =>
        {
            UnitManager.Instance.RemoveUnitFromList(playerUnit, true);
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
        gameObject.transform.position += playerUnit.MoveDir * playerUnit.MoveSpeed * Time.fixedDeltaTime;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        /*playerUnit.OnDead -= () =>
        {
            UnitManager.Instance.RemoveUnitFromList(playerUnit, true);
        };*/
        if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
        if (attackRoutine != null) StopCoroutine(attackRoutine);
    }
    public override void Attack()
    {
        base.Attack();
        //playerUnit.TargetUnit.BaseController.TakeDamage(playerUnit.AtkPower);
        playerUnit.TargetUnit.TakeDamage(playerUnit.AtkPower);
        Debug.Log("아군 유닛: 공격!");
    }
    IEnumerator TargetingRoutine()
    {
        // 0.2초마다 타겟 갱신
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            playerUnit.TargetUnit = UnitManager.Instance.FindClosestTarget(playerUnit, true);
            playerUnit.MoveDir = playerUnit.TargetUnit != null ? Vector3.zero : Vector3.right;
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
                Attack();
                yield return wait;
            }
            else yield return null;

        }
    }

}
