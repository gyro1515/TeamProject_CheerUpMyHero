using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HQSkillsCooldown : MonoBehaviour
{
    [SerializeField] float skillCoolTime;
    [SerializeField] Image timerImage;

    private float cooldownTimer;
    private bool isCooldown;

    //테스트 only
    private void Start()
    {
        StartCoroutine(TempUseSkill());
    }

    IEnumerator TempUseSkill()
    {
        while (true)
        {
            ShowSkillCooldown();
            yield return new WaitForSeconds(skillCoolTime + 1);
        }
    }

    private void Update()
    {
        if (!isCooldown) return; // 쿨타임이 아니면 리턴
        cooldownTimer += Time.deltaTime;
        timerImage.fillAmount = 1 - cooldownTimer / skillCoolTime;
        if (cooldownTimer < skillCoolTime) return; // 아직 쿨타임이 다 안돌았다면 리턴
        isCooldown = false;
    }


    public void ShowSkillCooldown()
    {
        isCooldown = true;
        timerImage.fillAmount = 1f;
        cooldownTimer = 0;
    }
}
