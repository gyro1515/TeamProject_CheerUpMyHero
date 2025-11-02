using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class PlayerMeleeSplashController : BaseUnitController
{
    private PlayerUnit playerUnit;

    private Coroutine findTargetRoutine;
    private Coroutine attackRoutine;
    private Coroutine atkAnimRoutine;
    private bool isAttacking = false;

    // 자세한 설명은 PlayerRangedSplashController.cs 참고
    PriorityQueue<BaseCharacter, float> selectedUnitPQ = new PriorityQueue<BaseCharacter, float>(isMinHeap: false);
    // 시간 비교용
    Stopwatch sw = new Stopwatch();
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
    public override void Attack()
    {
        base.Attack();

        // UnitManager가 관리하는 전체 적 리스트를 가져옴
        /*List<BaseCharacter> allEnemies = UnitManager.Instance.EnemyUnitList;
        List<BaseCharacter> enemiesInRange = new List<BaseCharacter>();
        int hitCount = 0;

        // 모든 적을 순회하며 거리와 공격 범위를 비교
        foreach (BaseCharacter enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            // 적 사이의 거리를 계산
            float distance = Mathf.Abs(transform.position.x - enemy.transform.position.x);
            if (distance <= playerUnit.CognizanceRange)
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
        List<BaseCharacter> allEnemies = UnitManager.EnemyUnitList;
        int hitCount = 0;
        // 우선 큐 비우기
        selectedUnitPQ.Clear();
        // 모든 적을 순회하며 폭발 지점과의 거리를 비교
        foreach (BaseCharacter enemy in allEnemies)
        {
            // 적이 유효한지 검사
            if (enemy == null || enemy.IsDead) continue;
            float distance = Mathf.Abs(transform.position.x - enemy.transform.position.x);
            // 공격 범위 내에 있는 적만 우선순위 큐 체크
            if (distance > playerUnit.AttackRange) continue;

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

        // 우선순위 큐에 남아있는 유닛들에게 피해 적용
        while (selectedUnitPQ.Count > 0)
        {
            BaseCharacter target = selectedUnitPQ.Dequeue().Element;
            target.Damageable.TakeDamage(playerUnit.AtkPower);
        }
        if (hitCount > 0)
        {
            //UnityEngine.Debug.Log($"{gameObject.name}이(가) {hitCount}명의 적에게 범위 공격!");
        }
    }
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

    #region Coroutines
    private IEnumerator TargetingRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        yield return null;
        while (true)
        {
            playerUnit.TargetUnit = UnitManager.Instance.FindClosestTarget(playerUnit, true);
            playerUnit.MoveDir = playerUnit.TargetUnit != null ? Vector3.zero : Vector3.right;
            animator.SetFloat(playerUnit.AnimationData.SpeedParameterHash, Mathf.Abs(playerUnit.MoveDir.x));
            yield return wait;
        }
    }

    private IEnumerator AttackRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(playerUnit.AttackRate);
        while (true)
        {
            if (playerUnit.TargetUnit != null)
            {
                if (isAttacking) { yield return null; continue; }

                if (animator == null)
                {
                    Attack();
                    yield return wait;
                    continue;
                }

                animator.SetTrigger(playerUnit.AnimationData.AttackParameterHash);
                if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
                isAttacking = true;
                atkAnimRoutine = StartCoroutine(AtkAnimRoutine());
                yield return wait;
            }
            else
            {
                yield return null;
            }
        }
    }

    // 공격 애니메이션을 제어하는 코루틴
    private IEnumerator AtkAnimRoutine()
    {
        // Attack 상태에 진입할 때까지 대기
        float normalizedTime = 0f;
        while (!playerUnit.IsAttackAnimPlaying)
        {
            yield return null;
        }
        // 선딜 설정
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
        // 오디오 효과음 재생
        //AudioManager.PlayRandomOneShot(DataManager.AudioData.meleeUnitAttackSE);

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
    }
}