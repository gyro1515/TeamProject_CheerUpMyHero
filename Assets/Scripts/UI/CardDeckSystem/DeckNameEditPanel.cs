using UnityEngine;
using System.Collections;

public class DeckNameEditPanel : BaseUI
{
    private CanvasGroup _canvasGroup;
    [SerializeField] DeckPresetController _controller;
    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public override void OpenUI()
    {
        base.OpenUI();
        FadeManager.Instance.FadeInUI(_canvasGroup);
    }

    public override void CloseUI()
    {
        FadeManager.Instance.FadeOutUI(_canvasGroup);
        _controller.ExitEditMode();
    }

}