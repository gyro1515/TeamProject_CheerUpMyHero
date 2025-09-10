using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public abstract class BaseHQ : BaseCharacter, IDamageable
{
    [Header("본부 세팅")]
    [SerializeField] protected float minY = 0;
    [SerializeField] protected float maxY = 0;
    [SerializeField] protected float spawnInterval = 0.5f;
    protected int tmpMinY;
    protected int tmpMaxY;

    

    protected override void Awake()
    {
        base.Awake();
        tmpMinY = (int)(minY * 100f);
        tmpMaxY = (int)(maxY * 100f) + 1;
        InvokeRepeating("SpawnUnit", 0f, spawnInterval);
        OnDead += Dead;
    }
    protected abstract void SpawnUnit();
    public void Dead()
    {
        OnDead -= Dead;
        Debug.Log("HQDead");
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
    public void TakeDamage(float damage)
    {
        CurHp -= damage;
    }
    
}
