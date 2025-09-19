using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainScreenUI : BaseUI
{
    [Header("버튼 세팅")]
    [SerializeField] private Button _adviserButton;
    [SerializeField] private Button _battleButton;
    [SerializeField] private Button _testButton;
    [SerializeField] private Button _deckSelectButton;
    [SerializeField] private Button _notYetButton;

    [Header("패널 (Canvas Group)")]
    [SerializeField] private CanvasGroup _battlePanelCanvasGroup;
    //[SerializeField] private CanvasGroup _testPanelCanvasGroup;
    [SerializeField] private CanvasGroup _deckSelectPanelCanvasGroup;

    UISelectCard uiSelectCard;


    private void Awake()
    {
        if (_adviserButton == null || _battleButton == null
            || _battlePanelCanvasGroup == null /*|| _testPanelCanvasGroup == null*/ || _testButton == null
            || _deckSelectPanelCanvasGroup == null || _deckSelectButton == null || _notYetButton == null)
        {
            Debug.LogError("MainSceneUI: 모든 UI 컴포넌트가 인스펙터에 연결되지 않았습니다.");
            return;
        }

        _adviserButton.onClick.AddListener(OnAdviserButtonClck);
        // 전투 버튼을 클릭했을 때 OnBattleButtonClick 메서드를 실행하도록 연결합니다.
        _battleButton.onClick.AddListener(OnBattleButtonClick);
        _testButton.onClick.AddListener(OnTestButtonClick);
        _deckSelectButton.onClick.AddListener(OnDeckSelectButtonClick);
        _notYetButton.onClick.AddListener(OnNotYetButtonClick);

        // OnEnable()로 이동, 열릴때마다 팝업 닫아주기
        /*_battlePanel.SetActive(false);
        _testPanel.SetActive(false);
        _deckSelectPanel.SetActive(false);*/
        uiSelectCard = UIManager.Instance.GetUI<UISelectCard>();
    }
    private void OnEnable()
    {
        ClosePanel(_battlePanelCanvasGroup, true);
        //ClosePanel(_testPanelCanvasGroup, true);
        ClosePanel(_deckSelectPanelCanvasGroup, true);
    }
    private void OnAdviserButtonClck()
    {
        OpenPanel(_battlePanelCanvasGroup);
        //OpenPanel(_testPanelCanvasGroup);
    }

    private void OnBattleButtonClick()
    {
        Debug.Log("덱 선택 패널을 엽니다.");
        OpenPanel(_deckSelectPanelCanvasGroup);
        ClosePanel(_battlePanelCanvasGroup);
        //ClosePanel(_testPanelCanvasGroup);
    }

    private void OnDeckSelectButtonClick()
    {
        // "덱 선택" 버튼 클릭 시 실행될 로직
        Debug.Log("덱을 선택하고 다음 단계로 넘어갑니다.");
        FadeManager.Instance.SwitchGameObjects(gameObject, uiSelectCard.gameObject);
    }

    private void OnNotYetButtonClick()
    {
        // "아직 아니야" 버튼 클릭 시 실행될 로직
        Debug.Log("덱 선택을 취소하고 패널을 닫습니다.");
        ClosePanel(_deckSelectPanelCanvasGroup);
    }

    private void OnTestButtonClick()
    {
        Debug.Log("테스트 버튼입니다.");
    }


    private void OpenPanel(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null) return;
        FadeEffectManager.Instance.FadeInUI(canvasGroup);
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
            FadeEffectManager.Instance.FadeOutUI(canvasGroup);
        }
    }

}

