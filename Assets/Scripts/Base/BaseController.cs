using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseController : MonoBehaviour, IAttackable, IDamageable
{
    protected BaseCharacter baseCharacter;
    protected virtual void Awake()
    {
        baseCharacter = GetComponent<BaseCharacter>();
    }
    protected virtual void Start()
    {

    }
    protected virtual void FixedUpdate()
    {

    }
    protected virtual void Update()
    {

    }
    public void Attack()
    {
        
    }

    public void TakeDamage(float damage)
    {
    }
}
