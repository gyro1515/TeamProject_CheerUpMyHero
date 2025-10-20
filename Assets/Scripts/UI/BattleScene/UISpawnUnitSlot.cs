using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISpawnUnitSlot : MonoBehaviour
{
    [Header("유닛 선택 소환 아이콘 설정")]
    [SerializeField] Image unitIcon;
    [SerializeField] Image unitIconTimer; // 쿨타임 표시, 버튼 클릭 방지용
    [SerializeField] Button spawnUnitBtn;
    [SerializeField] TextMeshProUGUI text; // 추후 아이콘만 설정하면 될 듯 합니다.
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] Outline outlineForCanSpawnLegendary; // 전설 등급 소환 가능시 아웃라인 효과

    float _cooldown = -1f;
    float _cooldownTimer = -1f;
    bool isCooldown = false;
    int _foodConsumption = 0;
    PoolType playerUnitType;
    private void Awake()
    {
        costText.gameObject.SetActive(true); // 현재 왜 꺼져있는지 모르겠음
        costText.raycastTarget = false; // 텍스트가 버튼 클릭 막는 현상 방지
        outlineForCanSpawnLegendary.enabled = false;
        spawnUnitBtn.onClick.AddListener(OnSpawnUnit);
    }
    private void Update()
    {
        if (!isCooldown) return; // 쿨타임이 아니면 리턴
        _cooldownTimer += Time.deltaTime;
        unitIconTimer.fillAmount = 1 - _cooldownTimer / _cooldown;
        if (_cooldownTimer < _cooldown) return; // 아직 쿨타임이 다 안돌았다면 리턴
        unitIconTimer.fillAmount = 1f;
        SetTimerIconActive(false);
    }

    public void InitSpawnUnitSlot(Sprite sprite, string unitName, int unitId, PoolType poolType, float cooldown, int foodConsumption)
    {
        _foodConsumption = foodConsumption;
        unitIcon.sprite = sprite;
        //_cooldown = cooldown;
        playerUnitType = poolType; // 소환할 유닛 타입을 직접 받음
        
        float totalReductionPercent = PlayerDataManager.Instance.TotalUnitCooldownReduction;

        //최종 쿨타임을 계산합니다. (기본 쿨타임 * (1 - 할인율))
        float finalCooldown = cooldown * (1.0f - totalReductionPercent / 100.0f);

        //계산된 최종 쿨타임을 이 슬롯의 쿨타임(_cooldown)으로 설정
        _cooldown = finalCooldown;

        if (unitId == -1) // 빈 슬롯 처리
        {
            text.enabled = false;
            spawnUnitBtn.enabled = false;
            unitIconTimer.fillAmount = 1f;
            costText.text = "";
            return;
        }

        text.text = unitName;
        costText.text = _foodConsumption.ToString();

        SetTimerIconActive(false);
    }

    void OnSpawnUnit()
    {
        if (GameManager.Instance.PlayerHQ == null) return;
        if (PlayerDataManager.Instance.CurrentFood < _foodConsumption) return;

        if(outlineForCanSpawnLegendary.enabled) outlineForCanSpawnLegendary.enabled = false;

        PlayerDataManager.Instance.AddResource(ResourceType.Food, -_foodConsumption);
        SetTimerIconActive(true);

        GameManager.Instance.PlayerHQ.SpawnUnit(playerUnitType, this);
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
    void SetTimerIconActive(bool active)
    {
        _cooldownTimer = 0f;
        isCooldown = active;
        enabled = active;
        unitIconTimer.gameObject.SetActive(active);
        unitIconTimer.fillAmount = active ? 1f : 0f;
    }
    
    public void SetOutLineForSpawnLegendaryUnit()
    {
        outlineForCanSpawnLegendary.enabled = true;
    }
}

