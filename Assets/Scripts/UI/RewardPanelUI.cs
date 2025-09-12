using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]

public class RewardPanelUI : BaseUI
{
    [Header("UI 요소 연결")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text ironText;
    [SerializeField] private TMP_Text magicStoneText;
    [SerializeField] private Button returnButton;



    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();


        if (GameManager.Instance != null)
        {
            GameManager.Instance.RewardPanelUI = this;
        }

        returnButton.onClick.AddListener(OnReturnToMainButton);
    }

    public void OpenUI(int gold, int wood, int iron, int magicStone)
    {
        goldText.text = $"골드 + {gold}";
        woodText.text = $"목재 + {wood}";
        ironText.text = $"철괴 + {iron}";
        magicStoneText.text = $"마력석 + {magicStone}";


        canvasGroup.alpha = 1f; // 다시 보이게
        canvasGroup.interactable = true; // 다시 상호작용 가능하게
        canvasGroup.blocksRaycasts = true; // 다시 클릭을 막도록
    }

    public override void CloseUI()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void OnReturnToMainButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SeongminTestScene");
    }
}