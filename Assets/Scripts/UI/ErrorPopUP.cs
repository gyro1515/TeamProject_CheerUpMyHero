using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ErrorPopUP : BaseUI
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button quitButton;

    private void OnEnable()
    {
        quitButton.onClick.AddListener(OnQuit);
    }

    private void OnDisable()
    {
        quitButton?.onClick.RemoveAllListeners();
    }

    public void ShowErrorPopUp(string message)
    {
        messageText.text = message;
        gameObject.SetActive(true);
    }

    private void OnQuit()
    {
#if UNITY_EDITOR
        // 에디터에서는 플레이 모드를 종료
        EditorApplication.isPlaying = false;
#else
        // 실제 빌드된 환경에서는 애플리케이션 종료
        Application.Quit();
#endif
    }
}
