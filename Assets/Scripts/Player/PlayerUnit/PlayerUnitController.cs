using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitController : BaseController
{
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        gameObject.transform.position += Vector3.right * baseCharacter.MoveSpeed * Time.fixedDeltaTime;
    }
}
