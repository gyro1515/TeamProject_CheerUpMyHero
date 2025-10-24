using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LaterUpdatePopup : BasePopUpUI 
{
    [Header("UI 참조")]
    [SerializeField] private TextMeshProUGUI messageText; 
    [SerializeField] private Button okButton;      

    protected override void Awake()
    {
        base.Awake(); 

        okButton?.onClick.AddListener(OnOkButtonClicked);
    }

    public void Show(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }

        OpenUI();
    }
    private void OnOkButtonClicked()
    {
        CloseUI();
    }

    public override void OnBackPressed()
    {
        if (_canvasGroup.interactable && !_isFade)
        {
            OnOkButtonClicked();
        }
    }
}