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

    protected override void Awake()
    {
        base.Awake();
        GetComponent<Image>().color = new Color(1, 1, 1, 0);
        GetComponent<Button>().onClick.AddListener(CloseParentPopup);
    }
    public void OnSynergyClicked(UnitSynergyType synergyType, int currentCount)
    //public void OnSynergyClicked(UnitSynergyType synergyType, SynergyGrade currentCount)
    {
        // TODO: 시너지 효과 설명 데이터 테이블 만들어서 불러오기

        // 아래는 테스트
        string effectText = "";
        switch (currentCount)
        {
            case 0:
                effectText = "브론즈";
                break;
            case 1:
                effectText = "골드";
                break;
            case 2:
                effectText = "프리즘";
                break;

        }
        synergyToolTipText.text = $"{synergyType} {effectText} 시너지 효과";
        synergyToolTipDescriptionText.text = DataManager.SynergyEffectData.GetData((int)synergyType * 1000 + currentCount).effectDescription;
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
