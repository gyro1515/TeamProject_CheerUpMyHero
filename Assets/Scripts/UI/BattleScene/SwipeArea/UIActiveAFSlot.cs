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
    void OnUseActiveAF()
    {
        //Debug.Log("액티브 유물 사용 시도");
        if (afData == null) return;
        if(!player) { Debug.LogWarning("플레이어 정보 없음"); return; } // 오류
        if (player.CurMana < manaCost) { Debug.Log("마나 부족"); return; }
        player.CurMana -= manaCost;
        SetTimerIconActive(true);
        player.PlayerController.TestForUseActiveArtifact();// 테스트로 일단 공격 애니메이션 재생
        Debug.Log($"{afData.name} 사용, 남은 마나{player.CurMana}");
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
            //cooldownText.text = "";
            costText.text = "";
            slotBtn.enabled = false; // 빈 슬롯은 클릭 불가
        }
    }
    void SetSlotByType(ArtifactData data)
    {
        afNameText.text = data.name;
        slotIcon.sprite = data.icon;
        switch (data.artifactType)
        {
            case ArtifactType.Active:
                ActiveArtifactData acAfData = data as ActiveArtifactData;
                costText.text = $"* {acAfData.cost}";
                cooldown = acAfData.levelData[acAfData.curLevel].coolTime;
                manaCost = acAfData.cost;
                SetTimerIconActive(false);
                enabled = true;
                break;
            case ArtifactType.Passive:
                // 클릭 안되고 표시만 되게
                costText.text = "";
                cooldownIcon.gameObject.SetActive(true);
                cooldownIcon.fillAmount = 1f;
                slotBtn.enabled = false;
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
