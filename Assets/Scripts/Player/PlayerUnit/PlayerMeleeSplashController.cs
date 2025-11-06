using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;

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
    private System.Random random = new System.Random();

    int totalHitCount = 0;
    double totalAverageTime = 0;
    double totalAverageTimeN = 0;

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

        int mainIterations = 100;   // 실제 측정 반복 횟수

        #region 비교 방법 1: 웜업 후, 랜덤 순서로 N log N, N log K 테스트 각각 100회씩 실행 후 평균 시간 비교

        /*int warmUpIterations = 30;    // 워밍업 반복 횟수
        // 웜업, 랜덤 순서로 테스트
        for (int i = 0; i < warmUpIterations; i++)
        {
            if (random.Next(0, 2) == 0)
            {
                TestNLogN();
                TestNLogK();
            }
            else
            {
                TestNLogK();
                TestNLogN();
            }
        }
        //GC 영향 최소화
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        *//*List<double> nLogNTimes = new List<double>(mainIterations);
        List<double> nLogKTimes = new List<double>(mainIterations);*//*

        
        // 시간 측정 시작
        double totalNlogN = 0;
        double totalNlogK = 0;
        //bool isK = false;
        *//*long totalTicksNlogN = 0;
        long totalTicksNlogK = 0;*//*
        for (int i = 0; i < mainIterations; i++)
        {
            if (random.Next(0, 2) == 0)
            {
                sw.Restart();
                TestNLogN();
                sw.Stop();
                totalNlogN += sw.Elapsed.TotalMilliseconds;
                //totalTicksNlogN += sw.ElapsedTicks;
                //nLogNTimes.Add(sw.Elapsed.TotalMilliseconds);

                sw.Restart();
                TestNLogK();
                sw.Stop();
                totalNlogK += sw.Elapsed.TotalMilliseconds;
                //totalTicksNlogK += sw.ElapsedTicks;
                //nLogKTimes.Add(sw.Elapsed.TotalMilliseconds);
            }
            else
            {
                //isK = true;
                sw.Restart();
                TestNLogK();
                sw.Stop();
                totalNlogK += sw.Elapsed.TotalMilliseconds;
                //totalTicksNlogK += sw.ElapsedTicks;
                //nLogKTimes.Add(sw.Elapsed.TotalMilliseconds);
                sw.Restart();
                TestNLogN();
                sw.Stop();
                totalNlogN += sw.Elapsed.TotalMilliseconds;
                //totalTicksNlogN += sw.ElapsedTicks;
                //nLogNTimes.Add(sw.Elapsed.TotalMilliseconds);
            }

        }

        double nlognTime = totalNlogN / mainIterations;
        double nlogkTime = totalNlogK / mainIterations;

        //double tickFreq = (double)Stopwatch.Frequency; // ticks per second
        //double nlognTimeTick = (totalTicksNlogN / tickFreq) * 1000.0 / mainIterations; // ms 단위
        //double nlogkTimeTick = (totalTicksNlogK / tickFreq) * 1000.0 / mainIterations; // ms 단위

        //UnityEngine.Debug.Log($"O(n log k): {nlogkTime} ms, {nlogkTimeTick} ms  vs \"O(n log n): {nlognTime} ms, {nlognTimeTick} ms");
        //string tmp = isK ? "O(n log k) 먼저 작동" : "O(n log n) 먼저 작동";
        //UnityEngine.Debug.Log($"{tmp} = O(n log k): {nlogkTime} ms  vs \"O(n log n): {nlognTime} ms");
        UnityEngine.Debug.Log($"O(n log k): {nlogkTime} ms  vs \"O(n log n): {nlognTime} ms");
        if (nlognTime < nlogkTime)
        {
            UnityEngine.Debug.Log($"O(n log n) was {nlogkTime / nlognTime:F2}x FASTER.");
        }
        else
        {
            UnityEngine.Debug.Log($"O(n log k) was {nlognTime / nlogkTime:F2}x FASTER.");
        }*/
        #endregion

        #region 비교 방법 2: 단순 100번 실행 후 평균 시간 비교
        bool isK = random.Next(0, 2) == 0 ? true : false;
        //bool isK = true;
        double totalTime = 0;
        double totalTimeN = 0;

        /*for (int i = 0; i < mainIterations; i++)
        {
            if (isK)
            {
                sw.Restart();
                TestNLogK();
                sw.Stop();
                totalTime += sw.Elapsed.TotalMilliseconds;
            }
            else
            {
                sw.Restart();
                TestNLogN();
                sw.Stop();
                totalTime += sw.Elapsed.TotalMilliseconds;
            }
        }*/
        if (isK)
        {
            sw.Restart();
            TestNLogK();
            sw.Stop();
            totalTime += sw.Elapsed.TotalMilliseconds;
            sw.Restart();
            TestNLogN();
            sw.Stop();
            totalTimeN += sw.Elapsed.TotalMilliseconds;
        }
        else
        {
            sw.Restart();
            TestNLogN();
            sw.Stop();
            totalTimeN += sw.Elapsed.TotalMilliseconds;
            sw.Restart();
            TestNLogK();
            sw.Stop();
            totalTime += sw.Elapsed.TotalMilliseconds;
        }
        string tmp = isK ? "O(n log k) 작동" : "O(n log n) 작동";
        
        totalHitCount++;
        if (mainIterations < totalHitCount)
        {
            totalAverageTime += totalTime;
            totalAverageTimeN += totalTimeN;
            int tmpHit = totalHitCount - mainIterations;
            //UnityEngine.Debug.Log($"{tmp}, 공격 횟수 {tmpHit} 평균 : {totalAverageTime / tmpHit} ms");
            UnityEngine.Debug.Log($"공격 횟수 {tmpHit},  O(n log k) 평균 : {totalAverageTime / tmpHit} ms vs O(n log n) 평균 : {totalAverageTimeN / tmpHit} ms");
            if (totalAverageTimeN < totalAverageTime)
            {
                UnityEngine.Debug.Log($"O(n log n) was {totalAverageTime / totalAverageTime:F2}x FASTER.");
            }
            else
            {
                UnityEngine.Debug.Log($"O(n log k) was {totalAverageTimeN / totalAverageTime:F2}x FASTER.");
            }
        }

        //double avgTime = totalTime / mainIterations;
        //totalAverageTime += avgTime;
        //UnityEngine.Debug.Log($"{tmp} 평균: {avgTime} ms");
        //UnityEngine.Debug.Log($"전체 평균 시간 after {totalHitCount * mainIterations} tests: {totalAverageTime / totalHitCount} ms");
        #endregion
    }
    void TestNLogN()
    {
        // UnitManager가 관리하는 전체 적 리스트를 가져옴
        List<BaseCharacter> allEnemies = UnitManager.EnemyUnitList;
        List<BaseCharacter> enemiesInRange = new List<BaseCharacter>();

        // 모든 적을 순회하며 거리와 공격 범위를 비교
        foreach (BaseCharacter enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            // 적 사이의 거리를 계산
            float distance = Mathf.Abs(transform.position.x - enemy.transform.position.x);
            if (distance > playerUnit.AttackRange) continue;

            enemiesInRange.Add(enemy);
        }
        // 거리 가까운 적을 선별
        List<BaseCharacter> hitEnemies = enemiesInRange
            .OrderBy(enemy => enemy.transform.position.x)
            .Take(playerUnit.UnitData.maxTargetCount)
            .ToList();
        foreach (BaseCharacter enemy in hitEnemies)
        {
            enemy.Damageable.TakeDamage(playerUnit.AtkPower);
        }
    }
    void TestNLogK()
    {
        List<BaseCharacter> allEnemies = UnitManager.EnemyUnitList;
        // 우선순위 큐 비우기
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

        // 우선순위 큐에 남아있는 유닛들에게 피해 적용
        for (int i = 0; i < selectedUnitPQ.Count; i++)
        {
            selectedUnitPQ.List[i].Element.Damageable.TakeDamage(playerUnit.AtkPower);
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

        //playerUnit.TargetUnit = null; // 다른 컨트롤러도 추가 필요@@@@

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