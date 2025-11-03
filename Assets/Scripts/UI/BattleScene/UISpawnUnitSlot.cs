using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class UISpawnUnitSlot : MonoBehaviour
{
    [Header("유닛 선택 소환 아이콘 설정")]
    [SerializeField] RawImage unitIcon;
    [SerializeField] Image unitIconTimer; // 쿨타임 표시, 버튼 클릭 방지용
    [SerializeField] Image unitIBG; // 시너지 표시 백그라운드
    [SerializeField] Button spawnUnitBtn;
    [SerializeField] UIAdvancedButton spawnUnitAdvancedBtn;
    //[SerializeField] TextMeshProUGUI text; // 추후 아이콘만 설정하면 될 듯 합니다.
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] GameObject outlineGOForCanSpawnLegendary; // 전설 등급 소환 가능시 아웃라인 효과

    float _cooldown = -1f;
    float _cooldownTimer = -1f;
    bool isCooldown = false;
    int _foodConsumption = 0;
    PoolType playerUnitType;

    Color whiteCol = Color.white;
    Color grayCol = new Color(100f / 255f, 100f / 255f, 100f / 255f, 1f);
    UnitTextureHandler unitTextureHandler;
    // 식량에 따른 소환 가능 여부 아이콘 처리
    bool canSpawnUnit = true;
    BaseUnitData cardData;

    IEventPublisher<SpawnUnitSlotStartHoldEvent> spawnUnitSlotStartHoldEventPub;
    IEventPublisher<SpawnUnitSlotReleaseHoldEvent> spawnUnitSlotReleaseHoldEventPub;
    private void Awake()
    {
        costText.gameObject.SetActive(true); // 현재 왜 꺼져있는지 모르겠음
        costText.raycastTarget = false; // 텍스트가 버튼 클릭 막는 현상 방지
        outlineGOForCanSpawnLegendary.SetActive(false);
        //spawnUnitBtn.onClick.AddListener(OnSpawnUnit);
    }
    private void Update()
    {
        // 식량체크는 계속 해줘야함
        CheckCanSpawnUnitByCost();

        if (!isCooldown) return; // 쿨타임이 아니면 리턴
        _cooldownTimer += Time.deltaTime;
        unitIconTimer.fillAmount = 1 - _cooldownTimer / _cooldown;
        if (_cooldownTimer < _cooldown) return; // 아직 쿨타임이 다 안돌았다면 리턴
        unitIconTimer.fillAmount = 1f;
        SetTimerIconActive(false);
    }
    public void InitSpawnUnitSlot(BaseUnitData cardData)
    {
        if(cardData == null)
        {
            InitSpawnUnitSlot("비었음", -1, PoolType.None, 0, -1);
            return;
        }
        this.cardData = cardData;
        InitSpawnUnitSlot(cardData.unitName, cardData.idNumber, cardData.poolType, cardData.spawnCooldown, cardData.cost);
        // 유닛 시너지에 따른 배경 설정
        unitIBG.sprite = cardData.unitBGSprite;
        spawnUnitSlotStartHoldEventPub = EventManager.GetPublisher<SpawnUnitSlotStartHoldEvent>();
        spawnUnitSlotReleaseHoldEventPub= EventManager.GetPublisher<SpawnUnitSlotReleaseHoldEvent>();
        spawnUnitAdvancedBtn.onShortClick += OnSpawnUnit;
        spawnUnitAdvancedBtn.onHoldStart += () =>
        {
            spawnUnitSlotStartHoldEventPub?.Publish(new SpawnUnitSlotStartHoldEvent(this.cardData));
        };
        spawnUnitAdvancedBtn.onHoldRelease += () =>
        {
            spawnUnitSlotReleaseHoldEventPub?.Publish();
        };
    }
    void InitSpawnUnitSlot(string unitName, int unitId, PoolType poolType, float cooldown, int foodConsumption)
    {
        // 플레이어 유닛 코스트 증감 보너스 값
        float costModifierBonus = Modifiercalculator.GetMultiplier(EffectTarget.PlayerUnit, StatType.SpawnCost, cardData);
        _foodConsumption = Mathf.FloorToInt(foodConsumption * (1f + costModifierBonus));
        
        if (poolType != PoolType.None) unitTextureHandler = UnitRenderManager.GetUnitTextureHandlerByPoolType(poolType);
        if(unitTextureHandler) unitIcon.texture = unitTextureHandler.UnitRT;
        //_cooldown = cooldown;
        playerUnitType = poolType; // 소환할 유닛 타입을 직접 받음

        // 플레이어 유닛 쿨타임 증감 값
        float cooldownModifierBonus = Modifiercalculator.GetMultiplier(EffectTarget.PlayerUnit, StatType.SpawnCooldown, cardData);

        float totalReductionPercent = PlayerDataManager.Instance.TotalUnitCooldownReduction;

        //최종 쿨타임을 계산합니다. (기본 쿨타임 * (1 - 할인율))
        float finalCooldown = cooldown * (cooldownModifierBonus + (1.0f - totalReductionPercent / 100.0f));

        //계산된 최종 쿨타임을 이 슬롯의 쿨타임(_cooldown)으로 설정
        _cooldown = finalCooldown;

        if (unitId == -1) // 빈 슬롯 처리
        {
            spawnUnitBtn.enabled = false;
            unitIconTimer.fillAmount = 1f;
            costText.text = "";
            outlineGOForCanSpawnLegendary.SetActive(true);
            unitIcon.gameObject.SetActive(false);
            enabled = false;
            return;
        }
        
        costText.text = _foodConsumption.ToString();

        SetTimerIconActive(false);
        CheckCanSpawnUnitByCost(); 
        enabled = true;
    }

    void OnSpawnUnit()
    {
        if (GameManager.Instance.PlayerHQ == null) return;
        if(isCooldown) return; // 쿨타임 중이면 리턴
        if (PlayerDataManager.Instance.CurrentFood < _foodConsumption) return;

        if(outlineGOForCanSpawnLegendary.activeSelf) outlineGOForCanSpawnLegendary.SetActive(false);

        // 이거 같은 경우는 반응이 느려질거 같아요. => 컴파일 경고 안뜨게 했습니다.
        PlayerDataManager.Instance.AddResource(ResourceType.Food, -_foodConsumption).Forget();
        SetTimerIconActive(true);
        CheckCanSpawnUnitByCost();
        GameManager.Instance.PlayerHQ.SpawnUnit(playerUnitType, this);
    }
    void CheckCanSpawnUnitByCost()
    {
        // 소환가능하면
        if(PlayerDataManager.Instance.CurrentFood >= _foodConsumption)
        {
            SetCanSpawnUnitIcon(true);
        }
        else
        {
            SetCanSpawnUnitIcon(false);
        }
    }
    void SetCanSpawnUnitIcon(bool canSpawn)
    {
        if (canSpawn == canSpawnUnit) return; // 상태 변화 없으면 리턴
        unitTextureHandler?.SetCanSpawnUnit(canSpawn);
        // 타이머와 아이콘 색상 동기화 코드
        unitIcon.color = canSpawn && !isCooldown ? whiteCol : grayCol;
        // 원래 방식: 스폰여부에 따라 아이콘 색상 변경
        //unitIcon.color = canSpawn ? whiteCol : grayCol;
        canSpawnUnit = canSpawn;
        spawnUnitBtn.enabled = canSpawn;
    }
    void SetTimerIconActive(bool active)
    {
        _cooldownTimer = 0f;
        isCooldown = active;
        //enabled = active;
        unitIconTimer.gameObject.SetActive(active);
        unitIconTimer.fillAmount = active ? 1f : 0f;
        // 타이머와 아이콘 색상 동기화 코드
        unitIcon.color = canSpawnUnit && !isCooldown ? whiteCol : grayCol;
    }

    public void SetOutLineForSpawnLegendaryUnit()
    {
        outlineGOForCanSpawnLegendary.SetActive(true);
    }
    //public void InitSpawnUnitSlot(Sprite sprite, int cardIdx, float cooldown, int foodConsumption)
    //{
    //    _foodConsumption = foodConsumption;
    //    unitIcon.sprite = sprite;
    //    text.text = cardIdx.ToString();
    //    _cooldown = cooldown;
    //    playerUnitType = (PoolType)cardIdx; // 테스트용
    //    if (cardIdx == -1) // 빈 슬롯은 클릭 안되도록
    //    {
    //        text.enabled = false;
    //        spawnUnitBtn.enabled = false;
    //        unitIconTimer.fillAmount = 1f;
    //        return;
    //    }
    //    SetTimerIconActive(false);
    //}
    //void OnSpawnUnit()
    //{
    //    if (GameManager.Instance.PlayerHQ == null) return; // 플레이어 HQ 죽었다면 작동 안하게 하기
    //    if (PlayerDataManager.Instance.CurrentFood < _foodConsumption) return;

    //    PlayerDataManager.Instance.AddResource(ResourceType.Food, -_foodConsumption);

    //    SetTimerIconActive(true);
    //    // 여기서 유닛 소환, 테스트 용으로 이렇게 형변환
    //    if((int)playerUnitType != -1) GameManager.Instance.PlayerHQ.SpawnUnit(playerUnitType);
    //}
}
// 슬롯 클릭 이벤트
struct SpawnUnitSlotStartHoldEvent
{
    public BaseUnitData unitData;
    public SpawnUnitSlotStartHoldEvent(BaseUnitData unitData)
    {
        this.unitData = unitData;
    }
}
struct SpawnUnitSlotReleaseHoldEvent
{

}

