using UnityEngine;
using UnityEngine.UI;


public class DiplomacyPanel : BasePopUpUI 
{
    [Header("버튼 참조")]
    [SerializeField] private Button embassyButton;  
    [SerializeField] private Button merchantGuildButton; 
    [SerializeField] private Button royalMarketButton;  
    [SerializeField] private Button adventurerGuildButton; 
    [SerializeField] private Button backButton;

    [Header("팝업 참조")]
    [SerializeField] private LaterUpdatePopup laterUpdatePopup; 

    protected override void Awake()
    {
        base.Awake(); 

        embassyButton?.onClick.AddListener(OnEmbassyClicked);
        merchantGuildButton?.onClick.AddListener(OnMerchantGuildClicked);
        royalMarketButton?.onClick.AddListener(OnRoyalMarketClicked);
        adventurerGuildButton?.onClick.AddListener(OnAdventurerGuildClicked);
        backButton?.onClick.AddListener(OnBackButtonClicked);
    }


    private void OnEmbassyClicked()
    {
        LaterUpdatePopup();
    }

    private void OnMerchantGuildClicked()
    {
        LaterUpdatePopup(); 
    }

    private void OnRoyalMarketClicked()
    {
        LaterUpdatePopup(); 
    }

    private void OnAdventurerGuildClicked()
    {
        //가챠 UI 연결
        CloseUI(); 
    }

    private void OnBackButtonClicked()
    {
        CloseUI();
    }

    // --- 헬퍼 함수 ---
    private void LaterUpdatePopup()
    {
        if (laterUpdatePopup != null)
        {
            laterUpdatePopup.Show("추후 업데이트 될 내용입니다.");
        }
        else
        {
            Debug.Log("알림: 추후 업데이트 될 내용입니다."); 
        }
    }

    public override void OnBackPressed()
    {
        if (_canvasGroup.interactable && !_isFade)
        {
            OnBackButtonClicked(); 
        }
    }
}