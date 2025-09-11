using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseController : MonoBehaviour, IAttackable, IDamageable
{
    protected BaseCharacter baseCharacter;
    protected virtual void Awake()
    {
        baseCharacter = GetComponent<BaseCharacter>();
        baseCharacter.OnDead += Dead;
        
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
    protected virtual void OnDisable()
    {
        baseCharacter.OnDead -= Dead;
    }
    public virtual void Attack()
    {
        
    }

    public virtual void TakeDamage(float damage)
    {
        baseCharacter.CurHp -= damage;
    }
    public virtual void Dead()
    {
        // 죽으면 여기서 오브젝트 풀 반환
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
