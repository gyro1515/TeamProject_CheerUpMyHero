using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// 팝업의 스타일을 정의
public enum PopupStyle
{
    RetryOrCancel,  // 재시도 / 취소(타이틀로 이동)
    RetryOrQuit,    // 재시도 / 앱종료 (시작 화면용)
    ConfirmOnly,    // 확인 (단순 알림용)
}

public class SystemPopup : BaseUI
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text titleText;   // 제목 (새로 추가 필요, 없으면 messageText만 사용)
    [SerializeField] private TMP_Text messageText; // 본문

    [Space]
    [Header("Buttons")]
    // 기존 버튼들을 재활용하지만, 역할은 일반화합니다.
    [SerializeField] private Button positiveButton; // 기존 retryButton (긍정: 재시도, 확인)
    [SerializeField] private Button negativeButton; // 기존 resetButton (부정: 취소, 타이틀로)
    [SerializeField] private Button quitButton;     // 기존 quitButton (앱 종료용)

    [Space]
    [Header("Button Texts")]
    [SerializeField] private TMP_Text positiveBtnText; // 버튼 텍스트 변경을 위해
    [SerializeField] private TMP_Text negativeBtnText;

    private UniTaskCompletionSource<bool> _tcs;

    private void OnEnable()
    {

        positiveButton.onClick.AddListener(() => OnButtonClick(true));
        negativeButton.onClick.AddListener(() => OnButtonClick(false));
        quitButton.onClick.AddListener(() => OnButtonClick(false));
    }

    private void OnDisable()
    {
        positiveButton.onClick.RemoveAllListeners();
        negativeButton.onClick.RemoveAllListeners();
        quitButton.onClick.RemoveAllListeners();
    }

    public async UniTask<bool> ShowMessageAsync(string title, string message, PopupStyle style)
    {
        // 1. 텍스트 설정
        if (titleText != null) titleText.text = title;
        messageText.text = message;

        // 2. 스타일별 버튼 세팅
        ResetButtons();

        switch (style)
        {
            case PopupStyle.RetryOrCancel:
                positiveButton.gameObject.SetActive(true);
                negativeButton.gameObject.SetActive(true);
                if (positiveBtnText) positiveBtnText.text = "재시도";
                if (negativeBtnText) negativeBtnText.text = "타이틀로";
                break;

            case PopupStyle.RetryOrQuit:
                positiveButton.gameObject.SetActive(true);
                quitButton.gameObject.SetActive(true); // 기존 prefab 구조 유지를 위해 quit버튼 별도 사용
                if (positiveBtnText) positiveBtnText.text = "재시도";
                break;

            case PopupStyle.ConfirmOnly:
                positiveButton.gameObject.SetActive(true);
                if (positiveBtnText) positiveBtnText.text = "확인";
                break;
        }

        // 3. UI 켜기
        this.OpenUI(); // BaseUI의 Open 메서드 사용 가정 (없다면 gameObject.SetActive(true))

        // 4. 결과 대기
        _tcs = new UniTaskCompletionSource<bool>();
        return await _tcs.Task;
    }

    private void ResetButtons()
    {
        positiveButton.gameObject.SetActive(false);
        negativeButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
    }

    private void OnButtonClick(bool result)
    {
        this.CloseUI(); // BaseUI의 Close 메서드 사용 가정
        _tcs?.TrySetResult(result);
    }

}
