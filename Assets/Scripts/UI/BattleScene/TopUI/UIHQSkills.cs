using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHQSkills : MonoBehaviour
{
    [SerializeField] Button openButtion;
    [SerializeField] Button closeButtion;

    [SerializeField] CanvasGroup skillPanel;

    private void OnEnable()
    {
        openButtion.onClick.AddListener(OnOpenButton);
        closeButtion.onClick.AddListener(OnCloseButton);
    }

    private void OnDisable()
    {
        openButtion.onClick.RemoveAllListeners();
        closeButtion.onClick.RemoveAllListeners();
    }

    void OnOpenButton()
    {
        skillPanel.alpha =1.0f;
        skillPanel.interactable = true;
        skillPanel.blocksRaycasts = true;
        openButtion.gameObject.SetActive(false);
    }

    void OnCloseButton()
    {
        skillPanel.alpha = 0.0f;
        skillPanel.interactable = false;
        skillPanel.blocksRaycasts = false;
        openButtion.gameObject.SetActive(true);
    }

}
