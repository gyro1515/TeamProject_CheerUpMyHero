using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : BaseController
{
    Player player;
    
    private Transform playerTransform;

    private PlayerHQ playerHQ;
    private EnemyHQ enemyHQ;

    private float maxX;         // 플레이어가 갈 수 있는 최대 X. 적 HQ 바로 앞임
    private float minX;         // 플레이어가 갈 수 있는 최소 X. 아군 HQ 바로 앞임.

    [Header("플레이어 스프라이트 들")]
    [SerializeField] Transform spriteTransform;

    protected override void OnEnable()
    {
        base.OnEnable();
        player.OnMoveDirChanged += PlayerMoveAnimation;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        player.OnMoveDirChanged -= PlayerMoveAnimation;
    }


    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<Player>();
        playerTransform = GetComponent<Transform>();
    }

    protected override void Start()
    {
        base.Start();

        playerHQ = GameManager.Instance.PlayerHQ;
        enemyHQ = GameManager.Instance.enemyHQ;

        if (playerHQ == null || enemyHQ == null)
        {
            Debug.Log("HQ null임");
        }

        SpriteRenderer spritePlayerHQ = playerHQ.GetComponentInChildren<SpriteRenderer>();
        SpriteRenderer spriteEnemyHQ = enemyHQ.GetComponentInChildren<SpriteRenderer>();

        minX = spritePlayerHQ.bounds.max.x;
        maxX = spriteEnemyHQ.bounds.min.x;
    }

    protected override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("N");
            if (animator)
                animator.SetTrigger(player.AnimationData.AttackParameterHash);
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!player) return;
        gameObject.transform.position += player.MoveDir * player.MoveSpeed * Time.fixedDeltaTime;

        Vector3 playerPosition = playerTransform.position;
        playerPosition.x = Mathf.Clamp(playerTransform.position.x, minX, maxX);
        playerTransform.position = playerPosition;
    }

    void PlayerMoveAnimation(Vector3 newMoveDir)
    {
        if (animator) 
            animator.SetFloat(player.AnimationData.SpeedParameterHash, Mathf.Abs((float)player.MoveDir.x));
        if (player.MoveDir.x < 0)
            spriteTransform.rotation = Quaternion.Euler(0, 0, 0);
        else
            spriteTransform.rotation = Quaternion.Euler(0, 180, 0);
    }

}
