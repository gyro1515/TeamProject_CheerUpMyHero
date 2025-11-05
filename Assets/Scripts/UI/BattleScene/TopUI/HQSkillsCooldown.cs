using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HQSkillsCooldown : MonoBehaviour
{
    [SerializeField] Image timerImage;
    [SerializeField] Image skillIconImg;

    float skillCoolTime;
    private float cooldownTimer;
    private bool isCooldown;

    BaseHQSkill baseHQSkill;
    PlayerHQSkill playerHQSkill;
    PoolType skillPoolType;
    public void InitHQSkillCooldowm(BaseHQSkill hqSkill, PlayerHQSkill playerHQSkill, PoolType type)
    {
        baseHQSkill = hqSkill;
        this.playerHQSkill = playerHQSkill;
        skillPoolType = type;
        skillIconImg.sprite = baseHQSkill.SkillIconRenderer.sprite;
        skillIconImg.color = baseHQSkill.SkillIconRenderer.color;
        skillCoolTime = baseHQSkill.CoolTime;
        timerImage.fillAmount = 0;
    }
    
    private void Update()
    {
        if (!isCooldown) return; // 쿨타임이 아니면 리턴
        cooldownTimer += Time.deltaTime;
        timerImage.fillAmount = 1 - cooldownTimer / skillCoolTime;
        if (cooldownTimer < skillCoolTime) return; // 아직 쿨타임이 다 안돌았다면 리턴
        isCooldown = false;

        if (playerHQSkill != null) 
            playerHQSkill.IsCoolTime[skillPoolType] = false; // 쿨타임 끝났으니 스킬도 사용 가능하게 변경
    }


    public void ShowSkillCooldown()
    {
        isCooldown = true;
        timerImage.fillAmount = 1f;
        cooldownTimer = 0;
    }
}
