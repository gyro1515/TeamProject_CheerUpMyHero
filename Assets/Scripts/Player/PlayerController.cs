using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : BaseController
{
    Player player;
    
    private Transform playerTransform;

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
        player = GetComponent<Player>();
        player.OnDead += () =>
        {
            UnitManager.Instance.RemoveUnitFromList(player, true);
        };
        base.Awake();

        playerTransform = GetComponent<Transform>();
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
        Vector3 playerPosition = playerTransform.position;
        playerPosition.x = Mathf.Clamp(playerTransform.position.x, -18f, 18f);
        playerTransform.position = playerPosition;
    }


    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!player) return;
        gameObject.transform.position += player.MoveDir * player.MoveSpeed * Time.fixedDeltaTime;
        
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
