using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ControlSettingPanel : BaseUI
{
    [Header("패널 변경 버튼")]
    [SerializeField] private Button _changeButton;

    [Header("돌아가기 버튼")]
    [SerializeField] private Button _cancelButton;

    [Header("조작 Ui 옵션 바꾸기")]
    [SerializeField] private Canvas _changeCanvas;
    private RectTransform _leftMoveButton;
    private RectTransform _rightMoveButton;
    private RectTransform _centerLine;
    private RectTransform _centerUnitPanel;
    private RectTransform _selUnitButton;
    private RectTransform _selPlayerButton;

    private bool isMoveButtonTop;

    private void Awake()
    {
        _changeButton.onClick.AddListener(OnChangeButtonClicked);
        _cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }

    private void OnChangeButtonClicked()
    {
       
    }

    private void OnCancelButtonClicked()
    {
        CloseUI();
    }
}
