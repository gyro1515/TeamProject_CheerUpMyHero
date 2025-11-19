using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGameSettingPanel : BasePopUpUI
{
    [SerializeField] Toggle optionTogle;
    protected override void Awake()
    {
        base.Awake();
        optionTogle.onValueChanged.AddListener((isOn) =>
        {
            Debug.Log($"웨이브 워닝 중에 속도 변경 여부 설정: {isOn}");
            SettingDataManager.IsSpeedChangedInWaring = isOn;
        });
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        optionTogle.isOn = SettingDataManager.IsSpeedChangedInWaring;
    }
}
