using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : BaseUnit
{
    protected override void Awake()
    {
        base.Awake();
        UIManager.Instance.GetUI<UIHpBarContainer>().AddHpBar(this, EUIHpBarType.EnemyUnit);
    }
}
