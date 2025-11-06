using System.Collections;
using System.Collections.Generic;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
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
    DeckPresetController deckPresetController;

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
        deckPresetController = UIManager.Instance.GetUI<DeckPresetController>();
    }

    void Update()
    {
        
    }
    private void OnWisdomBtnClicked()
    {
        UIManager.Instance.fromUI = FromUI.UIMenu;
        FadeManager.Instance.SwitchGameObjects(this.gameObject, mainScreenUI.gameObject);
    }
    private void OnBattleBtnClicked()
    {
        if (GameManager.IsStageAndDestinySelected)
        {
            // 스테이지를 '이미' 선택했다면 -> '덱 편성'으로 바로 이동
            Debug.Log("[UIMenu] 이미 선택한 스테이지가 있으므로 덱 편성으로 이동합니다.");
            if (deckPresetController != null)
            {
                UIManager.Instance.fromUI = FromUI.UIMenu;
                FadeManager.Instance.SwitchGameObjects(this.gameObject, deckPresetController.gameObject);
            }
        }
        else
        {
            //스테이지를 '아직' 선택 안 했다면 -> '스테이지 선택'으로 이동
            Debug.Log("[UIMenu] 선택한 스테이지가 없으므로 스테이지 선택으로 이동합니다.");
            if (uiStageSelect != null)
            {
                UIManager.Instance.fromUI = FromUI.UIMenu;
                FadeManager.Instance.SwitchGameObjects(this.gameObject, uiStageSelect.gameObject);
            }
        }
    }
    private void OnGachaBtnClicked()
    {
        //가챠 들어가면 자고 있는 서버 깨우기. 순서 상관없어서 백엔드매니저 없이 바로 호출
        var module = new GachaModuleV2Bindings(CloudCodeService.Instance);
        _ = module.WakeUpServer(); // 대기 안하고 바로 넘어가기

        UIManager.Instance.fromUI = FromUI.UIMenu;

        FadeManager.Instance.SwitchGameObjects(this.gameObject, gachaUIPanel.gameObject);
    }
    private void OnGuidBtnClicked()
    {
        UIManager.Instance.fromUI = FromUI.UIMenu;
        FadeManager.Instance.SwitchGameObjects(this.gameObject, UIGuide.gameObject);
    }
    private void OnLateUpdateClicked()
    {
        laterUpdatePopup.Show();
    }
}
