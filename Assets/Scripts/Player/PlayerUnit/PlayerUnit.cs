using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : BaseUnit
{
    protected override void Awake()
    {
        base.Awake();
        UIManager.Instance.GetUI<UIHpBarContainer>().AddHpBar(this, EUIHpBarType.PlayerUnit);
    }
    protected override void Start()
    {
        base.Start();

    }
}
