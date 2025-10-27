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

    ActiveArtifactLevelData currentLevelData; // 현재 레벨 데이터 저장
    ActiveSkillEffect skillEffectInstance; // 스킬 효과 객체
    Player player;
    ArtifactData afData;
    float cooldown = -1f;
    float cooldownTimer = -1f;
    bool isCooldown = false;
    float manaCost = -1f;
    private void Awake()
    {
        slotIcon.fillAmount = 1f;
        slotBtn.onClick.AddListener(OnUseActiveAF);
    }
    private void Start()
    {
        player = GameManager.Instance.Player;
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
    private ActiveSkillEffect CreateSkillEffectInstance(string skillTypeString)
    {
        switch (skillTypeString)
        {
            case "광역 공격 / 디버프":
                return new Skill_IceSpiritBreath();
            case "광역 공격":
                return new Skill_ThunderJudgment();
            case "아군 버프":
                return new Skill_KingMarch();
            case "회복":
                return new Skill_GoddessBlessing();
            case "소환":
                return new Skill_GiantCoffin();
            default:
                Debug.LogError($"알 수 없는 스킬 타입 문자열({skillTypeString})입니다.");
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

        //  7. 실제 스킬 효과 실행
        skillEffectInstance.Execute(currentLevelData);

        Debug.Log($"{afData.name} 사용, 남은 마나 {player.CurMana}");
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
        }
        else
        {
            afNameText.text = "빈 슬롯";
            slotIcon.sprite = null;
            costText.text = "";
            slotBtn.enabled = false;
            cooldownIcon.gameObject.SetActive(false); // 쿨타임 아이콘 끄기
            enabled = false; // Update 비활성화
        }
    }
    void SetSlotByType(ArtifactData data)
    {
        afNameText.text = data.name;
        slotIcon.sprite = Resources.Load<Sprite>(data.iconSpritePath);

        switch (data.artifactType)
        {
            case ArtifactType.Active:
                ActiveArtifactData acAfData = data as ActiveArtifactData;
                currentLevelData = acAfData.levelData[acAfData.curLevel]; // 현재 레벨 데이터 저장

                costText.text = $"* {acAfData.cost}";
                cooldown = currentLevelData.coolTime;
                manaCost = acAfData.cost; 

                // 스킬 효과 객체 생성 및 저장
                skillEffectInstance = CreateSkillEffectInstance(acAfData.type);

                SetTimerIconActive(false); // 쿨타임 UI 초기화
                enabled = true; // Update 함수 활성화 (쿨타임 감시)
                slotBtn.enabled = true; // 버튼 활성화
                break;

            case ArtifactType.Passive:
                // 패시브 슬롯 처리
                currentLevelData = null;
                skillEffectInstance = null;
                costText.text = "";
                cooldownIcon.gameObject.SetActive(false); // 패시브는 쿨타임 아이콘 필요 없음
                slotBtn.enabled = false; // 패시브는 클릭 불가
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

    }
}
