using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : BaseUnit
{
    [field: Header("플레이어 유닛 세팅")]
    [field: SerializeField] public float SpawnCooldown { get; set; } = 5f; 
    protected override void Awake()
    {
        base.Awake();
        OnDead += () =>
        {
            UnitManager.Instance.RemoveUnitFromList(this, true);
        };

    }
    protected override void Start()
    {
        base.Start();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        UnitManager.Instance.AddUnitList(this, true);
        
    }

    protected override float GetStatBonus(StatType type)
    {
        return ArtifactManager.Instance.GetPassiveArtifactStatBonus(EffectTarget.MeleeUnit, type);
        // 이거 일단 임시로 Melee 유닛으로 만들어두긴 했는데 유닛을 어떻게 구분할 지에 대한 것도 생각해봐야 함
    }
}
