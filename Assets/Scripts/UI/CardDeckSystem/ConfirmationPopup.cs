using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class ConfirmationPopup : BaseUI
{
    [Header("UI 요소")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Toggle dontAskAgainToggle;

    private Action _onConfirm;
    private Action _onCancel;

    private CanvasGroup _canvasGroup;
    private bool _isClosing = false;

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(OnCancel);
    }

    public void Open(Action onConfirm, Action onCancel = null)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;

        dontAskAgainToggle.isOn = false;

        OpenUI();
    }

    private void OnConfirm()
    {
        if (dontAskAgainToggle.isOn)
        {
            PlayerPrefs.SetInt("DontAskAgain_EmptyDeck", 1);
        }

        _onConfirm?.Invoke();
        CloseUI();
    }

    private void OnCancel()
    {
        _onCancel?.Invoke();
        CloseUI();
    }

    public override void OpenUI()
    {
        if (_isClosing) return;
        base.OpenUI();
        FadeManager.FadeInUI(_canvasGroup);
    }
 
    public override void CloseUI()
    {
        if (_isClosing) return;
        _isClosing = true;

        FadeManager.FadeOutUI(_canvasGroup);
        StartCoroutine(CoCloseAfterDelay(0.3f));
    }

    private IEnumerator CoCloseAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        base.CloseUI();
        _isClosing = false;
    }
}