using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDescriptionArtifactPanel : MonoBehaviour
{
    [Header("유물 설명 패널 세팅")]
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI cooldownText;
    [SerializeField] TextMeshProUGUI descriptionText;
    ActiveAfData activeAfData;

    private void Awake()
    {
        SetActivePanel(false);
    }
    public void SetDescriptionPanel(ActiveAfData data)
    {
        activeAfData = data;
        if(activeAfData != null)
        {
            SetActivePanel(true);
            icon.sprite = activeAfData.icon;
            nameText.text = activeAfData.name;
            cooldownText.text = $"{activeAfData.cooldown}s";
            descriptionText.text = 
                $"레벨: {activeAfData.lv}\n유형: {activeAfData.type}\n코스트: {activeAfData.cost}\n{activeAfData.description}";
        }
        else SetActivePanel(false);
    }
    void SetActivePanel(bool active)
    {
        icon.enabled = active;
        nameText.enabled = active;
        cooldownText.enabled = active;
        descriptionText.enabled = active;
    }

}
