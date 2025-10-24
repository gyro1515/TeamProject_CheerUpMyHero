using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : BaseUnit
{
    [field: Header("플레이어 세팅")]
    [field: SerializeField] public float ArtifactPower { get; private set; }
    [field: SerializeField] public float MaxMana { get; private set; } = 15;
    [field: SerializeField] public PlayerData PlayerData { get; private set; }
    float curMana;
    int curLevel = 1;
    int curExp = 0; // 나중에는 PlayerDataManager에서 관리
    public float CurMana { get { return curMana; }  set
        {
            curMana = value;
            curMana = Mathf.Clamp(curMana, 0, MaxMana);
            OnCurManaChanged?.Invoke(curMana, MaxMana);
        } }
    public int CurExp { get { return curExp; } 
        set 
        { 
            curExp = value;
            if(curExp >= PlayerData.exp)
            {
                PlayerLevelUP();
                int tmpExp = curExp - PlayerData.exp; // 남은 경험치
                //onPlayerLevelUpEvent?.Publish(new PlayerLevelUpEvent()); // 레벨업 이벤트 발행
                CurExp = tmpExp; // 계속 레벨업 가능하도록 재귀호출
            }
        }
    }
    //프로퍼티도 버추얼 오버라이드가 되네요??
    public override Vector3 MoveDir {
        get { return base.MoveDir; } 
        set
        {
            if (base.MoveDir == value) return;

            base.MoveDir = value;
            OnMoveDirChanged?.Invoke(base.MoveDir);
        } }

    public event Action<Vector3> OnMoveDirChanged;
    public event Action<float, float> OnCurManaChanged;
    IEventPublisher<PlayerLevelUpEvent> onPlayerLevelUpEvent;
    public PlayerController PlayerController { get; private set; }
    protected override void Awake()
    {
        base.Awake();
        GameManager.Instance.Player = this;
        UnitManager.Instance.AddUnitList(this, true);
        OnDead += () =>
        {
            UnitManager.Instance.RemoveUnitFromList(this, true);
            GameManager.Instance.ShowResultUI(false);
        };
        onPlayerLevelUpEvent = EventManager.GetPublisher<PlayerLevelUpEvent>();

        //GameManager에게 전투 시작 준비를 명령
        GameManager.Instance.StartBattle(); //배틀씬으로 갔을 때부터 식량 획득 증가 함수
        PlayerController = GetComponent<PlayerController>();

    }
    protected override void Start()
    {
        base.Start();
    }
    protected override void FixedUpdate()
    {
        base.Update();
        // 테스트로 플레이어는 계속 정렬해주기
        //InitCharacter();
    }
    public void PlayerLevelUP() //
    {
        curLevel++;
        SetDataFromExcelData();
        SetStatMultiplier();
    }
    protected override EffectTarget GetEffectTarget()
    {
        return EffectTarget.Player;
    }
    public override void SetStatMultiplier(float statMultiplier = 1f, bool isSpawnHero = false)
    {
        if (PlayerData == null) { Debug.LogError("데이터 없음"); return; }
        // 배율에 따른 체력 공격력 세팅

        EffectTarget target = GetEffectTarget();
        float hpModifierBonus = Modifiercalculator.GetMultiplier(target, StatType.MaxHp, this.UnitData);
        float atkModifierBonus = Modifiercalculator.GetMultiplier(target, StatType.AtkPower, this.UnitData);
        float moveSpeedModifierBonus = Modifiercalculator.GetMultiplier(target, StatType.MoveSpeed, this.UnitData);

        MaxHp = PlayerData.health * (hpModifierBonus + statMultiplier);
        curHp = MaxHp;
        AtkPower = PlayerData.atkPower * (atkModifierBonus + statMultiplier);
        AttackRate = PlayerData.attackRate * statMultiplier; // 공격 속도는 크기와 상관없이 배율에 비례
        MoveSpeed = PlayerData.moveSpeed * (moveSpeedModifierBonus + statMultiplier);
        
        // 251022 주석처리
        /*float tmpstatMultiplier = Math.Clamp(statMultiplier, 0.8f, 1.2f); // 크기는 너무 작아지거나 커지지 않도록 제한
        // 아래는 다 tmpstatMultiplier로 세팅, 크기에 따라 인식/공격 범위도 달라지도록
        gameObject.transform.localScale = TmpSize * tmpstatMultiplier;
        AttackRange = PlayerData.attackRange * tmpstatMultiplier;
        CognizanceRange = PlayerData.cognizanceRange * tmpstatMultiplier;*/
        curMana = MaxMana;

        /*CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        // 사이즈는 달라질 수 있으니 활성화 시마다 갱신
        knockbackHandler.Init(col.size.x * statMultiplier);*/
        // 251022 주석처리
        // knockbackHandler.Init((TmpSize * tmpstatMultiplier).x); 
        knockbackHandler.Init(TmpSize.x);
        // ex: 최대 체력 = 300 / HitBackCount = 3 => 데미지 100이 누적될때마다 히트백
        hitbackHp = MaxHp / PlayerData.hitBack;
        // ex: curHp / hitbackHp  => 2 -> 1 -> 0에서만 히트백이 발생하도록
        hitbackTriggerCount = PlayerData.hitBack - 1;
    }
    protected override void SetDataFromExcelData()
    {
        PlayerData = DataManager.PlayerData.GetData(curLevel);
        UnitData = PlayerData;
        Damageable = GetComponent<IDamageable>();
        //BaseController = UnitController;
    }
    protected override float GetStatBonus(StatType type)
    {
        return ArtifactManager.Instance.GetPassiveArtifactStatBonus(EffectTarget.Player, type);
    }
}
#region 플레이어 레벨 업 이벤트
public struct PlayerLevelUpEvent{ } // 추후 필요한 정보 있으면 추가
#endregion
