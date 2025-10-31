using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class BaseController : MonoBehaviour, IAttackable, IDamageable
{
    [Header("베이스 컨트롤러 세팅")]
    [SerializeField] protected Animator animator;
    protected BaseCharacter baseCharacter;
    private SpriteRenderer _spriteRenderer;
    BasePoolable poolable;
    protected readonly int attackStateHash = Animator.StringToHash("Attack");
    public Animator Animator { get { return animator; } }

    protected virtual void Awake()
    {
        poolable = GetComponent<BasePoolable>();
        baseCharacter = GetComponent<BaseCharacter>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>(); // 자식에 있을 경우 InChildren
        if (animator == null)
        {
            //Debug.Log($"Animator가 비어있습니다. {gameObject.name}의 자식 오브젝트에서 탐색합니다.");
            animator = GetComponentInChildren<Animator>();
            if(animator == null) Debug.LogError("Animator탐색 실패. Animator가 Null입니다.");
        }
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

        // 이펙트 소환
        GameObject fxGO;
        // 데미지양GO 소환
        GameObject damageGO;
        if (baseCharacter is Player player || baseCharacter is PlayerUnit playerUnit || baseCharacter is PlayerHQ playerHQ)
        {
            fxGO = ObjectPoolManager.Instance.Get(PoolType.FXPlayerUnitHit);
            fxGO.transform.position = baseCharacter.transform.position + Vector3.up * 0.7f;
            damageGO = ObjectPoolManager.Instance.Get(PoolType.DealAmountEnemy);
            DealAmountEnemy dealAmount = damageGO.GetComponent<DealAmountEnemy>();
            dealAmount.SetAmount(Mathf.CeilToInt(damage));
            damageGO.transform.position = baseCharacter.transform.position + Vector3.up * 1.85f;
        }
        else if (baseCharacter is EnemyUnit enemyUnit || baseCharacter is EnemyHQ enemyHQ)
        {
            fxGO = ObjectPoolManager.Instance.Get(PoolType.FXEnemyUnitHit);
            fxGO.transform.position = baseCharacter.transform.position + Vector3.up * 0.7f;
            damageGO = ObjectPoolManager.Instance.Get(PoolType.DealAmountPlayer);
            DealAmountPlayer dealAmount = damageGO.GetComponent<DealAmountPlayer>();
            dealAmount.SetAmount(Mathf.CeilToInt(damage));
            damageGO.transform.position = baseCharacter.transform.position + Vector3.up * 1.85f;
        }
    }
    public virtual void Dead()
    {
        // 죽으면 여기서 오브젝트 풀 반환
        baseCharacter.IsDead = true;

        // 아래 SetDead()로 이동
        /*// 이 오브젝트에 BasePoolable스크립트가 붙어 있다면 오브젝트 풀링, 아니면 그냥 삭제
        if (poolable)
        {
            poolable?.ReleaseSelf();
            return;
        }
        Debug.Log($"{gameObject} 삭제됨");
        gameObject.SetActive(false);
        Destroy(gameObject);*/
    }
    public void SetDead()
    {
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
        if (animator == null) return - 1f;
        
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
    public void TakeHeal(float amount)
    {
        if (baseCharacter.IsDead) return;
        baseCharacter.CurHp += amount;
        // 힐양GO 소환: 힐은 일단 모두 같은 오브젝트 풀 사용
        GameObject healGO;
        healGO = ObjectPoolManager.Instance.Get(PoolType.HealAmount);
        HealAmount healAmount = healGO.GetComponent<HealAmount>();
        healAmount.SetAmount(Mathf.CeilToInt(amount));
        healGO.transform.position = baseCharacter.transform.position + Vector3.up * 1.85f;
        /*if (baseCharacter is Player player || baseCharacter is PlayerUnit playerUnit || baseCharacter is PlayerHQ playerHQ)
        {
            
        }*/
    }
    public bool IsDead()
    {
        return baseCharacter.IsDead;
    }

}
