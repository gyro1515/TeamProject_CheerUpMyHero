using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerRangedSplashController : BaseUnitController
{
    private PlayerUnit playerUnit;

    private Coroutine findTargetRoutine;
    private Coroutine attackRoutine;
    private Coroutine atkAnimRoutine;
    private bool isAttacking = false;
    Transform targetPos = null;

    // 범위 범위 판별용 우선순위 큐
    // 플레이어는 최대 힙, 적은 최소 힙 생성해야 함
    // 플레이어는 x 좌표가 작은 적 우선 선택,
    // 적은 x 좌표가 큰 플레이어 우선 선택하기 때문
    PriorityQueue<BaseCharacter, float> selectedUnitPQ = new PriorityQueue<BaseCharacter, float>(isMinHeap: false);
    // 시간 비교용
    //Stopwatch sw = new Stopwatch();

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

    /// 타겟의 위치를 중심으로 범위 피해를 입히는 공격 함수
    public override void Attack()
    {
        base.Attack();
        if (targetPos == null) return;

        // UnitManager가 관리하는 전체 적 리스트를 가져옴
        /*List<BaseCharacter> allEnemies = UnitManager.Instance.EnemyUnitList;
        List<BaseCharacter> enemiesInRange = new List<BaseCharacter>();
        int hitCount = 0;

        // 모든 적을 순회하며 폭발 지점과의 거리를 비교
        foreach (BaseCharacter enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            float distance = Mathf.Abs(targetPos.position.x - enemy.transform.position.x);
            if (distance <= playerUnit.AttackRange / 2)
            {
                enemiesInRange.Add(enemy);
            }
        }
        // 거리 가까운 5명의 적을 선별

        List<BaseCharacter> hitEnemies = enemiesInRange
            .OrderBy(enemy => enemy.transform.position.x)
            .Take(5)
            .ToList();
        foreach (BaseCharacter enemy in hitEnemies)
        {
            enemy.Damageable.TakeDamage(playerUnit.AtkPower);
            hitCount++;
        }*/
        // 251020 우선순위 큐로 로직 변경 -> O(n log n) -> O(n log k)
        // 시간 비교
        //sw.Restart();
        // UnitManager가 관리하는 전체 적 리스트를 가져옴
        List<BaseCharacter> allEnemies = UnitManager.EnemyUnitList;
        int hitCount = 0;
        // 우선 큐 비우기
        selectedUnitPQ.Clear();
        // 모든 적을 순회하며 폭발 지점과의 거리를 비교
        foreach (BaseCharacter enemy in allEnemies)
        {
            // 적이 유효한지 검사
            if (enemy == null || enemy.IsDead) continue;
            float distance = Mathf.Abs(targetPos.position.x - enemy.transform.position.x);
            // 공격 범위 내에 있는 적만 우선순위 큐 체크
            if (distance > playerUnit.AttackRange / 2) continue;

            float priority = enemy.transform.position.x; // x 좌표가 작을수록 우선순위 높음
            // 최대 타겟 수보다 적게 선택된 경우 무조건 추가
            if (selectedUnitPQ.Count < playerUnit.UnitData.maxTargetCount)
            {
                selectedUnitPQ.Enqueue(enemy, priority);
            }
            // 최대 타겟 수에 도달한 경우 우선순위 비교 후 교체
            else if (priority < selectedUnitPQ.Peek().Priority) 
            {
                selectedUnitPQ.Dequeue(); // 가장 오른쪽 유닛 제거
                selectedUnitPQ.Enqueue(enemy, priority); // 새 유닛 추가
            }
        }
        hitCount = selectedUnitPQ.Count;

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
        #endregion

        // 우선순위 큐에 남아있는 유닛들에게 피해 적용
        while (selectedUnitPQ.Count > 0)
        {
            BaseCharacter target = selectedUnitPQ.Dequeue().Element;
            target.Damageable.TakeDamage(playerUnit.AtkPower);
        }
        // 시간 측정 종료
        //sw.Stop();
        //UnityEngine.Debug.Log($"이번 로직 실행시간: {sw.Elapsed.TotalMilliseconds:F6} ms");
        if (hitCount > 0)
        {
            //UnityEngine.Debug.Log($"{gameObject.name}이(가) {hitCount}명의 적에게 원거리 범위 공격!");
        }
    }

    #region Coroutines & State Management
    public override void Dead()
    {
        base.Dead();
        if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        if (atkAnimRoutine != null) StopCoroutine(atkAnimRoutine);
    }

    protected override void HitBackActive(bool active)
    {
        if (active)
        {
            if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
            if (attackRoutine != null) StopCoroutine(attackRoutine);
            if (atkAnimRoutine != null) StopCoroutine(atkAnimRoutine);
            ResetPlayerUnitController();
        }
        else
        {
            findTargetRoutine = StartCoroutine(TargetingRoutine());
            attackRoutine = StartCoroutine(AttackRoutine());
        }
    }

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

    private IEnumerator AttackRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(playerUnit.AttackRate);
        while (true)
        {
            // 타겟이 있고, 사거리 안에 있을 때만 공격 시도
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
                animator.SetTrigger(playerUnit.AnimationData.AttackParameterHash);
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

    private IEnumerator AtkAnimRoutine()
    {
        // Attack 상태에 진입할 때까지 대기
        float normalizedTime = 0f;
        while (!playerUnit.IsAttackAnimPlaying)
        {
            yield return null;
        }

        animator.speed = playerUnit.StartAttackTime / playerUnit.UnitData.attackDelayTime;

        while (playerUnit.IsAttackAnimPlaying && normalizedTime < playerUnit.StartAttackNormalizedTime)
        {
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
        playerUnit.TargetUnit = null;
        animator.speed = 1f;
        while (playerUnit.IsAttackAnimPlaying && playerUnit.IsAttackAnimPlaying && normalizedTime >= 0f && normalizedTime < 1f)
        {
            normalizedTime = GetNormalizedTime(attackStateHash);
            yield return null;
        }

        findTargetRoutine = StartCoroutine(TargetingRoutine());
        isAttacking = false;
    }
    private void ResetPlayerUnitController()
    {
        playerUnit.TargetUnit = null;
        playerUnit.MoveDir = Vector3.zero;
        if (animator) animator.speed = 1f;
        isAttacking = false;
    }
    #endregion
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (gameObject.IsDestroyed()) return;

        Gizmos.color = Color.cyan; // 색상 지정
        Vector3 pos = transform.position;
        pos.x += playerUnit.CognizanceRange / 2;
        pos.y += 0.75f;
        Gizmos.DrawWireCube(pos, new Vector3(playerUnit.CognizanceRange, 2f));
        if (!isAttacking ) return;
        Gizmos.color = Color.red;
        if (targetPos == null) return;
        pos = targetPos.position;
        pos.y += 0.75f;
        Gizmos.DrawWireCube(pos, new Vector3(playerUnit.AttackRange, 2f));

    }
}