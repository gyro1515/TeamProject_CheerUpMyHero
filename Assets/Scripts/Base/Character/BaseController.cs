using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BaseController : MonoBehaviour, IAttackable, IDamageable
{
    [Header("베이스 컨트롤러 세팅")]
    [SerializeField] protected Animator animator;
    protected BaseCharacter baseCharacter;
    BasePoolable poolable;
    protected readonly int attackStateHash = Animator.StringToHash("Attack");

    protected virtual void Awake()
    {
        poolable = GetComponent<BasePoolable>();
        baseCharacter = GetComponent<BaseCharacter>();
    }
    protected virtual void OnEnable()
    {
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
        if (baseCharacter.IsDead) return;
        // 어떤 공식에 의해서 피해량이 결정이 되고
        baseCharacter.CurHp -= damage;
    }
    public virtual void Dead()
    {
        // 죽으면 여기서 오브젝트 풀 반환
        baseCharacter.IsDead = true;
        // 이 오브젝트에 BasePoolable스크립트가 붙어 있다면 오브젝트 풀링, 아니면 그냥 삭제
        if (poolable)
        {
            poolable?.ReleaseSelf();
            return;
        }
        Debug.Log($"{gameObject} 삭제됨");
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
    protected float GetNormalizedTime(int stateHash)
    {
        AnimatorStateInfo currentInfo = animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo nextInfo = animator.GetNextAnimatorStateInfo(0);

        if (animator.IsInTransition(0) && nextInfo.tagHash == stateHash)
        {
            return nextInfo.normalizedTime;
        }
        else if (!animator.IsInTransition(0) && currentInfo.tagHash == stateHash)
        {
            return currentInfo.normalizedTime;
        }
        else return -1f;
    }
}
