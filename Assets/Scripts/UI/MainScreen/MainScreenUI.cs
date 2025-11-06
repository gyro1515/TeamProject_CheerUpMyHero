using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainScreenUI : BaseUI, IBackButtonHandler
{
    [Header("버튼 세팅")]
    [SerializeField] private Button _adviserButton;
    //[SerializeField] private Button _battleButton;
    //[SerializeField] private Button _testButton;
    [SerializeField] private Button _deckSelectButton;
    [SerializeField] private Button _notYetButton;
    [SerializeField] private Button backButton;

    [Header("패널 (Canvas Group)")]
    [SerializeField] private CanvasGroup _battlePanelCanvasGroup;
    //[SerializeField] private CanvasGroup _testPanelCanvasGroup;

    [Header("팝업 연결")]
    [SerializeField] private DeckSelectPopup _deckSelectPopup;

    //UISelectCard uiSelectCard;

    private DeckPresetController _deckPresetController;
    private UIStageSelect _uiStageSelect;
    private UIMenu uiMenu;

    private void Awake()
    {
        if (_adviserButton == null /*|| _battleButton == null*/
            || _battlePanelCanvasGroup == null /*|| _testPanelCanvasGroup == null*/ /*|| _testButton == null*/
            || _deckSelectPopup == null || _deckSelectButton == null || _notYetButton == null)
        {
            Debug.LogError("MainSceneUI: 모든 UI 컴포넌트가 인스펙터에 연결되지 않았습니다.");
            return;
        }

        _adviserButton.onClick.AddListener(OnAdviserButtonClck);
        // 전투 버튼을 클릭했을 때 OnBattleButtonClick 메서드를 실행하도록 연결합니다.
        //_battleButton.onClick.AddListener(OnBattleButtonClick);
        //_testButton.onClick.AddListener(OnTestButtonClick);
        _deckSelectButton.onClick.AddListener(OnDeckSelectButtonClick);
        _notYetButton.onClick.AddListener(OnNotYetButtonClick);
        backButton.onClick.AddListener(OnBackBtnClicked);
        if (GameManager.IsTutorialCompleted == false)
        {
            backButton.gameObject.SetActive(false);
        }

        // OnEnable()로 이동, 열릴때마다 팝업 닫아주기
        /*_battlePanel.SetActive(false);
        _testPanel.SetActive(false);
        _deckSelectPanel.SetActive(false);*/
        //uiSelectCard = UIManager.Instance.GetUI<UISelectCard>();

        
    }
    private void Start()
    {
        if (!GameManager.IsTutorialCompleted)
        {
            UIManager.Instance.GetUI<UITutorialMain>();
        }
        _uiStageSelect = UIManager.Instance.GetUI<UIStageSelect>();
        _deckPresetController = UIManager.Instance.GetUI<DeckPresetController>();
        uiMenu = UIManager.Instance.GetUI<UIMenu>();
    }
    private void OnEnable()
    {
        ClosePanel(_battlePanelCanvasGroup, true);
        UIManager.PubishAddUIStackEvent(this);
        //ClosePanel(_testPanelCanvasGroup, true);
       /* if (_deckSelectPopup != null)
        {
            // _deckSelectPopup이 보통 Awake()되기 전에 OnEnable()이 호출되므로
            _deckSelectPopup.CloseUI(); 
        }*/
    }
    private void OnDisable()
    {
        UIManager.PublishRemoveUIStackEvent();
    }
    private void OnAdviserButtonClck()
    {
        //OpenPanel(_battlePanelCanvasGroup);
        OnDeckSelectButtonClick();
        //OpenPanel(_testPanelCanvasGroup);
    }

    //private void OnBattleButtonClick()
    //{
    //    Debug.Log("덱 선택 패널을 엽니다.");
    //    OpenPanel(_deckSelectPanelCanvasGroup);
    //    ClosePanel(_battlePanelCanvasGroup);
    //    //ClosePanel(_testPanelCanvasGroup);
    //}

    private void OnDeckSelectButtonClick()
    {
        // "덱 선택" 버튼 클릭 시 실행될 로직
        //Debug.Log("덱을 선택하고 다음 단계로 넘어갑니다.");

        if (!GameManager.IsTutorialCompleted)
        {
            GameManager.IsStageAndDestinySelected = true;
            PlayerDataManager.Instance.SelectedStageIdx = (0, 1);
        }
        // 스테이지 선택을 이미 했다면 덱 선택으로
        if (GameManager.IsStageAndDestinySelected)
        {
            UIManager.Instance.fromUI = FromUI.MainScreen;
            FadeManager.Instance.SwitchGameObjects(gameObject, _deckPresetController.gameObject);
        }
        else // 스테이지 선택을 안했다면 스테이지 선택으로
        {
            Debug.Log("스테이지 선택으로");
            //  FadeManager.Instance.SwitchGameObjects(gameObject, uiSelectCard.gameObject);
            if (_uiStageSelect != null)
            {
                UIManager.Instance.fromUI = FromUI.MainScreen;
                FadeManager.Instance.SwitchGameObjects(gameObject, _uiStageSelect.gameObject);
                //FadeManager.Instance.SwitchGameObjects(gameObject, _deckPresetController.gameObject);
            }
            else
            {
                Debug.LogError("UIManager에서 _uiStageSelect 찾을 수 없습니다!");
            }
            //if (_deckSelectPopup != null)
            //{
            //    _deckSelectPopup.CloseUI();
            //}
        }
            
            
    }

    private void OnNotYetButtonClick()
    {
        // "아직 아니야" 버튼 클릭 시 실행될 로직
        Debug.Log("덱 선택을 취소하고 패널을 닫습니다.");
        if (_deckSelectPopup != null)
        {
            _deckSelectPopup.CloseUI();
        }
    }

    //private void OnTestButtonClick()
    //{
    //    Debug.Log("테스트 버튼입니다.");
    //}


    private void OpenPanel(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null) return;
        FadeManager.FadeInUI(canvasGroup);
    }

    private void ClosePanel(CanvasGroup canvasGroup, bool immediate = false)
    {
        if (canvasGroup == null) return;

        if (immediate)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            FadeManager.FadeOutUI(canvasGroup);
        }
    }
    private void OnBackBtnClicked()
    {
        if(GameManager.IsTutorialCompleted == false)
        {
            return;
        }
        FadeManager.Instance.SwitchGameObjects(this.gameObject, uiMenu.gameObject);
    }

    public void OnBackPressed()
    {
        Debug.Log("[MainScreenUI] 뒤로 가기 버튼 눌림");
        OnBackBtnClicked();
    }
}

