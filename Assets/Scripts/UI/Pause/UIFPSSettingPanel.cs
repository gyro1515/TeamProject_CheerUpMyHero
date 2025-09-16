using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFPSSettingPanel : BaseUI
{
    [Header("FPS버튼")]
    [SerializeField] private Button _30fpsButton;
    [SerializeField] private Button _60fpsButton;

    private void Awake()
    {
        _30fpsButton.onClick.AddListener(On30FPSButtonClicked);
        _60fpsButton.onClick.AddListener(On60FPSButtonClicked);
    }

    private void On30FPSButtonClicked()
    {
        Application.targetFrameRate = 30;
        Debug.Log("FPSSettingPanel -> 30프레임 고정");
        CloseUI();
    }

    private void On60FPSButtonClicked()
    {
        Application.targetFrameRate = 60;
        Debug.Log("FPSSettingPanel -> 60프레임 고정");
        CloseUI();
    }    
}
