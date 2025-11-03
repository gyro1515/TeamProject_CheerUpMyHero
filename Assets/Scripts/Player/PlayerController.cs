using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : BaseController
{

    public static event Action OnPlayerAction; //행동을 외부에 알리는 이벤트

    Player player;
    Coroutine findTargetRoutine;
    Coroutine attackRoutine;
    Coroutine atkAnimRoutine;
    bool isAttacking = false;

    private Transform playerTransform;

    private PlayerHQ playerHQ;
    private EnemyHQ enemyHQ;

    private float maxX;         // 플레이어가 갈 수 있는 최대 X. 적 HQ 바로 앞임
    private float minX;         // 플레이어가 갈 수 있는 최소 X. 아군 HQ 바로 앞임.

    [Header("플레이어 스프라이트 들")]
    [SerializeField] Transform spriteTransform;

    Coroutine manaRecoveryRoutine;



    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<Player>();
        playerTransform = GetComponent<Transform>();
        spriteTransform.rotation = Quaternion.Euler(0, 180, 0);
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        player.OnMoveDirChanged += PlayerMoveAnimation;
        manaRecoveryRoutine = StartCoroutine(ManaRecoveryRoutine());

        findTargetRoutine = StartCoroutine(TargetingRoutine());
        attackRoutine = StartCoroutine(AttackRoutine());
    }
    protected override void Start()
    {
        base.Start();

        playerHQ = GameManager.Instance.PlayerHQ; // 게임 매니저에서 가져와야 함
        enemyHQ = GameManager.Instance.enemyHQ;

        if (playerHQ == null || enemyHQ == null)
        {
            Debug.Log("HQ null임");
        }

        /*SpriteRenderer spritePlayerHQ = playerHQ.GetComponentInChildren<SpriteRenderer>();
        SpriteRenderer spriteEnemyHQ = enemyHQ.GetComponentInChildren<SpriteRenderer>();*/

        /*minX = spritePlayerHQ.bounds.max.x;
        maxX = spriteEnemyHQ.bounds.min.x;*/
        minX = playerHQ.gameObject.transform.position.x;
        maxX = enemyHQ.gameObject.transform.position.x;
    }

    protected override void Update()
    {
        base.Update();
        //if (Input.GetKeyDown(KeyCode.N))
        //{
        //    Debug.Log("N");
        //    if (animator)
        //        animator.SetTrigger(player.AnimationData.AttackParameterHash);

        //    Attack();//추가한 부분
        //    OnPlayerAction?.Invoke();//추가한 부분
        //}

    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!player) return;
        if (player.MoveDir != Vector3.zero) //추가한 부분
        {
            OnPlayerAction?.Invoke();
        }
        gameObject.transform.position += player.MoveDir * player.MoveSpeed * Time.fixedDeltaTime;
        Vector3 playerPosition = playerTransform.position;
        if(!IsDead()) playerPosition.x = Mathf.Clamp(playerTransform.position.x, minX, maxX);
        playerTransform.position = playerPosition;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        player.OnMoveDirChanged -= PlayerMoveAnimation;
        if (manaRecoveryRoutine != null) StopCoroutine(manaRecoveryRoutine);

    }
    IEnumerator ManaRecoveryRoutine()
    {
        // 초기화 안될 수도 있어서 초기화될 때까지 대기
        while(player.PlayerData.level == 0)
        {
            yield return null;
        }
        WaitForSeconds wait = new WaitForSeconds(player.PlayerData.manaRecoveryTime);
        while (true)
        {
            yield return wait;
            player.CurMana += 1;
        }
    }
    public override void Attack()
    {
        base.Attack();
        player.TargetUnit?.TakeDamage(player.AtkPower);
    }
    void PlayerMoveAnimation(Vector3 newMoveDir)
    {
        if (isAttacking)
        {
            ResetPlayerUnitController();
            if (atkAnimRoutine != null)
                StopCoroutine(atkAnimRoutine);
            findTargetRoutine = StartCoroutine(TargetingRoutine());
        }

        if (animator)
            animator.SetFloat(player.AnimationData.SpeedParameterHash, Mathf.Abs((float)player.MoveDir.x));
        if (player.MoveDir.x < 0)
            spriteTransform.rotation = Quaternion.Euler(0, 0, 0);
        else
            spriteTransform.rotation = Quaternion.Euler(0, 180, 0);
    }


    //일단 PlayerUnitController에서 그대로 가져옴
    IEnumerator TargetingRoutine()
    {
        // 0.2초마다 타겟 갱신
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        yield return null;
        while (true)
        {
            //player.TargetUnit = UnitManager.Instance.FindClosestTarget(player, true);
            player.TargetUnit = UnitManager.Instance.FindClosestTarget(player, true, out Transform targetPos);
            /*if (targetPos != null)
                Debug.Log($"타겟위치: {targetPos.position.x}");*/
            yield return wait;
        }
    }
    IEnumerator AttackRoutine()
    {
        // 0.2초마다 타겟 갱신
        WaitForSeconds wait = new WaitForSeconds(player.AttackRate);
        while (true)
        {
            if (player.TargetUnit != null && player.MoveDir == Vector3.zero)
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
                animator?.SetTrigger(player.AnimationData.AttackParameterHash);
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
        while (!player.IsAttackAnimPlaying)
        {
            yield return null;
        }
        // 현재 기준 예시:
        // 공격 애니메이션 총 길이 0.25초
        // 0.36지점까지 = 0.09초에 해당
        // 0.09초를 딜레이 초로 늘리려면
        animator.speed = player.StartAttackTime / player.UnitData.attackDelayTime;
        float animatorSpeed = animator.speed;

        while (player.IsAttackAnimPlaying && normalizedTime < player.StartAttackNormalizedTime)
        {
            if (!isAttacking)
                yield break;

            if (player.TargetUnit == null || player.TargetUnit.IsDead()) // 공격 중에 죽었다면 브레이크
            {
                ResetPlayerUnitController();
                findTargetRoutine = StartCoroutine(TargetingRoutine());
                yield break;
            }
            normalizedTime = GetNormalizedTime(attackStateHash);
            yield return null;
        }

        Attack();
        animator.speed = 1f;

        while (player.IsAttackAnimPlaying && normalizedTime >= 0f && normalizedTime < 1f)
        {
            if (!isAttacking)
                yield break;
            normalizedTime = GetNormalizedTime(attackStateHash);
            yield return null;
        }
        // 공격 재생이 끝났다면 다시 적 찾기
        findTargetRoutine = StartCoroutine(TargetingRoutine());
        isAttacking = false;

        player.TargetUnit = null;
    }

    void ResetPlayerUnitController()
    {
        player.TargetUnit = null;
        if (animator) animator.speed = 1f;
        isAttacking = false;
        animator.SetTrigger(player.AnimationData.StopAttackParameterHash);
    }
    public void TestForUseActiveArtifact()
    {
        animator.SetTrigger(player.AnimationData.AttackParameterHash);
    }
}
