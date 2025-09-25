using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPassiveArtifactInventory : MonoBehaviour
{
    [Header("닫기 버튼")]
    [SerializeField] private Button _closeButton;

    [Header("인벤토리 버튼들")]
    [SerializeField] private Button _sortButton;
    [SerializeField] private Button _equipButton;
    [SerializeField] private Button _noEquipButton;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        _closeButton.onClick.AddListener(OnCloseButtonClicked);
    }

    private void OnCloseButtonClicked()
    {
        FadeEffectManager.Instance.FadeOutUI(_canvasGroup);
    }
}
