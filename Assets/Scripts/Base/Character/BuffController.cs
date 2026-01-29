using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region 251128: 리팩토링 -> 완성 시 다른 코드 삭제, 참조 수정 예정
// 코루틴 삭제, 버프/디버프 분류 삭제, 스킬 중복 시 시간 초기화로, 스탯 자체 중첩은 가능하도록
// 버프/디버프 사용하는 것들 
public enum BuffSource
{
    None,
    // 액티브 아티팩트 스킬들
    Skill_KingMarch,       // 왕국의 진군가
    Skill_IceSpiritBreath, // 얼음 정령의 숨결
    // 추후 시너지 등등 추가 예정
}
public enum IntegratedBuffType
{
    AttackPower,   // 공격력
    AttackRate,    // 공격 속도
    MoveSpeed,     // 이동 속도
}
public enum BuffColorType
{
    None, // 기본, white
    Red,
    Blue,
    Green,
    Yellow, // red + green
    Magenta, // red + blue
    Cyan    // blue + green
}
// TODO: 상태이상 관련 enum 및 클래스 추가 예정
#endregion

/*public enum BuffType
{
    AttackDamage,   // 공격력 증가
    AttackSpeed     // 공격 속도 증가
}
public enum DebuffType
{
    MoveSpeed,      // 이동 속도 감소
    AttackCooldown  // 공격 쿨타임(속도) 감소 (증가)
}*/
public class BuffController : MonoBehaviour
{
    #region 251128: 리팩토링 -> 완성 시 다른 코드 삭제, 참조 수정 예정
    // 활성화된 버프 수
    List<ActiveBuff> activeBuffs = new List<ActiveBuff>();
    Dictionary<BuffSource, ActiveBuff> activeBuffDict = new Dictionary<BuffSource, ActiveBuff>();
    Stack<ActiveBuff> activeBuffStack = new Stack<ActiveBuff>();
    List<BuffColor> buffColors = new List<BuffColor>();
    Dictionary<BuffColorType, BuffColor> buffColorDict = new Dictionary<BuffColorType, BuffColor>();
    Stack<BuffColor> buffColorStack = new Stack<BuffColor>();
    float colorChangeTime = 0.2f;
    float colorChangeTimer = 0f;
    int colorCycleIndex = 0;
    Color appliedColor = Color.clear;
    #endregion

