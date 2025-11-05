using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIPause : BaseUI, IBackButtonHandler
{
    [Header("일시정지 버튼")]
    [SerializeField] private Button _pauseButton;

    [Header("환경설정 메뉴")]
    [SerializeField] private GameObject _settingPanel;
    [SerializeField] private UISettingMenu _settingMenuScript;

    private CanvasGroup _settingPanelCanvasGroup;
    BasePopUpUI _settingPanelPopUpUI;

    IEventSubscriber<TutorialSkipEvent> tutorialSkipEventSub;

    private void Awake()
    {
        tutorialSkipEventSub = EventManager.GetSubscriber<TutorialSkipEvent>();
        tutorialSkipEventSub.Subscribe(ApplyCurSpeed);
        _pauseButton.onClick.AddListener(OnPauseButtonClicked);

        _settingPanelPopUpUI = _settingPanel.GetComponent<BasePopUpUI>();
        _settingPanelCanvasGroup = _settingPanel.GetComponent<CanvasGroup>();
        _settingPanel.GetComponent<Button>().onClick.AddListener(() => 
        { 
            ApplySpeed(CurrentSpeed);
            //_settingMenuScript.showPanel(null);
        }); 
        InitSpeedBtn();
    }
    private void Start()
    {
        if (GameManager.Instance != null &&
                 GameManager.Instance.enemyHQ != null &&
                 GameManager.Instance.enemyHQ.WaveSystem != null)
        {
            Debug.Log("[UIPause] enemyHQ 및 WaveSystem 발견. 리스너를 등록합니다.");
            /*GameManager.Instance.enemyHQ.WaveSystem.OnWarningDisplayed += () =>
            {
                ApplySpeed(SpeedState.X1); // 웨이브 경고 시 배속 초기화
            };
            GameManager.Instance.enemyHQ.WaveSystem.SetOnWarningEnd(() => ApplySpeed(CurrentSpeed));*/
        }
        else
        {
            Debug.LogWarning("[UIPause] enemyHQ 또는 WaveSystem을 찾을 수 없습니다. (StartScene에서는 정상 동작입니다)");
        }
        // 배틀씬에서만 배틀 튜토리얼 호출 가능하도록
        if(SceneLoader.CurrentSceneState == SceneState.BattleScene && !GameManager.IsTutorialCompleted)
        {
            UIManager.Instance.GetUI<UITutorialBattle>();
        }
    }
    private void OnEnable()
    {
        if (SceneLoader.CurrentSceneState == SceneState.BattleScene)
        {
            UIManager.PubishAddUIStackEvent(this);
        }
    }
    private void OnDisable()
    {
        if (SceneLoader.CurrentSceneState == SceneState.BattleScene)
        {
            UIManager.PublishRemoveUIStackEvent();
        }
        tutorialSkipEventSub.Unsubscribe(ApplyCurSpeed);
    }
    private void OnPauseButtonClicked()
    {
        GameManager.IsPaused = true;
        Time.timeScale = 0.0f;
        /*_settingPanel.SetActive(true);
        _settingPanelCanvasGroup.alpha = 0.0f;
        _settingPanelCanvasGroup.DOFade(1f, 0.3f).SetUpdate(true);
        _settingPanelCanvasGroup.interactable = true;
        _settingPanelCanvasGroup.blocksRaycasts = true;*/
        /*_settingPanel.SetActive(true);
        _settingMenuScript.ShowPausePanel();*/

        _settingPanelPopUpUI.OpenUI();
    }

    [Header("속도조절 버튼")]
    [SerializeField] private TextMeshProUGUI speedText; // 배속 텍스트
    [SerializeField] private Button _speedButton;

    public enum SpeedState { X1 = 1, X2 = 2, X3 = 3 }
    public SpeedState CurrentSpeed { get; private set; } = SpeedState.X1;


    private void InitSpeedBtn()
    {
        _speedButton.onClick.AddListener(OnClickSpeed);
        _settingMenuScript.OnResumeButton += () => ApplySpeed(CurrentSpeed); // 일시정지 해제 시 현재 배속 적용
        // 저장된 배속 값 불러오기
        //배틀씬일때만 처음 배속 적용
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.buildIndex == 2)
        {
            CurrentSpeed = SettingDataManager.SavedSpeed;
            ApplySpeed(CurrentSpeed);
        }
        else
        {
            ApplySpeed(SpeedState.X1);
        }
    }

    private void OnClickSpeed()
    {
        if(_settingPanelCanvasGroup.interactable)
        {
            return;
        }

        ToggleSpeed();
    }

    private void ToggleSpeed()
    {
        switch (CurrentSpeed)
        {
            case SpeedState.X1:
                SetSpeed(SpeedState.X2);
                break;
            case SpeedState.X2:
                SetSpeed(SpeedState.X3);
                break;
            case SpeedState.X3:
                SetSpeed(SpeedState.X1);
                break;
        }
    }

    private void SetSpeed(SpeedState speed)
    {
        CurrentSpeed = speed;
        SettingDataManager.SavedSpeed = CurrentSpeed;
        ApplySpeed(speed);
    }

    private void ApplySpeed(SpeedState speed)
    {
        Time.timeScale = (int)speed;
        speedText.text = $"x{(int)speed}";
        //Debug.Log($"[SpeedBtn] 현재 배속: {speed}");
    }

    void ApplyCurSpeed(TutorialSkipEvent e)
    {
        Time.timeScale = (int)CurrentSpeed;
        speedText.text = $"x{(int)CurrentSpeed}";
    }
    public void OnBackPressed()
    {
        Debug.Log("[UIPause] 뒤로 가기 버튼 눌림");
        OnPauseButtonClicked();
    }
}
