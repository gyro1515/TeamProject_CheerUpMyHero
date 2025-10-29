using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class NoticeNetworkError : BaseUI
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button quitButton;

    private UniTaskCompletionSource<bool> _tcs;

    private void OnEnable()
    {
        retryButton.onClick.AddListener(OnRetry);
        resetButton.onClick.AddListener(OnReset);
    }

    private void OnDisable()
    {
        retryButton.onClick.RemoveListener(OnRetry);
        resetButton.onClick.RemoveListener(OnReset);
        quitButton.onClick.RemoveAllListeners();
    }


    // 외부에서 팝업을 띄우고 사용자의 선택(true: 재시도, false: 취소)을 기다리는 함수
    public async UniTask<bool> ShowAndWaitForResponse(string message, bool isStartScene = false)
    {
       
        if (isStartScene)
        {
            resetButton.gameObject.SetActive(false);
            quitButton.gameObject.SetActive(true);
            quitButton.onClick.AddListener(OnQuit);
        }

        messageText.text = message;
        gameObject.SetActive(true);

        _tcs = new UniTaskCompletionSource<bool>();
        return await _tcs.Task;
    }

    private void OnRetry()
    {
        gameObject.SetActive(false);
        _tcs?.TrySetResult(true);
    }

    private void OnReset()
    {
        gameObject.SetActive(false);
        _tcs?.TrySetResult(false);

        SceneLoader.Instance.StartLoadScene(SceneState.StartScene);
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
