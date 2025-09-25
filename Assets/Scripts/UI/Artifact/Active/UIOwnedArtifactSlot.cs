using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIOwnedArtifactSlot : MonoBehaviour
{
    [Header("소유 유물 슬롯 세팅")]
    [SerializeField] Button slotBtn;
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI cooldownText;
    [SerializeField] TextMeshProUGUI forTestNameText;
    [SerializeField] Outline outLine;
    public ActiveAfData ActiveAfData { get; private set; }
    
    public void SetSlot(ActiveAfData data, UnityAction action = null)
    {
        ActiveAfData = data;
        if (ActiveAfData == null) return;

        icon.sprite = ActiveAfData.icon;
        cooldownText.text = $"{ActiveAfData.cooldown}s";
        forTestNameText.text = ActiveAfData.name;
        float alpha;
        if (ActiveAfData.isEquipped) // 장착 여부에 따라 반투명 여부 결정
            alpha = 0.5f;
        else
            alpha = 1f;

        // 아이콘은 흰색 기반, 글은 현재 검은색 기반
        Color color = new Color(1f, 1f, 1f, alpha);
        icon.color = color;
        color = new Color(0f, 0f, 0f, alpha);
        cooldownText.color = color;
        forTestNameText.color = color;
        // 모든 리스너 제거하고 추가하기
        slotBtn.onClick.RemoveAllListeners();
        if (action != null) slotBtn.onClick.AddListener(action);
        outLine.enabled = false;
        gameObject.SetActive(true);
    }
    public void SetEquipState()
    {
        float alpha;
        if (ActiveAfData.isEquipped) // 장착 여부에 따라 반투명 여부 결정
            alpha = 0.5f;
        else
            alpha = 1f;

        // 아이콘은 흰색 기반, 글은 현재 검은색 기반
        Color color = new Color(1f, 1f, 1f, alpha);
        icon.color = color;
        color = new Color(0f, 0f, 0f, alpha);
        cooldownText.color = color;
        forTestNameText.color = color;
    }
    public void SetOutLine(bool active)
    {
        outLine.enabled = active;
    }
}
