using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMenu : BaseUI
{
    [SerializeField] private LoginConfirmPopup laterUpdatePopup;
    [Header("탑버튼")]
    [SerializeField] private Button PostBtn;
    [SerializeField] private Button NoticeBtn;
    [SerializeField] private Button GuideBtn;
    [Header("바텀버튼")]
    [SerializeField] private Button EnforceBtn;
    [SerializeField] private Button StoreBtn;
    [SerializeField] private Button GachaBtn;
    [SerializeField] private Button AlliesBtn;
    [SerializeField] private Button BattleBtn;
    [SerializeField] private Button WisdomBtn;

    MainScreenUI mainScreenUI;
    UIStageSelect uiStageSelect;
    GachaUIPanel gachaUIPanel;
    UIGuide UIGuide;
    private void Awake()
    {
        PostBtn.onClick.AddListener(OnLateUpdateClicked);
        NoticeBtn.onClick.AddListener(OnLateUpdateClicked);
        GuideBtn.onClick.AddListener(OnGuidBtnClicked);
        EnforceBtn.onClick.AddListener(OnLateUpdateClicked);
        StoreBtn.onClick.AddListener(OnLateUpdateClicked);
        GachaBtn.onClick.AddListener(OnGachaBtnClicked);
        AlliesBtn.onClick.AddListener(OnLateUpdateClicked);
        BattleBtn.onClick.AddListener(OnBattleBtnClicked);
        WisdomBtn.onClick.AddListener(OnWisdomBtnClicked);

    }
    void Start()
    {
        mainScreenUI = UIManager.Instance.GetUI<MainScreenUI>();
        uiStageSelect = UIManager.Instance.GetUI<UIStageSelect>();
        gachaUIPanel = UIManager.Instance.GetUI<GachaUIPanel>();
        UIGuide = UIManager.Instance.GetUI<UIGuide>();
    }

    void Update()
    {
        
    }
    private void OnWisdomBtnClicked()
    {
        FadeManager.Instance.SwitchGameObjects(this.gameObject, mainScreenUI.gameObject);
    }
    private void OnBattleBtnClicked()
    {
        FadeManager.Instance.SwitchGameObjects(this.gameObject, uiStageSelect.gameObject);

    }
    private void OnGachaBtnClicked()
    {
        FadeManager.Instance.SwitchGameObjects(this.gameObject, gachaUIPanel.gameObject);
    }
    private void OnGuidBtnClicked()
    {
        FadeManager.Instance.SwitchGameObjects(this.gameObject, UIGuide.gameObject);
    }
    private void OnLateUpdateClicked()
    {
        laterUpdatePopup.Show();
    }
}
