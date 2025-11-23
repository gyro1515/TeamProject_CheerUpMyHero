using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class UISynergyExplanationForGuide : BasePopUpUI
{
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI explanationText;

    StringBuilder sb = new StringBuilder();
    SynergyGrade[] synergyGrades = (SynergyGrade[])Enum.GetValues(typeof(SynergyGrade));
    Dictionary<SynergyGrade, string> colorBySynergyGrade = new Dictionary<SynergyGrade, string>()
    {
        { SynergyGrade.Bronze, "<color=#754A2D>" },
        { SynergyGrade.Gold, "<color=#E4FF00>" },
        { SynergyGrade.Prism, "<color=#C4F7F7>" }
    };
    protected override float fadeDuration => 0.05f;
    IEventSubscriber<UISynergyExplanationEvent> uiSynergyExplanationEventSub;
    IEventSubscriber<UISynergyIconPressedEvent> uiSynergyIconPressedEventSub;

    public void Init()
    {
        uiSynergyExplanationEventSub = EventManager.GetSubscriber<UISynergyExplanationEvent>();
        uiSynergyIconPressedEventSub = EventManager.GetSubscriber<UISynergyIconPressedEvent>();

    }
    public void SetEvent(bool isActive)
    {
        if (isActive)
        {
            uiSynergyExplanationEventSub?.Subscribe(SetSynergyExplanationPopup);
            uiSynergyIconPressedEventSub?.Subscribe(SetSynergyExplanationPopup);
        }
        else
        {
            uiSynergyExplanationEventSub?.Unsubscribe(SetSynergyExplanationPopup);
            uiSynergyIconPressedEventSub?.Unsubscribe(SetSynergyExplanationPopup);
        }
    }
    void SetSynergyExplanationPopup(UISynergyExplanationEvent uISynergyExplanationEvent)
    {
        if (uISynergyExplanationEvent.isActive)
        {
            SetData(uISynergyExplanationEvent.synergyType);
            OpenUI();
        }
        else
        {
            CloseUI();
        }
    }
    void SetSynergyExplanationPopup(UISynergyIconPressedEvent uiSynergyIconPressedEvent)
    {
        if (uiSynergyIconPressedEvent.isPressed)
        {
            SetData(uiSynergyIconPressedEvent.synergyType);
            OpenUI();
        }
        else
        {
            CloseUI();
        }
    }
    void SetData(UnitSynergyType synergyType)
    {
        SynergyData synergyData = DataManager.SynergyEffectData.GetData((int)synergyType * 1000);
        titleText.text = $"{synergyData.synergyTypeText} 시너지";
        sb.Clear();
        for (int i = 0; i < synergyGrades.Length; i++)
        {
            synergyData = DataManager.SynergyEffectData.GetData((int)synergyType * 1000 + (int)synergyGrades[i]);
            if (synergyData == null) continue;

            sb.AppendLine($"{colorBySynergyGrade[synergyGrades[i]]}{synergyData.synergyGradeText}({synergyData.requiredUnitCount})</color>");
            if (i == synergyGrades.Length - 1)
                sb.Append($"{synergyData.effectDescription.ToString()}");
            else
            {
                sb.AppendLine($"{synergyData.effectDescription.ToString()}");
                sb.AppendLine();
            }
        }
        explanationText.text = sb.ToString();
    }
}


public struct UISynergyExplanationEvent
{
    public bool isActive;
    public UnitSynergyType synergyType;
    public UISynergyExplanationEvent(bool isActive, UnitSynergyType synergyType)
    {
        this.isActive = isActive;
        this.synergyType = synergyType;
    }
}

