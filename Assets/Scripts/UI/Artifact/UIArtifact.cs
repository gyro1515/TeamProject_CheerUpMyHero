using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifact : MonoBehaviour
{
    [Header("돌아가기 버튼")]
    [SerializeField] private Button _closeButton;

    CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnCloseButtonClicked()
    {
        FadeEffectManager.Instance.FadeOutUI(_canvasGroup);
    }
}
