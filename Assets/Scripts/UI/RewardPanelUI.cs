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
    [SerializeField] private TMP_Text resultText;           // 승리 실패 뜨는 텍스트. 결과창 분리되면 없애기
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

    public void OpenUI(int gold, int wood, int iron, int magicStone, bool isVictory)
    {
        goldText.text = $"골드 + {gold}";
        woodText.text = $"목재 + {wood}";
        ironText.text = $"철괴 + {iron}";
        magicStoneText.text = $"마력석 + {magicStone}";

        resultText.text = isVictory ? "스테이지 클리어" : "스테이지 실패";   // 승리, 실패 텍스트 조건문. 결과창 분리되면 삭제하기

        //패널을 끄고 실행을 하면 Awake에서 게임매니저에 자기 자신을 넣을 수 없어서 패널을 켜두고 알파값을 0으로 만든 상태에서
        //스테이지 클리어 함수가 실행이 되면 다시 알파값을 1로 만들고 보여지게
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
        //SceneManager.LoadScene("SeongminTestScene");//임시로 성민테스트씬 연결했음 추후 수정
        SceneLoader.Instance.StartLoadScene(SceneState.MainScene);
    }
}