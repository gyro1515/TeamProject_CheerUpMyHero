using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIActiveAFSlot : MonoBehaviour
{
    [Header("액티브 유물 슬롯 세팅")]
    [SerializeField] TextMeshProUGUI afNameText;
    [SerializeField] Image slotIcon;
    [SerializeField] Image cooldownIcon;
    //[SerializeField] TextMeshProUGUI cooldownText;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] Button slotBtn;
    [SerializeField] UIAdvancedButton slotAdvancedBtn;

    ActiveArtifactLevelData currentLevelData; // 현재 레벨 데이터 저장
    ActiveSkillEffect skillEffectInstance; // 스킬 효과 객체
    Player player;
    ArtifactData afData;
    float cooldown = -1f;
    float cooldownTimer = -1f;
    bool isCooldown = false;
    float manaCost = -1f;

    IEventPublisher<AfSlotStartHoldEvent> afSlotStartHoldEventPub;
    IEventPublisher<AfSlotReleaseHoldEvent> afSlotReleaseHoldEventPub;
    private void Awake()
    {
        
        slotIcon.fillAmount = 1f;
        //slotBtn.onClick.AddListener(OnUseActiveAF);
    }
    private void Start()
    {
        player = GameManager.Instance.Player;
        player.OnCurManaChanged += (curMana, maxMana) =>
        {
            // 마나가 부족할 때 슬롯 반투명 처리
            if (afData == null) return;
            if (afData.artifactType != ArtifactType.Active) return;
            ChekMana(curMana);
            //Debug.Log($"액티브 유물 슬롯 마나 체크 {curMana}");
        };
        //SetTimerIconActive(false);
        enabled = false;
        
    }
    private void Update()
    {
        if (!isCooldown) return; // 쿨타임이 아니면 리턴
        cooldownTimer += Time.deltaTime;
        cooldownIcon.fillAmount = 1 - cooldownTimer / cooldown;
        if (cooldownTimer < cooldown) return; // 아직 쿨타임이 다 안돌았다면 리턴
        cooldownIcon.fillAmount = 1f;
        SetTimerIconActive(false);
    }
    void ChekMana(float curMana)
    {
        if (curMana < manaCost)
        {
            slotIcon.color = new Color(100f / 255f, 100f / 255f, 100f / 255f, 1.0f);
        }
        else
        {
            slotIcon.color = Color.white;
        }
    }

    private ActiveSkillEffect CreateSkillEffectInstance(int idnumber)
    {
        switch (idnumber)
        {
            case 08010001:
                return new Skill_IceSpiritBreath();
            case 08010002:
                return new Skill_ThunderJudgment();
            case 08010003:
                return new Skill_KingMarch();
            case 08010004:
                return new Skill_GoddessBlessing();
            case 08010005:
                return new Skill_GiantCoffin();
            default:
                Debug.LogError($"알 수 없는 스킬 타입 문자열({idnumber})입니다.");
                return null;
        }
    }
    void OnUseActiveAF()
    {
        // 1. 데이터 및 참조 확인
        if (skillEffectInstance == null) { Debug.LogWarning("스킬 효과가 없습니다."); return; }
        if (player == null) { Debug.LogWarning("플레이어 정보 없음"); return; }
        //  2. 쿨타임 확인 (UI 슬롯이 직접 관리)
        if (isCooldown) { Debug.Log("쿨타임 중입니다."); return; }
        //  3. 마나 확인 (UI 슬롯이 직접 관리)
        if (player.CurMana < manaCost) { Debug.Log("마나 부족"); return; }

        // --- 모든 조건 통과 ---

        // 4. 마나 차감
        player.CurMana -= manaCost;

        // 5. 쿨타임 UI 시작
        SetTimerIconActive(true);

        // 6. 플레이어 애니메이션 재생
        player.PlayerController.TestForUseActiveArtifact();

        // 7. 유물 사운드 실행
        switch (afData.idNumber)
        {
            case 08010001:
                AudioManager.PlayOneShot(DataManager.AudioData.AF_IceBreath);
                break;
            case 08010002:
                AudioManager.PlayOneShot(DataManager.AudioData.AF_ThunderGod);
                break;
            case 08010003:
                AudioManager.PlayOneShot(DataManager.AudioData.AF_KingdomMarch);
                break;
            case 08010004:
                AudioManager.PlayOneShot(DataManager.AudioData.AF_goddess);
                break;
            case 08010005:
                AudioManager.PlayOneShot(DataManager.AudioData.AF_golem);
                break;
        }

        //  8. 실제 스킬 효과 실행
        skillEffectInstance.Execute(currentLevelData);

        //Debug.Log($"{afData.name} 사용, 남은 마나 {player.CurMana}");
    }

    public void InitAfSlot(ArtifactData data)
    {
        afData = data;
        if (data != null)
        {
            SetSlotByType(data);
            /*afNameText.text = data.name;
            slotIcon.sprite = data.icon;
            //cooldownText.text = $"{data.cooldown}s";
            costText.text = $"* {data.cost}";
            // ToDo 쿨타임/마나 코스트은 액티브 유물인 경우만 세팅
            cooldown = data.cooldown;
            manaCost = data.cost;
            // ToDo 패시브 유물은 버튼 비활성화*/
            afSlotStartHoldEventPub = EventManager.GetPublisher<AfSlotStartHoldEvent>();
            afSlotReleaseHoldEventPub = EventManager.GetPublisher<AfSlotReleaseHoldEvent>();
            slotAdvancedBtn.onShortClick += OnUseActiveAF;
            slotAdvancedBtn.onHoldStart += () =>
            {
                afSlotStartHoldEventPub?.Publish(new AfSlotStartHoldEvent(afData));
            };
            slotAdvancedBtn.onHoldRelease += () =>
            {
                afSlotReleaseHoldEventPub?.Publish();
            };
        }
        else
        {
            //afNameText.text = "빈 슬롯";
            //slotIcon.sprite = null;
            costText.text = "";
            slotBtn.enabled = false;
            //slotAdvancedBtn.Interactable = false;
            cooldownIcon.gameObject.SetActive(false); // 쿨타임 아이콘 끄기
            enabled = false; // Update 비활성화
        }
    }
    void SetSlotByType(ArtifactData data)
    {
        afNameText.text = "";
        slotIcon.sprite = Resources.Load<Sprite>(data.iconSpritePath);

        switch (data.artifactType)
        {
            case ArtifactType.Active:
                ActiveArtifactData acAfData = data as ActiveArtifactData;
                currentLevelData = acAfData.levelData[acAfData.curLevel]; // 현재 레벨 데이터 저장

                costText.text = $"{acAfData.cost}";
                cooldown = currentLevelData.coolTime;
                manaCost = acAfData.cost;

                // 스킬 효과 객체 생성 및 저장
                skillEffectInstance = CreateSkillEffectInstance(acAfData.idNumber);

                SetTimerIconActive(false); // 쿨타임 UI 초기화
                enabled = true; // Update 함수 활성화 (쿨타임 감시)
                slotBtn.enabled = true; // 버튼 활성화
                //slotAdvancedBtn.Interactable = true;

                SetTimerIconActive(false);
                enabled = true;

                break;

            case ArtifactType.Passive:
                // 패시브 슬롯 처리
                currentLevelData = null;
                skillEffectInstance = null;
                costText.text = "";
                cooldownIcon.gameObject.SetActive(true); // 패시브는 쿨타임 아이콘 필요 없음
                cooldownIcon.fillAmount = 1;
                slotBtn.enabled = false; // 패시브는 클릭 불가
                //slotAdvancedBtn.Interactable = false;

                enabled = false; // Update 필요 없음
                break;
        }
    }
    void SetTimerIconActive(bool active)
    {
        cooldownTimer = 0f;
        isCooldown = active;
        enabled = active;
        cooldownIcon.gameObject.SetActive(active);
        cooldownIcon.fillAmount = active ? 1f : 0f;
        if (afData != null) slotBtn.enabled = !active;
        else slotBtn.enabled = false;
        /*if (afData != null) slotAdvancedBtn.Interactable = !active;
        else slotAdvancedBtn.Interactable = false;*/
    }
}
// 슬롯 클릭 이벤트
struct AfSlotStartHoldEvent
{
    public ArtifactData artifactData;
    public AfSlotStartHoldEvent(ArtifactData data)
    {
        artifactData = data;
    }
}
struct AfSlotReleaseHoldEvent {}
