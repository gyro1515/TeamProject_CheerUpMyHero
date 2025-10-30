using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class LoginConfirmPopup : BasePopUpUI 
{
    [Header("UI 참조")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button okButton;

    [Header("컨트롤러 참조")]
    [SerializeField] private StartUI startUIController; // StartUI 스크립트 참조

    protected override void Awake()
    {
        base.Awake();
        okButton?.onClick.AddListener(OnOkButtonClicked);

        if (startUIController == null)
        {
            Debug.Log("LoginConfirmPopup에 StartUI가 연결되지 않았습니다!");
        }
    }

    public void Show()
    {
        OpenUI(); // 팝업 띄우기
    }

    private void OnOkButtonClicked()
    {
        if (startUIController != null)
            startUIController.OnLoginSuccess(); 
        CloseUI(); // 그냥 바로 닫기
    }
    public override void OnBackPressed()
    {
        CloseUI();
    }
}
    