    private BaseUnit _baseUnit;       // 스탯을 변경할 대상
    private SpriteRenderer[] _spriteRenderer; // 색상을 변경할 대상
    List<Color> _colors = new List<Color>();
    int colorTargetlayer;
    /*Coroutine _Co_ChangeColor;
    Coroutine _Co_ApplySlowDebuff;
    Coroutine _Co_ApplyAttackCooldownDebuff;
    Coroutine _Co_ApplyAttackBuff;
    Coroutine _Co_ApplyAttackSpeedBuff;*/
    private void Awake()
    {
        colorTargetlayer = LayerMask.NameToLayer("Animation");
        _baseUnit = GetComponent<BaseUnit>();
        _spriteRenderer = GetComponentsInChildren<SpriteRenderer>(true);
        if (_baseUnit == null) Debug.LogError($"{name}에서 BaseCharacter를 찾을 수 없습니다.");

        foreach (var sp in _spriteRenderer)
        {
            _colors.Add(sp.color);
        }
    }
    #region 251128: 리팩토링 -> 완성 시 다른 코드 삭제, 참조 수정 예정
    private void Update()
    {
        // 버프 관련 업데이트
        UpdateActiveBuffs();
        // 버프 색 관련 업데이트
        UpdateBuffColors();
    }
    void UpdateActiveBuffs()
    {
        if (activeBuffs.Count == 0) return;

        // 버프 관련 업데이트 및 삭제 처리
        int idx = 0;
        while (idx < activeBuffs.Count)
        {
            // 버프 타이머 업데이트
            activeBuffs[idx].UpdateBuffTimer(Time.deltaTime);

            // 활성화된 버프면 다음으로
            if (activeBuffs[idx].IsActive)
            {
                idx++;
                continue;
            }
            // 비활성화된 버프 스왑 백 후 삭제, 재사용을 위해 스택에 보관
            activeBuffDict.Remove(activeBuffs[idx].Source);
            int lastIdx = activeBuffs.Count - 1;
            activeBuffStack.Push(activeBuffs[idx]);
            if(idx < lastIdx)
            {
                activeBuffs[idx] = activeBuffs[lastIdx];
            }
            // 리스트 맨뒤 삭제
            activeBuffs.RemoveAt(lastIdx);
        }
    }
    private void UpdateBuffColors()
    {
        if (buffColors.Count == 0) return;

        // 전체 버프 색상 업데이트 및 삭제 처리
        int idx = 0;
        // 적용된 색상 유효 체크용
        bool isCurrentColorValid = true;
        while (idx < buffColors.Count)
        {
            buffColors[idx].UpdateBuffTimer(Time.deltaTime);

            if (buffColors[idx].IsActive)
            {
                idx++;
                continue;
            }
            // 만약 현재 적용된 색상이라면 적용된 색상 초기화
            if (appliedColor == buffColors[idx].changedColor)
            {
                // 현재 적용된 색상이 비활성화 됐다면 현재 색상 유효하지 않음으로 변경
                isCurrentColorValid = false;
            }

            // 비활성화된 버프 스왑 백 후 삭제, 재사용을 위해 스택에 보관
            buffColorDict.Remove(buffColors[idx].Type);
            int lastIdx = buffColors.Count - 1;
            buffColorStack.Push(buffColors[idx]);
            if (idx < lastIdx)
            {
                buffColors[idx] = buffColors[lastIdx];
            }
            // 리스트 맨뒤 삭제
            buffColors.RemoveAt(lastIdx);
            
        }
        // 색관련 버프가 있다면 순차별 색상 변경
        if (buffColors.Count > 1)
        {
            colorChangeTimer += Time.deltaTime;

            // 타이머가 아직 안 됐고 && 현재 색상이 유효하다면 -> 대기
            if (colorChangeTimer < colorChangeTime && isCurrentColorValid) return;

            // 타이머가 됐거나, 현재 색상이 유효하지 않다면 색상 변경
            colorChangeTimer -= colorChangeTime; // 초과분 보정

            // 다음 인덱스로 이동 (나머지 연산으로 순환)
            colorCycleIndex = (colorCycleIndex + 1) % buffColors.Count;

            // 해당 인덱스의 색상 적용
            Color targetColor = buffColors[colorCycleIndex].changedColor;

            // 만약 방금 적용된 색과 같다면(리스트 변경 등으로 인해) 다음 걸로 한 번 더 이동
            if (appliedColor == targetColor)
            {
                colorCycleIndex = (colorCycleIndex + 1) % buffColors.Count;
                targetColor = buffColors[colorCycleIndex].changedColor;
            }
            ChangeColor(targetColor);
        }
        // 하나라면 바로 적용
        else if (buffColors.Count == 1)
        {
            if (appliedColor == buffColors[0].changedColor) return;
            colorChangeTimer = 0f;
            ChangeColor(buffColors[0].changedColor);
        }
        // 없다면 기존 색상으로
        else
        {
            if (appliedColor == Color.clear) return;
            ToOriginColor();
        }
    }
    public void ApplyBuff(BuffSource buffSource, List<BuffEffect> buffEffects, float duration)
    {
        if (activeBuffDict.TryGetValue(buffSource, out ActiveBuff existingBuff))
        {
            // 이미 존재하는 버프가 있으면 지속시간 갱신
            existingBuff.RefreshActiveBuff(duration);
        }
        else
        {
            // 새로운 버프 생성
            ActiveBuff newBuff;
            // 재사용 가능한 버프가 있으면 사용 -> 풀링, 힙 재사용해 GC 최소화
            if (activeBuffStack.Count > 0)
            {
                newBuff = activeBuffStack.Pop();
            }
            else
            {
                newBuff = new ActiveBuff();
            }
            newBuff.ApplyActiveBuff(_baseUnit, buffSource, buffEffects, duration);
            activeBuffs.Add(newBuff);
            activeBuffDict[buffSource] = newBuff;
        }
    }
    private Color GetColorByType(BuffColorType type)
    {
        switch (type)
        {
            case BuffColorType.Red: return Color.red;
            case BuffColorType.Blue: return Color.blue;
            case BuffColorType.Green: return Color.green;
            case BuffColorType.Yellow: return Color.yellow;
            case BuffColorType.Magenta: return Color.magenta;
            case BuffColorType.Cyan: return Color.cyan;
            default: return Color.white;
        }
    }
    public void ApplyBuffColor(BuffColorType buffColorType, float duration)
    {
        // 이미 존재하는 버프가 있으면 지속시간 갱신
        if (buffColorDict.TryGetValue(buffColorType, out BuffColor buffColor))
        {
            buffColor.RefreshActiveBuff(duration);
            return;
        }
        // 없다면 새로운 버프 생성
        Color newColor = GetColorByType(buffColorType);
        BuffColor newBuffColor;
        // 재사용 가능한 버프가 있으면 사용 -> 풀링, 힙 재사용해 GC 최소화
        if (buffColorStack.Count > 0)
        {
            newBuffColor = buffColorStack.Pop();
        }
        else
        {
            newBuffColor = new BuffColor();
        }
        newBuffColor.ApplyBuffColor(buffColorType, newColor, duration);
        buffColors.Add(newBuffColor);
        buffColorDict[buffColorType] = newBuffColor;
    }
    void ChangeColor(Color newColor)
    {
        if (_spriteRenderer == null) return;
        appliedColor = newColor;
        foreach (var sp in _spriteRenderer)
        {
            if (sp.gameObject.layer != colorTargetlayer) continue;

            sp.color = newColor;
        }
    }
    void ToOriginColor()
    {
        if (_spriteRenderer == null) return;
        for (int i = 0; i < _spriteRenderer.Length; i++)
        {
            _spriteRenderer[i].color = _colors[i];
        }
        appliedColor = Color.clear;
    }
    #endregion

