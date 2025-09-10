using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUnit : BaseCharacter
{
    [field: Header("유닛 세팅")]
    [field: SerializeField] public float AttackRate { get; private set; }
    [field: SerializeField] public float AttackRange { get; private set; }
    [field: SerializeField] public int foodConsumption { get; private set; }
}
