using System; 
using System.Collections;
using UnityEngine;


public class SummonedUnitController : PlayerUnit
{
    private float _lifeTimeDuration = 0f;

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        StartCoroutine(LifetimeCoroutine());
    }
    //public void Setup(float health, float duration)
    //{
    //    _lifeTimeDuration = duration;

    //    InitializeController();

    //    // 2. 체력 및 기본 스탯 설정
    //    MaxHp = health;
    //    CurHp = health;
    //    MoveSpeed = defaultMoveSpeed;
    //    AtkPower = defaultAtkPower; 
    //    AttackRate = defaultAttackRate; 

    //    // 3. 소멸 타이머 시작
    //    StartCoroutine(LifetimeCoroutine());

    //    if (UnitController != null)
    //    {
    //        UnitController.enabled = true;
    //        Debug.Log($"[소환수] {name}: AI 컨트롤러 ({UnitController.GetType().Name}) 활성화됨. 이동 속도: {MoveSpeed}");
    //    }
    //    else
    //    {
    //        Debug.LogError($"[소환수] {name}: AI 컨트롤러(UnitController)가 null입니다!");
    //    }
    //}

    private IEnumerator LifetimeCoroutine()
    {
        yield return new WaitForSeconds(_lifeTimeDuration);

        if(IsDead) yield break; // 이미 죽었으면 리턴

        Debug.Log($"{name} (소환수) 지속시간 만료. 사라집니다.");
        IsDead = true;
        UnitManager.Instance.RemoveUnitFromList(this, true);
        BaseController.SetDead();
        Debug.Log($"{UnitManager.PlayerUnitList.Count}");
    }
    public override void SetStatMultiplier(float statMultiplier, bool isSpawnHero = false)
    {
        if (UnitData == null) { Debug.LogError("데이터 없음"); return; }
        //if (gameObject.IsDestroyed() || gameObject == null)
        //{
        //    Debug.LogWarning($"왜 파괴됐을까?");
        //    return; // 비활성화/파괴된 상태라면 리턴
        //}
        ActiveArtifactData activeArtifactData = (ActiveArtifactData)(DataManager.ArtifactData.GetData(08010005));

        _lifeTimeDuration = activeArtifactData.levelData[activeArtifactData.curLevel].summonDuration;
        MaxHp = activeArtifactData.levelData[activeArtifactData.curLevel].summonHealth;
        curHp = MaxHp;
        AtkPower = UnitData.atkPower;
        MoveSpeed = UnitData.moveSpeed;

        AttackRate = UnitData.attackRate;
        AttackRange = UnitData.attackRange;
        CognizanceRange = UnitData.cognizanceRange; 

        knockbackHandler.Init((TmpSize).x);
        // ex: 최대 체력 = 300 / HitBackCount = 3 => 데미지 100이 누적될때마다 히트백
        hitbackHp = MaxHp / UnitData.hitBack;
        // ex: curHp / hitbackHp  => 2 -> 1 -> 0에서만 히트백이 발생하도록
        hitbackTriggerCount = UnitData.hitBack - 1;
    }

    //private void InitializeController()
    //{
    //    UnitController = GetComponent<GolemAIController>();
    //    if (UnitController == null)
    //    {
    //        Debug.LogWarning($"[소환수] {name}: GolemAIController가 프리팹에 없습니다. AddComponent로 추가합니다.");
    //        UnitController = gameObject.AddComponent<GolemAIController>();
    //    }

    //    BaseController = UnitController;
    //    Damageable = GetComponent<IDamageable>();
    //}
}