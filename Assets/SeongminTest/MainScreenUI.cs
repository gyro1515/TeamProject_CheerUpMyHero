using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainScreenUI : MonoBehaviour
{
    [SerializeField] private Button _officerImageButton;
    [SerializeField] private Button _battleButton;
    [SerializeField] private Button _testButton;
    [SerializeField] private GameObject _battlePanel;
    [SerializeField] private GameObject _testPanel;

    private void Awake()
    {
        if (_officerImageButton == null || _battleButton == null 
            || _battlePanel == null || _testPanel == null || _testButton == null)
        {
            Debug.LogError("MainSceneUI: 모든 UI 컴포넌트가 인스펙터에 연결되지 않았습니다.");
            return;
        }

        _officerImageButton.onClick.AddListener(OnOfficerImageClick);
        // 전투 버튼을 클릭했을 때 OnBattleButtonClick 메서드를 실행하도록 연결합니다.
        _battleButton.onClick.AddListener(OnBattleButtonClick);
        _testButton.onClick.AddListener(OnTestButtonClick);

        _battlePanel.SetActive(false);
        _testPanel.SetActive(false);

    }

    private void OnOfficerImageClick()
    {
        _battlePanel.SetActive(true);
        _testPanel.SetActive(true);
    }

    private void OnBattleButtonClick()
    {
        Debug.Log("전투 씬으로 이동합니다.");
        //나중에 추가해야할 것 SceneLoadManager.Instance.LoadScene(SceneState.BattleScene);
    }

    private void OnTestButtonClick()
    {
        Debug.Log("테스트 버튼입니다.");
    }
}

