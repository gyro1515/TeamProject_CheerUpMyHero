using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class PlayerUnitController : BaseUnitController
{
    PlayerUnit playerUnit;
    Coroutine findTargetRoutine;
    Coroutine attackRoutine;
    Coroutine atkAnimRoutine;
    bool isAttacking = false;

    UnitSynergyType[] _allSynergyTypes = (UnitSynergyType[])Enum.GetValues(typeof(UnitSynergyType));
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
        gameObject.transform.position += playerUnit.MoveDir * playerUnit.MoveSpeed * Time.fixedDeltaTime;
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        if (atkAnimRoutine != null) StopCoroutine(atkAnimRoutine);
    }
    public override void Attack()
    {
        base.Attack();

        //if (playerUnit.CognizanceRange < 2f)
        //{
        //    AudioManager.PlayRandomOneShot(DataManager.AudioData.meleeUnitAttackSE);
        //}
        //else 
        //{
        //    if ((playerUnit.UnitData.synergyType & UnitSynergyType.Archer) != 0)
        //        AudioManager.PlayOneShot(DataManager.AudioData.archerUnitAttackSE);
        //    else if ((playerUnit.UnitData.synergyType & UnitSynergyType.Mage) != 0)
        //        AudioManager.PlayOneShot(DataManager.AudioData.magicUnitAttackSE);
        //    else
        //        AudioManager.PlayOneShot(DataManager.AudioData.archerUnitAttackSE);
        //}

        //switch (playerUnit.UnitData.synergyType)
        //{
        //    case UnitSynergyType.None:
        //        break;
        //    case UnitSynergyType.Kingdom:
        //        break;
        //    case UnitSynergyType.Empire:
        //        break;
        //    case UnitSynergyType.Cleric:
        //        break;
        //    case UnitSynergyType.Berserker:
        //        break;
        //    case UnitSynergyType.Hero:
        //        break;
        //    case UnitSynergyType.Frost:
        //        AudioManager.PlayOneShot(DataManager.AudioData.synergy_iceSE);
        //        break;
        //    case UnitSynergyType.Burn:
        //        AudioManager.PlayOneShot(DataManager.AudioData.synergy_fireSE);
        //        break;
        //    case UnitSynergyType.Poison:
        //        AudioManager.PlayOneShot(DataManager.AudioData.synergy_poisonSE);
        //        break;
        //}

        playerUnit.TargetUnit?.TakeDamage(playerUnit.AtkPower);
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
            ResetPlayerUnitController();
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
            playerUnit.TargetUnit = UnitManager.Instance.FindClosestTarget(playerUnit, true);
            playerUnit.MoveDir = playerUnit.TargetUnit != null ? Vector3.zero : Vector3.right;
            if (animator) animator.SetFloat(
                playerUnit.AnimationData.SpeedParameterHash, 
                Mathf.Abs((float)playerUnit.MoveDir.x));
            yield return wait;
        }
    }
    IEnumerator AttackRoutine()
    {
        // 0.2초마다 타겟 갱신
        WaitForSeconds wait = new WaitForSeconds(playerUnit.AttackRate);
        while (true)
        {
            if (playerUnit.TargetUnit != null)
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
                animator?.SetTrigger(playerUnit.AnimationData.AttackParameterHash);
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
        while (!playerUnit.IsAttackAnimPlaying)
        {
            yield return null;
        }
        // 현재 기준 예시:
        // 공격 애니메이션 총 길이 0.25초
        // 0.36지점까지 = 0.09초에 해당
        // 0.09초를 딜레이 초로 늘리려면
        animator.speed = playerUnit.StartAttackTime / playerUnit.UnitData.attackDelayTime;

        while (playerUnit.IsAttackAnimPlaying && normalizedTime < playerUnit.StartAttackNormalizedTime)
        {
            if (playerUnit.TargetUnit == null || playerUnit.TargetUnit.IsDead()) // 공격 중에 죽었다면 브레이크
            {
                ResetPlayerUnitController();
                findTargetRoutine = StartCoroutine(TargetingRoutine());
                yield break;
            }
            normalizedTime = GetNormalizedTime(attackStateHash);
            yield return null;
        }
        Attack();
        playerUnit.TargetUnit = null; // 다른 컨트롤러도 추가 필요@@@@
        animator.speed = 1f;
        while (playerUnit.IsAttackAnimPlaying && normalizedTime >= 0f && normalizedTime < 1f)
        {
            normalizedTime = GetNormalizedTime(attackStateHash);
            yield return null;
        }
        // 공격 재생이 끝났다면 다시 적 찾기
        findTargetRoutine = StartCoroutine(TargetingRoutine());
        isAttacking = false;
    }
    void ResetPlayerUnitController()
    {
        playerUnit.TargetUnit = null;
        playerUnit.MoveDir = Vector3.zero;
        if(animator) animator.speed = 1f;
        isAttacking = false;
    }
}
