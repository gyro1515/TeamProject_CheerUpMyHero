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
    private Collider2D[] overLapCollider;
    private List<BaseCharacter> listForSortTarget = new();
    private const int MAX_OVERLAP_COUNT = 100;
    private Vector2 boxSize;
    private const float BOX_YSCALE = 6f; // 6은 일단 아무 숫자 넣음. 적절한 세로 박스 크기는 얼마??

    [Header("공격 대상 레이어(플레이어 레이어)")]
    [SerializeField] LayerMask targetLayerMask;

    //테스트용, 데이터 테이블로 이전 예정
    [SerializeField] float attackBound = 3;
    [SerializeField] AttackTypeTest attackType;
    [SerializeField] int targetLimit = 2;

    protected override void Awake()
    {
        enemyUnit = GetComponent<EnemyUnit>();
        enemyUnit.OnDead += () =>
        {
            UnitManager.Instance.RemoveUnitFromList(enemyUnit, false);
        };
        base.Awake();

        
        if (attackType == AttackTypeTest.ExplosiveRange)
            boxSize = new Vector2(attackBound, BOX_YSCALE);
        else if (attackType == AttackTypeTest.Penetrative || attackType == AttackTypeTest.Multiple)
            boxSize = new Vector2(enemyUnit.AttackRange, BOX_YSCALE);
        overLapCollider = new Collider2D[MAX_OVERLAP_COUNT];
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
            enemyUnit.TargetUnit?.TakeDamage(enemyUnit.AtkPower);
        }
        else if (attackType == AttackTypeTest.ExplosiveRange)
        {
            ExplosiveAttackTarget();
        }
        else if (attackType == AttackTypeTest.Penetrative)
        {
            PenetrativeAttackTarget();
        }
        else if (attackType == AttackTypeTest.Multiple)
        {
            AttackClosestMultipleTarget();
        }
        Debug.Log($"적 유닛 {gameObject.name}: 공격!");

    }

    IEnumerator TargetingRoutine()
    {
        // 0.2초마다 타겟 갱신
        WaitForSeconds wait = new WaitForSeconds(0.1f);

        while (true)
        {
            //enemyUnit.TargetUnit = UnitManager.Instance.FindClosestTarget(enemyUnit, false, out targetPos);
            enemyUnit.MoveDir = enemyUnit.TargetUnit != null ? Vector3.zero : Vector3.left;
            yield return wait;
        }
    }
    IEnumerator AttackRoutine()
    {
        // 0.2초마다 타겟 갱신
        WaitForSeconds wait = new WaitForSeconds(enemyUnit.AttackRate);
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

    #region AttackTypeMethods
    //위치(x좌표) 중심 폭발형 범위공격
    void ExplosiveAttackTarget()
    {
        int hitCount = Physics2D.OverlapBoxNonAlloc(targetPos, boxSize, 0f, overLapCollider, targetLayerMask);

        for (int i = 0; i < hitCount; i++)
        {
            if (overLapCollider[i].TryGetComponent<BaseCharacter>(out BaseCharacter unit))
                unit.Damageable.TakeDamage(enemyUnit.AtkPower);
        }

    }

    //new 관통형 범위공격. 로직은 폭발과 동일, 대신 범위만 사거리 전체
    public void PenetrativeAttackTarget()
    {
        Vector2 rangeCenter = new Vector2(enemyUnit.transform.position.x - (enemyUnit.AttackRange / 2), enemyUnit.transform.position.y);

        int hitCount = Physics2D.OverlapBoxNonAlloc(rangeCenter, boxSize, 0f, overLapCollider, targetLayerMask);

        for (int i = 0; i < hitCount; i++)
        {
            if (overLapCollider[i].TryGetComponent<BaseCharacter>(out BaseCharacter unit))
                unit.Damageable.TakeDamage(enemyUnit.AtkPower);
        }
    }

    //타겟수 제한 있는 범위공격 (가까운 순 정렬)
    public void AttackClosestMultipleTarget()
    {
        listForSortTarget.Clear();
        
        Vector2 rangeCenter = new Vector2(enemyUnit.transform.position.x - (enemyUnit.AttackRange / 2), enemyUnit.transform.position.y);

        int hitCount = Physics2D.OverlapBoxNonAlloc(rangeCenter, boxSize, 0f, overLapCollider, targetLayerMask);

        for (int i = 0; i < hitCount; i++)
        {
            if (overLapCollider[i].TryGetComponent<BaseCharacter>(out BaseCharacter unit))
                listForSortTarget.Add(unit);
        }

        //리스트 정렬
        listForSortTarget.Sort((a, b) =>
        {
            float distA = Mathf.Abs(a.transform.position.x - enemyUnit.transform.position.x);
            float distB = Mathf.Abs(b.transform.position.x - enemyUnit.transform.position.x);

            // distA가 더 작으면(가까우면) 앞으로 오도록 정렬
            return distA.CompareTo(distB);
        });

        int attackCount = Mathf.Min(targetLimit, listForSortTarget.Count);
        Debug.Log($"현재 타겟: {attackCount}명");
        for (int i = 0; i < attackCount; i++)
        {
            // 정렬된 리스트의 앞에서부터 순서대로 공격
            listForSortTarget[i].Damageable.TakeDamage(enemyUnit.AtkPower);
        }
    }


    #endregion

    #region DrawRangeBox
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        // 기즈모 색상 설정
        Gizmos.color = Color.red;

        if (attackType == AttackTypeTest.ExplosiveRange)
            Gizmos.DrawWireCube(targetPos, boxSize);
        else if (attackType == AttackTypeTest.Penetrative || attackType == AttackTypeTest.Multiple)
        {
            Gizmos.DrawWireCube(new Vector2(transform.position.x - (enemyUnit.AttackRange / 2), transform.position.y), boxSize);
        }
    }
    #endregion

}
