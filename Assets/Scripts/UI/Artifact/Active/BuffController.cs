using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffType
{
    AttackDamage,   // 공격력 증가
    AttackSpeed     // 공격 속도 증가
}
public enum DebuffType
{
    MoveSpeed,      // 이동 속도 감소
    AttackCooldown  // 공격 쿨타임(속도) 감소 (증가)
}
public class BuffController : MonoBehaviour
{
    private BaseCharacter _character;       // 스탯을 변경할 대상
    private SpriteRenderer _spriteRenderer; // 색상을 변경할 대상

    private void Awake()
    {
        _character = GetComponent<BaseCharacter>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (_character == null) Debug.LogError($"{name}에서 BaseCharacter를 찾을 수 없습니다.");
    }


    public void ApplyBuff(BuffType type, float duration, float value)
    {
        if (_character == null) return;
        switch (type)
        {
            case BuffType.AttackDamage:
                StartCoroutine(Co_ApplyAttackBuff(duration, value));
                break;
            case BuffType.AttackSpeed:
                StartCoroutine(Co_ApplyAttackSpeedBuff(duration, value));
                break;
        }
    }

    public void ApplyDebuff(DebuffType type, float duration, float value)
    {
        if (_character == null) return;
        switch (type)
        {
            case DebuffType.MoveSpeed:
                StartCoroutine(Co_ApplySlowDebuff(duration, value));
                break;
            case DebuffType.AttackCooldown:
                StartCoroutine(Co_ApplyAttackCooldownDebuff(duration, value));
                break;
        }
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


    private IEnumerator Co_ChangeColor(Color newColor, float duration)
    {
        if (_spriteRenderer == null) yield break; // 안전장치
        Color originalColor = _spriteRenderer.color;
        _spriteRenderer.color = newColor;
        yield return new WaitForSeconds(duration);
        if (_spriteRenderer.color == newColor) _spriteRenderer.color = originalColor;
    }

    private IEnumerator Co_ApplySlowDebuff(float duration, float slowPercent)
    {
        float originalSpeed = _character.MoveSpeed;
        _character.SetMoveSpeed(originalSpeed * (1f - slowPercent / 100f));
        yield return new WaitForSeconds(duration);
        _character.SetMoveSpeed(originalSpeed);
    }

    private IEnumerator Co_ApplyAttackCooldownDebuff(float duration, float atkCooldownPercent)
    {
        float originalRate = _character.AttackRate;
        _character.SetAttackRate(originalRate * (1f + atkCooldownPercent / 100f));
        yield return new WaitForSeconds(duration);
        _character.SetAttackRate(originalRate); 
    }

    private IEnumerator Co_ApplyAttackBuff(float duration, float atkPercent)
    {
        float originalAtk = _character.AtkPower;
        _character.SetAttackPower(originalAtk * (1f + atkPercent / 100f));
        yield return new WaitForSeconds(duration);
        _character.SetAttackPower(originalAtk); 
    }

    private IEnumerator Co_ApplyAttackSpeedBuff(float duration, float atkSpeedPercent)
    {
        float originalRate = _character.AttackRate;
        _character.SetAttackRate(originalRate * (1f - atkSpeedPercent / 100f));
        yield return new WaitForSeconds(duration);
        _character.SetAttackRate(originalRate);
    }
}