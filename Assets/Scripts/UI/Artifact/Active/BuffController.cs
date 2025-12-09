using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

#region 251128: 리팩토링 -> 완성 시 다른 코드 삭제, 참조 수정 예정
// 코루틴 삭제, 버프/디버프 분류 삭제, 스킬 중복 시 시간 초기화로, 스탯 자체 중첩은 가능하도록
// 버프/디버프 사용하는 것들 
public enum BuffSource
{
    // 액티브 아티팩트 스킬들
    Skill_KingMarch,       // 왕국의 진군가
    Skill_IceSpiritBreath, // 얼음 정령의 숨결
    // 추후 시너지 등등 추가 예정
}
public enum IntegratedBuffType
{
    AtkackPower,   // 공격력
    AttackRate,    // 공격 속도
    MoveSpeed,     // 이동 속도
    //ChangeColor    // 색상 변경 -> 별도 처리 예정
}
[System.Serializable]
public class BuffStat
{
    public IntegratedBuffType Type { get; private set; } // 디버그용
    public float PercentValue { get; set; }

    public BuffStat(IntegratedBuffType type)
    {
        Type = type;
        PercentValue = 0f;
    }
}
[System.Serializable]
public abstract class BuffTimer
{
    public BuffSource Source { get; private set; }  // 디버깅용 (어떤 스킬인지)
    public bool IsActive { get; set; }      // 켜져 있는가?
    public float Duration { get; set; }     // 남은 시간
    public BuffTimer(BuffSource source)
    {
        Source = source;
        IsActive = false;
        Duration = -1f;
    }
}
[System.Serializable]
public class ActiveBuff : BuffTimer
{
    // 리스트는 클래스 생성 시 미리 할당 (재사용 예정)
    public List<BuffStat> BuffStats = new List<BuffStat>();

