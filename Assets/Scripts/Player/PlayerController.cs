using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : BaseController
{
    Player player;
    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<Player>();
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!player) return;
        gameObject.transform.position += player.MoveDir * player.MoveSpeed * Time.fixedDeltaTime;
    }
}
