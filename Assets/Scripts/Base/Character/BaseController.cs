using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
public enum BuffType
{
    AttackDamage,   // 공격력 증가
    AttackSpeed     // 공격 속도 증가
}

public enum DebuffType
{
    MoveSpeed,      // 이동 속도 감소
    AttackCooldown  // 공격 쿨타임(속도) 감소
}
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
    public void ApplyBuff(BuffType type, float duration, float value)
    {
        if (baseCharacter == null) return; // 스탯 없으면 중단

        switch (type)
        {
            case BuffType.AttackDamage:
                StartCoroutine(Co_ApplyAttackBuff(duration, value));
                break;
            case BuffType.AttackSpeed:
                StartCoroutine(Co_ApplyAttackSpeedBuff(duration, value));
                break;
        }
        Debug.Log($"{name}에게 {type} 버프 ({duration}초, 값:{value}%) 적용 시작");
    }

    public void ApplyDebuff(DebuffType type, float duration, float value)
    {
        if (baseCharacter == null) return; // 스탯 없으면 중단

        switch (type)
        {
            case DebuffType.MoveSpeed:
                StartCoroutine(Co_ApplySlowDebuff(duration, value));
                break;
            case DebuffType.AttackCooldown:
                StartCoroutine(Co_ApplyAttackCooldownDebuff(duration, value));
                break;
        }
        Debug.Log($"{name}에게 {type} 디버프 ({duration}초, 값:{value}%) 적용 시작");
    }

    public void ChangeColor(Color newColor, float duration)
    {
        if (_spriteRenderer == null)
        {
            Debug.LogWarning($"{name}에 SpriteRenderer가 없어 색상 변경 불가.");
            return;
        }
        StartCoroutine(Co_ChangeColor(newColor, duration));
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
    }
    public bool IsDead()
    {
        return baseCharacter.IsDead;
    }

    private IEnumerator Co_ChangeColor(Color newColor, float duration)
    {
        Color originalColor = _spriteRenderer.color; // 원래 색상 저장
        _spriteRenderer.color = newColor; // 새 색상 적용

        yield return new WaitForSeconds(duration); // 지속시간만큼 대기

        // 복원 시, 다른 효과로 색이 또 바뀌었는지 확인 (간단한 중첩 처리)
        if (_spriteRenderer.color == newColor)
        {
            _spriteRenderer.color = originalColor; // 원래 색상 복원
        }
    }

    private IEnumerator Co_ApplySlowDebuff(float duration, float slowPercent)
    {
        float originalSpeed = baseCharacter.MoveSpeed;
        baseCharacter.SetMoveSpeed(originalSpeed * (1f - slowPercent / 100f));

        yield return new WaitForSeconds(duration);

        baseCharacter.SetMoveSpeed(originalSpeed); // 원상 복구
    }

    private IEnumerator Co_ApplyAttackCooldownDebuff(float duration, float atkCooldownPercent)
    {
        float originalRate = baseCharacter.AttackRate;
        baseCharacter.SetAttackRate(originalRate * (1f + atkCooldownPercent / 100f));

        yield return new WaitForSeconds(duration);

        baseCharacter.SetAttackRate(originalRate); // 원상 복구
    }

    private IEnumerator Co_ApplyAttackBuff(float duration, float atkPercent)
    {
        float originalAtk = baseCharacter.AtkPower;
        baseCharacter.SetAttackPower(originalAtk * (1f + atkPercent / 100f));

        yield return new WaitForSeconds(duration);

        baseCharacter.SetAttackPower(originalAtk); // 원상 복구
    }

    private IEnumerator Co_ApplyAttackSpeedBuff(float duration, float atkSpeedPercent)
    {
        float originalRate = baseCharacter.AttackRate;
        // ✨ 함수를 호출()하고, 계산된 새 값을 전달합니다 ✨
        baseCharacter.SetAttackRate(originalRate * (1f - atkSpeedPercent / 100f));

        yield return new WaitForSeconds(duration);

        baseCharacter.SetAttackRate(originalRate); // 원상 복구
    }
}