    private void OnDisable()
    {
        #region 251128: 리팩토링 -> 완성 시 다른 코드 삭제, 참조 수정 예정
        // 버프 초기화
        for (int i = 0; i < activeBuffs.Count; i++)
        {
            activeBuffs[i].Reset();
            // 재사용 위해 스택에 보관
            activeBuffStack.Push(activeBuffs[i]);
        }
        activeBuffs.Clear();
        activeBuffDict.Clear();
        // 색상 복구
        ToOriginColor();
        foreach (var buffColor in buffColors)
        {
            buffColor.Reset();
            buffColorStack.Push(buffColor);
        }
        buffColorDict.Clear();
        buffColors.Clear();
        #endregion

        /*if (_Co_ChangeColor != null) StopCoroutine(_Co_ChangeColor);
        if(_Co_ApplySlowDebuff != null) StopCoroutine(_Co_ApplySlowDebuff);
        if(_Co_ApplyAttackCooldownDebuff != null) StopCoroutine(_Co_ApplyAttackCooldownDebuff);
        if(_Co_ApplyAttackBuff != null) StopCoroutine(_Co_ApplyAttackBuff);
        if(_Co_ApplyAttackSpeedBuff != null) StopCoroutine(_Co_ApplyAttackSpeedBuff);*/
    }

   /* public void ApplyBuff(BuffType type, float duration, float value)
    {
        if (_baseUnit == null) return;
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
        if (_baseUnit == null) return;
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
        float originalSpeed = _baseUnit.MoveSpeed;
        _baseUnit.SetMoveSpeed(originalSpeed * (1f - slowPercent / 100f));
        yield return new WaitForSeconds(duration);
        _baseUnit.SetMoveSpeed(originalSpeed);
    }

    private IEnumerator Co_ApplyAttackCooldownDebuff(float duration, float atkCooldownPercent)
    {
        float originalRate = _baseUnit.AttackRate;
        _baseUnit.SetAttackRate(originalRate * (1f + atkCooldownPercent / 100f));
        yield return new WaitForSeconds(duration);
        _baseUnit.SetAttackRate(originalRate); 
    }

    private IEnumerator Co_ApplyAttackBuff(float duration, float atkPercent)
    {
        float originalAtk = _baseUnit.AtkPower;
        _baseUnit.SetAttackPower(originalAtk * (1f + atkPercent / 100f));
        yield return new WaitForSeconds(duration);
        _baseUnit.SetAttackPower(originalAtk); 
    }

    private IEnumerator Co_ApplyAttackSpeedBuff(float duration, float atkSpeedPercent)
    {
        float originalRate = _baseUnit.AttackRate;
        _baseUnit.SetAttackRate(originalRate * (1f - atkSpeedPercent / 100f));
        yield return new WaitForSeconds(duration);
        _baseUnit.SetAttackRate(originalRate);
    }*/
}