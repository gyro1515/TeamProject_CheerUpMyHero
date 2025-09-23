using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifactPassiveInventory : MonoBehaviour
{
    [Header("닫기 버튼")]
    [SerializeField] private Button _closeButton;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        
    }

    private void OnCloseButtonClicked()
    {
        FadeEffectManager.Instance.FadeOutUI(_canvasGroup);
    }
}
