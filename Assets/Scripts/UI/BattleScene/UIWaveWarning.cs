using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWaveWarning : BaseUI
{
    [Header("웨이브 경고 설정")]
    [SerializeField] RectTransform warningRT1;
    [SerializeField] RectTransform warningRT2;
    [SerializeField] float moveSpeed = 100f;
    [SerializeField] float displayTime = 3f;
    [SerializeField] float warningPanelWidth = 1626f; // 만약 이 값으로 세팅이 안된다면 연산 없애고 넓이 값 가져오기
    float displayTimer = 0f;
    public event Action OnWarningEnd; // 웨이브 경고가 끝날 때 발생하는 이벤트 -> 251106: 현재 사용안하지만 나중에 설정에 따라 쓸 수 있도록 변경 예정
    private void Awake()
    {
        CloseUI();
    }
    private void Start()
    {
        warningPanelWidth = warningRT1.rect.size.x;
    }
    private void Update()
    {
        if (!warningRT1 || !warningRT2) return;
        displayTimer += Time.deltaTime;
        warningRT1.anchoredPosition += Vector2.left * moveSpeed * Time.deltaTime;
        warningRT2.anchoredPosition += Vector2.left * moveSpeed * Time.deltaTime;
        if (warningRT1.anchoredPosition.x <= -warningPanelWidth)
        {
            warningRT1.anchoredPosition = new Vector2(warningRT2.anchoredPosition.x + warningPanelWidth,
                warningRT1.anchoredPosition.y);
        }
        else if(warningRT2.anchoredPosition.x <= -warningPanelWidth)
        {
            warningRT2.anchoredPosition = new Vector2(warningRT1.anchoredPosition.x + warningPanelWidth,
                warningRT2.anchoredPosition.y);
        }
        if (displayTimer < displayTime) return;
        displayTimer = 0f;
        CloseUI();
        //OnWarningEnd?.Invoke();
    }

    public override void OpenUI()
    {
        base.OpenUI();
        AudioManager.PlayOneShot(DataManager.AudioData.waveWarningSE, 0.6f);
        enabled = true;
    }
    public override void CloseUI()
    {
        base.CloseUI();
        enabled = false;
    }
}
