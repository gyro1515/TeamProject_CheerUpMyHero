using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainScreenUI : MonoBehaviour
{
    [Header("버튼 세팅")]
    [SerializeField] private Button _officerImageButton;
    [SerializeField] private Button _battleButton;
    [SerializeField] private Button _testButton;
    [SerializeField] private Button _deckSelectButton;
    [SerializeField] private Button _notYetButton;

    [Header("패널 세팅")]
    [SerializeField] private GameObject _battlePanel;
    [SerializeField] private GameObject _testPanel;
    [SerializeField] private GameObject _deckSelectPanel;


    private void Awake()
    {
        if (_officerImageButton == null || _battleButton == null
            || _battlePanel == null || _testPanel == null || _testButton == null 
            || _deckSelectPanel == null || _deckSelectButton == null || _notYetButton == null)
        {
            Debug.LogError("MainSceneUI: 모든 UI 컴포넌트가 인스펙터에 연결되지 않았습니다.");
            return;
        }

        _officerImageButton.onClick.AddListener(OnOfficerImageClick);
        // 전투 버튼을 클릭했을 때 OnBattleButtonClick 메서드를 실행하도록 연결합니다.
        _battleButton.onClick.AddListener(OnBattleButtonClick);
        _testButton.onClick.AddListener(OnTestButtonClick);
        _deckSelectButton.onClick.AddListener(OnDeckSelectButtonClick);
        _notYetButton.onClick.AddListener(OnNotYetButtonClick);

        _battlePanel.SetActive(false);
        _testPanel.SetActive(false);
        _deckSelectPanel.SetActive(false);

    }

    private void OnOfficerImageClick()
    {
        _battlePanel.SetActive(true);
        _testPanel.SetActive(true);
    }

    private void OnBattleButtonClick()
    {
        Debug.Log("덱 선택 패널을 엽니다.");
        _deckSelectPanel.SetActive(true);
        _battlePanel.SetActive(false);
        _testPanel.SetActive(false);
    }

    private void OnDeckSelectButtonClick()
    {
        // "덱 선택" 버튼 클릭 시 실행될 로직
        Debug.Log("덱을 선택하고 다음 단계로 넘어갑니다.");
    }

    private void OnNotYetButtonClick()
    {
        // "아직 아니야" 버튼 클릭 시 실행될 로직
        Debug.Log("덱 선택을 취소하고 패널을 닫습니다.");
        _deckSelectPanel.SetActive(false);
    }

    private void OnTestButtonClick()
    {
        Debug.Log("테스트 버튼입니다.");
    }
}