    // 초기화 편의 함수
    public ActiveBuff(BuffSource source) : base(source) { }
}
[System.Serializable]
public class BuffColor : BuffTimer
{
    public Color changedColor { get; set; }
    public BuffColor(BuffSource source) : base(source)
    {
        changedColor = Color.white;
    }
}
#endregion

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
    #region 251128: 리팩토링 -> 완성 시 다른 코드 삭제, 참조 수정 예정
    bool isReFactoringDone = false; // 리팩토링 완료 플래그 -> 완성 시 삭제

    // 활성화된 버프 수
    List<ActiveBuff> activeBuffs = new List<ActiveBuff>();
    List<BuffColor> buffColors = new List<BuffColor>();
    float colorChangeTime = 0.2f;
    float colorChangeTimer = 0f;
    int colorChangeIndex = -1;
    int buffSourceCount = 0;
    int buffStatCount = 0;
    #endregion

    private BaseUnit _baseUnit;       // 스탯을 변경할 대상
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
        _baseUnit = GetComponent<BaseUnit>();
        _spriteRenderer = GetComponentsInChildren<SpriteRenderer>(true);
        if (_baseUnit == null) Debug.LogError($"{name}에서 BaseCharacter를 찾을 수 없습니다.");

        foreach (var sp in _spriteRenderer)
        {
            _colors.Add(sp.color);
        }
        buffSourceCount = System.Enum.GetValues(typeof(BuffSource)).Length;
        buffStatCount = System.Enum.GetValues(typeof(IntegratedBuffType)).Length;
        for(int i = 0; i < buffSourceCount; i++)
        {

            ActiveBuff newBuff = new ActiveBuff((BuffSource)i);
            for(int j = 0; j < buffStatCount; j++)
            {
                newBuff.BuffStats.Add(new BuffStat((IntegratedBuffType)j));
            }
            activeBuffs.Add(newBuff);
            buffColors.Add(new BuffColor((BuffSource)i));
        }
    }
    #region 251128: 리팩토링 -> 완성 시 다른 코드 삭제, 참조 수정 예정
    private void Update()
    {
        if (!isReFactoringDone) return; // 리팩토링 완료 전까지는 리턴

        // 버프 관련 업데이트
        UpdateActiveBuffs();
        // 버프 색 관련 업데이트
        UpdateBuffColors();
    }
    void UpdateActiveBuffs()
    {
        // 버프 관련
        for (int i = 0; i < buffSourceCount; i++)
        {
            if (!activeBuffs[i].IsActive) continue;
            activeBuffs[i].Duration -= Time.deltaTime;
            if (activeBuffs[i].Duration > 0f) continue;
            // 버프 종료 시
            activeBuffs[i].IsActive = false;
            // 능력치 원상복구 처리
            for (int j = 0; j < buffStatCount; j++)
            {
                // 값이 0이면 패스
                if (activeBuffs[i].BuffStats[j].PercentValue == 0f) continue;
                // 능력치 원상복구, 기존 값의 마이너스 값으로 설정
                _baseUnit.SetBuffStat((IntegratedBuffType)j, -activeBuffs[i].BuffStats[j].PercentValue);
                activeBuffs[i].BuffStats[j].PercentValue = 0f; // *필수, 값 초기화 -> ApplyBuff에서 값 비교하기 때문
            }
        }
    }
    private void UpdateBuffColors()
    {
        int buffColorActiveCnt = 0;
        int buffColorActiveIdx = -1;
        // 버프 색관련
        for (int i = 0; i < buffSourceCount; i++)
        {
            if (!buffColors[i].IsActive) continue;
            buffColors[i].Duration -= Time.deltaTime;
            if (buffColors[i].Duration > 0f)
            {
                buffColorActiveCnt++;
                buffColorActiveIdx = i;
                continue;
            }
            buffColors[i].IsActive = false;
        }

        // 색관련 버프가 있다면 순차별 색상 변경
        if (buffColorActiveCnt > 1)
        {
            colorChangeTimer += Time.deltaTime;

            // 현재 적용된 색상 버프가 비활성 상태면 즉시 변경해야 함
            bool isCurrentColorValid = (colorChangeIndex != -1) && buffColors[colorChangeIndex].IsActive;

            // 타이머가 아직 안 됐고 && 현재 색상이 유효하다면 -> 대기
            if (colorChangeTimer < colorChangeTime && isCurrentColorValid) return;

            colorChangeTimer = colorChangeTimer - colorChangeTime; // 초과분 보정

            int searchIdx = colorChangeIndex;
            for (int i = 0; i < buffSourceCount; i++)
            {
                // (현재 + 1) % 전체개수 -> 다음 인덱스 (끝이면 0으로)
                searchIdx = (searchIdx + 1) % buffSourceCount;

                if (!buffColors[searchIdx].IsActive) continue;

                colorChangeIndex = searchIdx; // 인덱스 갱신
                ChangeColor(buffColors[searchIdx].changedColor); // 색상 적용
                break; // 찾았으니 탈출
            }
            
        }
        // 하나라면 바로 적용
        else if (buffColorActiveCnt == 1)
        {
            if (colorChangeIndex == buffColorActiveIdx) return;
            colorChangeTimer = 0f;
            colorChangeIndex = buffColorActiveIdx;
            ChangeColor(buffColors[buffColorActiveIdx].changedColor);
        }
        // 없다면 기존 색상으로
        else
        {
            if (colorChangeIndex == -1) return;
            colorChangeIndex = -1;
            ToOriginColor();
        }
    }
    public void ApplyBuff(BuffSource buffSource, IntegratedBuffType buffType, float duration, float percentValue)
    {
        activeBuffs[(int)buffSource].IsActive = true;
        activeBuffs[(int)buffSource].Duration = duration;

        float diff = percentValue - activeBuffs[(int)buffSource].BuffStats[(int)buffType].PercentValue;
        // 능력치 변동이 없다면 패스
        if (diff == 0) return;
        // 데이터 저장
        activeBuffs[(int)buffSource].BuffStats[(int)buffType].PercentValue = percentValue;
        // '차이'만큼 능력치 적용
        _baseUnit?.SetBuffStat(buffType, diff);
    }
    public void ApplyBuffColor(BuffSource buffSource, Color newColor, float duration)
    {
        buffColors[(int)buffSource].IsActive = true;
        buffColors[(int)buffSource].Duration = duration;
        buffColors[(int)buffSource].changedColor = newColor;
    }
    void ChangeColor(Color newColor)
    {
        if (_spriteRenderer == null) return;
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
    }
    #endregion

    private void OnDisable()
    {
        #region 251128: 리팩토링 -> 완성 시 다른 코드 삭제, 참조 수정 예정
        // 버프 초기화
        // 능력치는 기본 스탯으로 알아서 설정됨
        for (int i = 0; i < buffSourceCount; i++)
        {
            activeBuffs[i].IsActive = false;
            /*for (int j = 0; j < buffStatCount; j++)
            {
                activeBuffs[i].BuffStats[j].Value = -1f; // 필요 없겠지만 안전하게 초기화
            }*/
            buffColors[i].IsActive = false;
        }
        // 색상 복구
        for (int i = 0; i < _colors.Count; i++)
        {
            if (_spriteRenderer[i].gameObject.layer != colorTargetlayer) continue;

            _spriteRenderer[i].color = _colors[i];
        }
        #endregion

        if (_Co_ChangeColor != null) StopCoroutine(_Co_ChangeColor);
        if(_Co_ApplySlowDebuff != null) StopCoroutine(_Co_ApplySlowDebuff);
        if(_Co_ApplyAttackCooldownDebuff != null) StopCoroutine(_Co_ApplyAttackCooldownDebuff);
        if(_Co_ApplyAttackBuff != null) StopCoroutine(_Co_ApplyAttackBuff);
        if(_Co_ApplyAttackSpeedBuff != null) StopCoroutine(_Co_ApplyAttackSpeedBuff);
    }

    public void ApplyBuff(BuffType type, float duration, float value)
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
    }
}