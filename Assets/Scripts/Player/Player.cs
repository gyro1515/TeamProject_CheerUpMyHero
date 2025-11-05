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

    [field: Header("플레이어 오라")]
    [field: SerializeField] public float AuraRange { get; private set; }
    [field: SerializeField] public float AuraAtkBonus { get; private set; }
    [Header("오라 연결")]
    [SerializeField] private GameObject playerAura;

    private HashSet<PlayerUnit> _unitInAura = new HashSet<PlayerUnit>();

    private float _auraCheckTimer = 0f;
    private const float AuraCheckInterver = 0.1f;

    float curMana;
    int curLevel = 1;
    int curExp = 0; // 나중에는 PlayerDataManager에서 관리
    // --- 사운드 재생을 위한 플래그 ---
    private bool isPlayedSound70 = false;
    private bool isPlayedSound40 = false;
    private bool isPlayedSound10 = false;
    public override float CurHp
    {
        get => base.CurHp;
        set
        {
            base.CurHp = value;
            if (curHp == MaxHp) return; // 최대 체력이라면 아래 실행x
            // 체력 비율에 따른 효과음 재생
            CheckHealthRatioAndPlaySound();
        }
    }
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
                //int tmpExp = curExp - PlayerData.exp; // 남은 경험치
                ////onPlayerLevelUpEvent?.Publish(new PlayerLevelUpEvent()); // 레벨업 이벤트 발행
                //CurExp = tmpExp; // 계속 레벨업 가능하도록 재귀호출

                curExp = 0;
            }

            PlayerDataManager.Instance.CurExp = curExp;
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
            GameManager.Instance.ShowResultUI(false).Forget(); // await 일부러 뺀거에 컴파일 경고 안뜨드록 처리
        };
        onPlayerLevelUpEvent = EventManager.GetPublisher<PlayerLevelUpEvent>();

        //GameManager에게 전투 시작 준비를 명령
        GameManager.Instance.StartBattle(); //배틀씬으로 갔을 때부터 식량 획득 증가 함수
        PlayerController = GetComponent<PlayerController>();

        //TODO 
    }
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        _auraCheckTimer += Time.deltaTime;

        if (_auraCheckTimer >= AuraCheckInterver)
        {
            _auraCheckTimer -= AuraCheckInterver;
            if (!IsDead)
            {
                UpdateAuraBuffs();
            }
        }
    }

    protected override void FixedUpdate()
    {
        base.Update();
        // 테스트로 플레이어는 계속 정렬해주기
        //InitCharacter();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    public void PlayerLevelUP() //
    {
        curLevel++;

        PlayerDataManager.Instance.PlayerLevel = curLevel;

        PlayerData = DataManager.PlayerData.GetData(curLevel);
        UnitData = PlayerData;

        SetDataFromExcelData();
        SetStatMultiplier();
    }
    protected override EffectTarget GetEffectTarget()
    {
        return EffectTarget.Player;
    }
    public override void SetStatMultiplier(float statMultiplier = 1f, bool isSpawnHero = false)
    {
        if (PlayerData == null || PlayerData.level == 0) { Debug.LogError("데이터 없음"); return; }
        // 배율에 따른 체력 공격력 세팅

        EffectTarget target = GetEffectTarget();
        float hpModifierBonus = Modifiercalculator.GetMultiplier(target, StatType.MaxHp, this.UnitData);
        float atkModifierBonus = Modifiercalculator.GetMultiplier(target, StatType.AtkPower, this.UnitData);
        float moveSpeedModifierBonus = Modifiercalculator.GetMultiplier(target, StatType.MoveSpeed, this.UnitData);

        float hpArtifactBonusPercent = ArtifactManager.Instance.GetPassiveArtifactStatBonus(target, StatType.MaxHp) / 100f;
        float atkArtifactBonusPercent = ArtifactManager.Instance.GetPassiveArtifactStatBonus(target, StatType.AtkPower) / 100f;
        float moveSpeedArtifactBonusPercent = ArtifactManager.Instance.GetPassiveArtifactStatBonus(target, StatType.MoveSpeed) / 100f;
        float auraArtifactBonusPercent = ArtifactManager.Instance.GetPassiveArtifactStatBonus(target, StatType.AuraRange) / 100f;

        MaxHp = PlayerData.health * (hpModifierBonus + statMultiplier + hpArtifactBonusPercent);
        curHp = MaxHp;
        AtkPower = PlayerData.atkPower * (atkModifierBonus + statMultiplier + hpArtifactBonusPercent);
        AttackRate = PlayerData.attackRate * statMultiplier; // 공격 속도는 크기와 상관없이 배율에 비례
        MoveSpeed = PlayerData.moveSpeed * (moveSpeedModifierBonus + 1f + moveSpeedArtifactBonusPercent);
        AuraRange = PlayerData.auraRange * (1f + auraArtifactBonusPercent);
        AuraAtkBonus = PlayerData.auraAtkBonus;
        playerAura.transform.localScale = Vector3.one * AuraRange;

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
        curLevel = PlayerDataManager.Instance.PlayerLevel;
        curExp = PlayerDataManager.Instance.CurExp;

        PlayerData = DataManager.PlayerData.GetData(curLevel);
        UnitData = PlayerData;
        Damageable = GetComponent<IDamageable>();
        //BaseController = UnitController;
    }

    void CheckHealthRatioAndPlaySound()
    {
        int healthRatio = Mathf.CeilToInt(curHp / MaxHp * 100);
        Debug.Log($"현재 체력 비율{healthRatio}");
        // 1. 체력이 10% 이하이고, 아직 10% 사운드를 재생한 적이 없다면
        if (healthRatio <= 10 && !isPlayedSound10)
        {
            AudioManager.PlayOneShotByCameraDistance(DataManager.AudioData.playerWarningHpSE, gameObject.transform, 1.5f);
            isPlayedSound10 = true;  // "재생했음"으로 표시
        }
        // 2. 체력이 40% 이하이고, 아직 40% 사운드를 재생한 적이 없다면
        else if (healthRatio <= 40 && !isPlayedSound40)
        {
            AudioManager.PlayOneShotByCameraDistance(DataManager.AudioData.playerWarningHpSE, gameObject.transform, 1.5f);
            isPlayedSound40 = true;  // "재생했음"으로 표시
        }
        // 3. 체력이 70% 이하이고, 아직 70% 사운드를 재생한 적이 없다면
        else if (healthRatio <= 70 && !isPlayedSound70)
        {
            AudioManager.PlayOneShotByCameraDistance(DataManager.AudioData.playerWarningHpSE, gameObject.transform, 1.5f);
            isPlayedSound70 = true;  // "재생했음"으로 표시
        }

        // (중요) 체력이 회복되었을 때 플래그를 다시 false로 리셋
        ResetSoundFlags(healthRatio);
    }
    void ResetSoundFlags(int healthRatio)
    {
        // 70%보다 체력이 많아지면 모든 플래그 리셋
        if (healthRatio > 70)
        {
            isPlayedSound70 = false;
            isPlayedSound40 = false;
            isPlayedSound10 = false;
        }
        // 40%보다 많아지면 40%와 10% 플래그 리셋
        else if (healthRatio > 40)
        {
            isPlayedSound40 = false;
            isPlayedSound10 = false;
        }
        // 10%보다 많아지면 10% 플래그만 리셋
        else if (healthRatio > 10)
        {
            isPlayedSound10 = false;
        }
    }

    private void UpdateAuraBuffs()
    {
        if (gameObject == null) return;

        Vector3 playerPos = gameObject.transform.position;
        List<BaseCharacter> unitList = UnitManager.PlayerUnitList;

        foreach (var unit in unitList)
        {
            if (unit == null || unit == this || unit.IsDead) continue;
            if (!(unit is PlayerUnit playerUnit)) continue;

            float dist = Mathf.Abs(playerUnit.transform.position.x - playerPos.x);

            if (dist > AuraRange)
            {
                playerUnit.RemoveAuraBuff();
            }
            else
            {
                playerUnit.ApplyAuraBuff(PlayerData.auraAtkBonus);
            }
        }
    }
}
#region 플레이어 레벨 업 이벤트
public struct PlayerLevelUpEvent{ } // 추후 필요한 정보 있으면 추가
#endregion
