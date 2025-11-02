using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealerUnitController : BaseUnitController
{
    private PlayerUnit playerUnit;

    // 코루틴 관리를 위한 변수들
    private Coroutine findTargetRoutine;
    private Coroutine attackRoutine;
    private Coroutine atkAnimRoutine;
    private Coroutine healAnimRoutine;
    private bool isAttacking = false;
    Transform targetPos = null;
    IDamageable targetForAttack;
    BaseCharacter HealTarget;
    float healCognizanceRange = 2f;
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

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (playerUnit.MoveDir != Vector3.zero)
        {
            transform.position += playerUnit.MoveDir * playerUnit.MoveSpeed * Time.fixedDeltaTime;
        }
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        if (atkAnimRoutine != null) StopCoroutine(atkAnimRoutine);
    }
    public override void Dead()
    {
        base.Dead();
        if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        if (atkAnimRoutine != null) StopCoroutine(atkAnimRoutine);
        if (healAnimRoutine != null) StopCoroutine(healAnimRoutine);
    }

    protected override void HitBackActive(bool active)
    {
        if (active)
        {
            if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
            if (attackRoutine != null) StopCoroutine(attackRoutine);
            if (atkAnimRoutine != null) StopCoroutine(atkAnimRoutine);
            if (healAnimRoutine != null) StopCoroutine(healAnimRoutine);
            ResetPlayerUnitController();
        }
        else
        {
            findTargetRoutine = StartCoroutine(TargetingRoutine());
            attackRoutine = StartCoroutine(AttackRoutine());
        }
    }

    // 힐 또는 공격을 수행하는 함수. TargetUnit의 종류를 확인하여 행동을 결정
    public override void Attack()
    {
        base.Attack();

        // 타격 오디오 재생 -> 나중에 효과음 생기면 넣어야 함
        #region 타격 시 효과음 로직
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
        #endregion

        playerUnit.TargetUnit?.TakeDamage(playerUnit.AtkPower);
    }
    #region Coroutines

    private IEnumerator TargetingRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        yield return null;
        while (true)
        {
            playerUnit.TargetUnit = UnitManager.Instance.FindClosestTarget(playerUnit, true, out targetPos);

            // 원거리 유닛의 이동/정지 로직
            playerUnit.MoveDir = playerUnit.TargetUnit != null ? Vector3.zero : Vector3.right;
            if (animator) animator.SetFloat(
                playerUnit.AnimationData.SpeedParameterHash,
                Mathf.Abs((float)playerUnit.MoveDir.x));
            yield return wait;
        }
    }


    // 힐 > 공격 > 이동 우선순위에 따라 '주 타겟'과 '이동 방향'을 결정하는 코루틴
    //private IEnumerator TargetingRoutine()
    //{
    //    WaitForSeconds wait = new WaitForSeconds(0.2f);
    //    while (true)
    //    {
    //        // 1순위: 힐 대상 찾기
    //        BaseCharacter healTarget = FindClosestInjuredAlly();
    //        if (healTarget != null)
    //        {
    //            playerUnit.TargetUnit = healTarget.Damageable; // 힐 대상을 IDamageable로 저장
    //            //targetPos = healTarget.transform.position;
    //            //float distance = Mathf.Abs(transform.position.x - targetPos.x);
    //            //playerUnit.MoveDir = (distance > playerUnit.CognizanceRange) ? (targetPos - (Vector2)transform.position).normalized : Vector3.zero;
    //        }
    //        else
    //        {
    //            // 2순위: 공격 대상 찾기
    //            playerUnit.TargetUnit = UnitManager.Instance.FindClosestTarget(playerUnit, true, out targetPos);
    //            if (playerUnit.TargetUnit != null)
    //            {
    //                float distance = Mathf.Abs(transform.position.x - targetPos.position.x);
    //                playerUnit.MoveDir = (distance > playerUnit.AttackRange) ? Vector3.right : Vector3.zero;
    //            }
    //            else
    //            {
    //                // 3순위: 기본 전진
    //                playerUnit.TargetUnit = null;
    //                playerUnit.MoveDir = Vector3.right;
    //            }
    //        }

    //        animator?.SetFloat(playerUnit.AnimationData.SpeedParameterHash, Mathf.Abs(playerUnit.MoveDir.x));
    //        yield return wait;
    //    }
    //}


    /// 타겟이 사거리 안에 있을 때 공격(또는 힐) 애니메이션을 시작시키는 코루틴
    private IEnumerator AttackRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(playerUnit.AttackRate);
        while (true)
        {
            if (playerUnit.TargetUnit != null)
            {
                if (isAttacking) { yield return null; continue; }
                HealTarget = FindClosestInjuredAlly();

                if (HealTarget != null) 
                {
                    animator.SetTrigger(playerUnit.AnimationData.AttackParameterHash);
                    if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
                    isAttacking = true;
                    healAnimRoutine = StartCoroutine(HealAnimRoutine());
                    yield return wait;
                }
                else
                {
                    animator.SetTrigger(playerUnit.AnimationData.AttackParameterHash);
                    if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
                    isAttacking = true;
                    atkAnimRoutine = StartCoroutine(AtkAnimRoutine());
                    yield return wait;
                }

            }
            else if(targetForAttack != null)
            {
                
            }
                yield return null;
        }
    }
    private IEnumerator HealAnimRoutine()
    {
        float normalizedTime = 0f;
        while (!playerUnit.IsAttackAnimPlaying)
        {
            yield return null;
        }

        animator.speed = playerUnit.StartAttackTime / playerUnit.UnitData.attackDelayTime;

        while (playerUnit.IsAttackAnimPlaying && normalizedTime < playerUnit.StartAttackNormalizedTime)
        {
            // 공격 애니메이션 중에 타겟이 죽으면 즉시 행동 리셋
            if (HealTarget == null || HealTarget.Damageable.IsDead())
            {
                ResetPlayerUnitController();
                findTargetRoutine = StartCoroutine(TargetingRoutine());
                yield break;
            }
            normalizedTime = GetNormalizedTime(attackStateHash);
            yield return null;
        }
        HealTarget.Damageable.TakeHeal(playerUnit.UnitData.healAmount);
        GameObject fxHeal = ObjectPoolManager.Instance.Get(PoolType.FXHealEffect);
        //fxHeal.transform.SetParent(HealTarget.transform);
        fxHeal.transform.position = HealTarget.transform.position + new Vector3(0f, 0.7f, 0f);
        AudioManager.PlayOneShotByCameraDistance(DataManager.AudioData.unitHealSE, HealTarget.transform);

        animator.speed = 1f;
        while (playerUnit.IsAttackAnimPlaying && normalizedTime >= 0f && normalizedTime < 1f)
        {
            normalizedTime = GetNormalizedTime(attackStateHash);
            yield return null;
        }
        findTargetRoutine = StartCoroutine(TargetingRoutine());
        isAttacking = false;
        HealTarget = null;
    }
    // 공격/힐 애니메이션 타이밍을 제어하는 코루틴
    private IEnumerator AtkAnimRoutine()
    {
        /*float normalizedTime = -1f;
        do { normalizedTime = GetNormalizedTime(attackStateHash); yield return null; } while (!playerUnit.IsAttackAnimPlaying || normalizedTime < 0f);*/
        float normalizedTime = 0f;
        while(!playerUnit.IsAttackAnimPlaying)
        {
            yield return null;
        }

        animator.speed = playerUnit.StartAttackTime / playerUnit.UnitData.attackDelayTime;

        while (playerUnit.IsAttackAnimPlaying && normalizedTime < playerUnit.StartAttackNormalizedTime)
        {
            // 공격 애니메이션 중에 타겟이 죽으면 즉시 행동 리셋
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
        playerUnit.TargetUnit = null; // 다른 컨트롤러도 추가 필요@@@@
        animator.speed = 1f;
        while (playerUnit.IsAttackAnimPlaying && normalizedTime >= 0f && normalizedTime < 1f)
        {
            normalizedTime = GetNormalizedTime(attackStateHash);
            yield return null;
        }
        findTargetRoutine = StartCoroutine(TargetingRoutine());
        isAttacking = false;
    }
    #endregion

    /// 인식 범위(힐 범위) 내에서 체력이 80% 미만인 가장 가까운 아군을 찾습니다.
    private BaseCharacter FindClosestInjuredAlly()
    {
        float healthThreshold = 0.8f; // 체력이 80% 이하인 아군만 대상
        List<BaseCharacter> allAllies = UnitManager.PlayerUnitList;

        // --- 1순위: 플레이어 캐릭터 ---
        BaseCharacter player = GameManager.Instance.Player;
        if (player != null && !player.IsDead && (player.CurHp / player.MaxHp) < healthThreshold)
        {
            float distance = (player.transform.position.x - transform.position.x);    
            if (distance >= - healCognizanceRange && distance <= playerUnit.CognizanceRange)
            {
                return player; // 플레이어가 조건에 맞으면 무조건 1순위로 반환
            }
        }

        // --- 2~4순위: 탱커 > 힐러 > 딜러 중에서 가장 가까운 유닛 ---
        BaseCharacter closestTanker = null;
        BaseCharacter closestHealer = null;
        BaseCharacter closestDealer = null;
        float minDistTanker = float.MaxValue;
        float minDistHealer = float.MaxValue;
        float minDistDealer = float.MaxValue;

        foreach (BaseCharacter ally in allAllies)
        {
            // 자기 자신, 죽었거나 체력이 꽉 찬 유닛, 인식 범위를 벗어난 유닛은 제외
            if (ally == this.playerUnit || ally == null || ally.IsDead || (ally.CurHp / ally.MaxHp) > healthThreshold) continue;


            float distance = (ally.transform.position.x - transform.position.x);
            if (distance < -healCognizanceRange || distance > playerUnit.CognizanceRange) continue;

                BaseUnit unit = ally as BaseUnit;
            if (unit != null)
            {
                // 타입에 따라 가장 가까운 유닛을 찾아서 기록
                if (unit.UnitType == UnitType.Tanker && distance < minDistTanker)
                {
                    minDistTanker = distance;
                    closestTanker = ally;
                }
                else if (unit.UnitType == UnitType.Healer && distance < minDistHealer)
                {
                    minDistHealer = distance;
                    closestHealer = ally;
                }
                else if (unit.UnitType == UnitType.Dealer && distance < minDistDealer)
                {
                    minDistDealer = distance;
                    closestDealer = ally;
                }
            }
        }

        // 우선순위 순서대로 대상이 있는지 확인하고 반환
        if (closestTanker != null) return closestTanker;
        if (closestHealer != null) return closestHealer;
        if (closestDealer != null) return closestDealer;

        return null; // 모든 우선순위에서 힐 대상 없음
    }

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
        pos = transform.position;
        pos.x -= healCognizanceRange / 2;
        pos.y += 0.75f;
        Gizmos.DrawWireCube(pos, new Vector3(healCognizanceRange, 2f));

    }
}