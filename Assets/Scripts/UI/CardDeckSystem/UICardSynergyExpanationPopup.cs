using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UICardSynergyExpanationPopup : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI explanationText;

    bool _isFade = false;
    CanvasGroup _canvasGroup;
    StringBuilder sb = new StringBuilder();
    IEventSubscriber<UISynergyIconPressedEvent> uiSynergyIconPressedEventSub;
    SynergyGrade[] synergyGrades = (SynergyGrade[])Enum.GetValues(typeof(SynergyGrade));
    Dictionary<SynergyGrade, string> colorBySynergyGrade = new Dictionary<SynergyGrade, string>()
    {
        { SynergyGrade.Bronze, "<color=#754A2D>" },
        { SynergyGrade.Gold, "<color=#E4FF00>" },
        { SynergyGrade.Prism, "<color=#C4F7F7>" }
    };
    public void Init()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        uiSynergyIconPressedEventSub = EventManager.GetSubscriber<UISynergyIconPressedEvent>();
        uiSynergyIconPressedEventSub?.Subscribe(SetActivePopup);
    }
    private void OnDestroy()
    {
        uiSynergyIconPressedEventSub?.Unsubscribe(SetActivePopup);
    }
    void SetActivePopup(UISynergyIconPressedEvent uISynergyIconPressedEvent)
    {
        if(uISynergyIconPressedEvent.isPressed)
        {
            if (gameObject.activeSelf) return;
            SetData(uISynergyIconPressedEvent.synergyType);
            OpenUI(0.05f);
        }
        else
        {
            if (!gameObject.activeSelf) return;
            CloseUI(0.05f);
        }
    }
    void SetData(UnitSynergyType synergyType)
    {
        SynergyData synergyData = DataManager.SynergyEffectData.GetData((int)synergyType * 1000);
        titleText.text = $"{synergyData.synergyTypeText} 시너지";
        sb.Clear();
        for(int i = 0; i < synergyGrades.Length; i++)
        {
            synergyData = DataManager.SynergyEffectData.GetData((int)synergyType * 1000 + (int)synergyGrades[i]);
            if(synergyData == null) continue;

            sb.AppendLine($"{colorBySynergyGrade[synergyGrades[i]]}{synergyData.synergyGradeText}({synergyData.requiredUnitCount})</color>");
            if(i == synergyGrades.Length - 1)
                sb.Append($"{synergyData.effectDescription.ToString()}");
            else
            {
                sb.AppendLine($"{synergyData.effectDescription.ToString()}");
                sb.AppendLine();
            }
        }
        explanationText.text = sb.ToString();
    }
    void OpenUI(float fadeTime)
    {
        if (_isFade) return;
        gameObject.SetActive(true);
        _isFade = true;
        FadeManager.FadeInUI(_canvasGroup, SetFadeFalse, true, fadeTime);
    }
    void CloseUI(float fadeTime)
    {
        //if (_isFade) return;
        _isFade = true;
        FadeManager.FadeOutUI(_canvasGroup, () => { gameObject.SetActive(false); SetFadeFalse(); }, true, fadeTime);
    }
    protected void SetFadeFalse()
    {
        _isFade = false;
    }
}
