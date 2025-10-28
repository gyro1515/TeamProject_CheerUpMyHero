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
    private SpriteRenderer[] _spriteRenderer; // 색상을 변경할 대상
    List<Color> _colors = new List<Color>();
    int colorTargetlayer;
    Coroutine _Co_ChangeColor;
    Coroutine _Co_ApplySlowDebuff;
    Coroutine _Co_ApplyAttackCooldownDebuff;
    Coroutine _Co_ApplyAttackBuff;
    Coroutine _Co_ApplyAttackSpeedBuff;
    private void Awake()
    {
        colorTargetlayer = LayerMask.NameToLayer("Animation");
        _character = GetComponent<BaseCharacter>();
        _spriteRenderer = GetComponentsInChildren<SpriteRenderer>(true);
        if (_character == null) Debug.LogError($"{name}에서 BaseCharacter를 찾을 수 없습니다.");

        foreach (var sp in _spriteRenderer)
        {
            _colors.Add(sp.color);
        }
    }
    private void OnDisable()
    {
        for (int i = 0; i < _colors.Count; i++)
        {
            if (_spriteRenderer[i].gameObject.layer != colorTargetlayer) continue;

            _spriteRenderer[i].color = _colors[i];
        }

        if(_Co_ChangeColor != null) StopCoroutine(_Co_ChangeColor);
        if(_Co_ApplySlowDebuff != null) StopCoroutine(_Co_ApplySlowDebuff);
        if(_Co_ApplyAttackCooldownDebuff != null) StopCoroutine(_Co_ApplyAttackCooldownDebuff);
        if(_Co_ApplyAttackBuff != null) StopCoroutine(_Co_ApplyAttackBuff);
        if(_Co_ApplyAttackSpeedBuff != null) StopCoroutine(_Co_ApplyAttackSpeedBuff);
    }

    public void ApplyBuff(BuffType type, float duration, float value)
    {
        if (_character == null) return;
        switch (type)
        {
            case BuffType.AttackDamage:
                _Co_ApplyAttackBuff = StartCoroutine(Co_ApplyAttackBuff(duration, value));
                break;
            case BuffType.AttackSpeed:
                _Co_ApplyAttackSpeedBuff = StartCoroutine(Co_ApplyAttackSpeedBuff(duration, value));
                break;
        }
    }

    public void ApplyDebuff(DebuffType type, float duration, float value)
    {
        if (_character == null) return;
        switch (type)
        {
            case DebuffType.MoveSpeed:
                _Co_ApplySlowDebuff = StartCoroutine(Co_ApplySlowDebuff(duration, value));
                break;
            case DebuffType.AttackCooldown:
                _Co_ApplyAttackCooldownDebuff = StartCoroutine(Co_ApplyAttackCooldownDebuff(duration, value));
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
        _Co_ChangeColor = StartCoroutine(Co_ChangeColor(newColor, duration));
    }


    private IEnumerator Co_ChangeColor(Color newColor, float duration)
    {
        if (_spriteRenderer == null) yield break; // 안전장치
        
        foreach(var sp in _spriteRenderer)
        {
            if (sp.gameObject.layer == colorTargetlayer)
            {
                sp.color = newColor;
            }
        }
        //Color originalColor = _spriteRenderer.color;
        //_spriteRenderer.color = newColor;
        yield return new WaitForSeconds(duration);
        for (int i = 0; i < _spriteRenderer.Length; i++)
        {
            _spriteRenderer[i].color = _colors[i];
        }
        //if (_spriteRenderer.color == newColor) _spriteRenderer.color = originalColor;
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