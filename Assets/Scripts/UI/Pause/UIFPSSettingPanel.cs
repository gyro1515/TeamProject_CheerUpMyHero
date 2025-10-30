using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFPSSettingPanel : BaseUI
{
    [Header("FPS버튼")]
    [SerializeField] private Button _30fpsButton;
    [SerializeField] private Button _60fpsButton;
    [SerializeField] private Button _120fpsButton;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _30fpsButton.onClick.AddListener(On30FPSButtonClicked);
        _60fpsButton.onClick.AddListener(On60FPSButtonClicked);
        _120fpsButton.onClick.AddListener(On120FPSButtonClicked);

        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void On30FPSButtonClicked()
    {
        Application.targetFrameRate = 30;
        Debug.Log("FPSSettingPanel -> 30프레임 고정");
        FadeManager.FadeOutUI(_canvasGroup);
    }

    private void On60FPSButtonClicked()
    {
        Application.targetFrameRate = 60;
        Debug.Log("FPSSettingPanel -> 60프레임 고정");
        FadeManager.FadeOutUI(_canvasGroup);
    }    
    private void On120FPSButtonClicked()
    {
        Application.targetFrameRate = 120;
        Debug.Log("FPSSettingPanel -> 120프레임 고정");
        FadeManager.FadeOutUI(_canvasGroup);
    }    
}
