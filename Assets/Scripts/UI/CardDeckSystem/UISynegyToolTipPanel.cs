using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasRenderer))]
public class UISynegyToolTipPanel : BasePopUpUI
{
    [SerializeField] TextMeshProUGUI synergyToolTipText;
    [SerializeField] TextMeshProUGUI synergyToolTipDescriptionText;
    Dictionary<SynergyGrade, string> colorBySynergyGrade = new Dictionary<SynergyGrade, string>()
    {
        { SynergyGrade.Bronze, "<color=#754A2D>" },
        { SynergyGrade.Gold, "<color=#E4FF00>" },
        { SynergyGrade.Prism, "<color=#C4F7F7>" }
    };
    protected override void Awake()
    {
        base.Awake();
        GetComponent<Image>().color = new Color(1, 1, 1, 0);
        GetComponent<Button>().onClick.AddListener(CloseParentPopup);

    }
    public void OnSynergyClicked(UnitSynergyType synergyType, int curGrade, int currentCount)
    //public void OnSynergyClicked(UnitSynergyType synergyType, SynergyGrade currentCount)
    {
        SynergyData synergyData = DataManager.SynergyEffectData.GetData((int)synergyType * 1000 + curGrade);
        // 아래는 테스트
        
        synergyToolTipText.text = $"{synergyData.synergyTypeText} {colorBySynergyGrade[(SynergyGrade)curGrade]}{synergyData.synergyGradeText}({currentCount}/{synergyData.requiredUnitCount})</color> 시너지 효과";
        synergyToolTipDescriptionText.text = synergyData.effectDescription;
        //Debug.Log($"현재 {currentCount}마리 적용 중");
        if (gameObject.activeSelf) return;
        OpenUI(0.1f);
    }
    // 이 팝업은 페이드 타임 빨라야 할 거 같아서 오버로드
    public void OpenUI(float fadeTime)
    {
        if (_isFade) return;
        gameObject.SetActive(true);
        _isFade = true;
        FadeManager.FadeInUI(_canvasGroup, SetFadeFalse, true, fadeTime);
    }
    public void CloseUI(float fadeTime)
    {
        if (_isFade) return;
        _isFade = true;
        FadeManager.FadeOutUI(_canvasGroup, () => { gameObject.SetActive(false); SetFadeFalse(); }, true, fadeTime);
    }
    public override void OnBackPressed()
    {
        Debug.Log($"오버라이드 {gameObject.name} 뒤로가기: ");
        CloseUI(0.1f);
    }
    void CloseParentPopup()
    {
        //Debug.Log("꺼짐");
        CloseUI(0.1f);
    }
}
