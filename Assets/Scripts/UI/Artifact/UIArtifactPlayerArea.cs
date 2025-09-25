using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifactPlayerArea : BaseUI
{
    [Header("유물 슬롯")]
    [SerializeField] private Button slot1;
    [SerializeField] private Button slot2;
    [SerializeField] private Button slot3;
    [SerializeField] private Button slot4;

    [Header("패시브 인벤토리 패널")]
    [SerializeField] private CanvasGroup inventoryPanel;

    private void Awake()
    {
        slot1.onClick.AddListener(OnArtifactSlotclicked);
        slot2.onClick.AddListener(OnArtifactSlotclicked);
        slot3.onClick.AddListener(OnArtifactSlotclicked);
        slot4.onClick.AddListener(OnArtifactSlotclicked);
    }

    private void OnArtifactSlotclicked()
    {
        FadeManager.Instance.FadeInUI(inventoryPanel);
    }
}
