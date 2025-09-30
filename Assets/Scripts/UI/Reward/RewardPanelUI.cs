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
    [SerializeField] private GameObject goldRewardGroup;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private GameObject woodRewardGroup;
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private GameObject ironRewardGroup;
    [SerializeField] private TMP_Text ironText;
    [SerializeField] private GameObject magicStoneRewardGroup;
    [SerializeField] private TMP_Text magicStoneText;
    [SerializeField] private TMP_Text resultText;           // 승리 실패 뜨는 텍스트. 결과창 분리되면 없애기

    [Header("버튼 그룹")]
    [SerializeField] private GameObject victoryButtonGroup;
    [SerializeField] private GameObject defeatButtonGroup;

    [Header("승리 버튼")]
    [SerializeField] private Button nextStageButton;
    [SerializeField] private Button retryButton_Victory;
    [SerializeField] private Button returnButton_Victory;

    [Header("패배 버튼")]
    [SerializeField] private Button reformDeckButton;
    [SerializeField] private Button retryButton_Defeat;
    [SerializeField] private Button returnButton_Defeat;

    private CanvasGroup canvasGroup;
    private DeckPresetController _deckPresetController; // 덱 재편성을 위해 추가

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        _deckPresetController = UIManager.Instance.GetUI<DeckPresetController>();


        if (GameManager.Instance != null)
        {
            GameManager.Instance.RewardPanelUI = this;
        }

        nextStageButton.onClick.AddListener(OnNextStageButton);
        reformDeckButton.onClick.AddListener(OnReformDeckButton);

        retryButton_Victory.onClick.AddListener(OnRetryButton);
        retryButton_Defeat.onClick.AddListener(OnRetryButton);
        returnButton_Victory.onClick.AddListener(OnReturnToMainButton);
        returnButton_Defeat.onClick.AddListener(OnReturnToMainButton);
    }

    public void OpenUI(int gold, int wood, int iron, int magicStone, bool isVictory)
    {
        //goldText.text = $"골드 + {gold}";
        //woodText.text = $"목재 + {wood}";
        //ironText.text = $"철괴 + {iron}";
        //magicStoneText.text = $"마력석 + {magicStone}";
        if (gold > 0)
        {
            goldRewardGroup.SetActive(true);
            goldText.text = $"골드 + {gold}";
        }
        else
        {
            goldRewardGroup.SetActive(false);
        }

        // 목재 보상
        if (wood > 0)
        {
            woodRewardGroup.SetActive(true);
            woodText.text = $"목재 + {wood}";
        }
        else
        {
            woodRewardGroup.SetActive(false);
        }

        // 철괴 보상
        if (iron > 0)
        {
            ironRewardGroup.SetActive(true);
            ironText.text = $"철괴 + {iron}";
        }
        else
        {
            ironRewardGroup.SetActive(false);
        }

        // 마력석 보상
        if (magicStone > 0)
        {
            magicStoneRewardGroup.SetActive(true);
            magicStoneText.text = $"마력석 + {magicStone}";
        }
        else
        {
            magicStoneRewardGroup.SetActive(false);
        }

        resultText.text = isVictory ? "스테이지 클리어" : "스테이지 실패";   // 승리, 실패 텍스트 조건문. 결과창 분리되면 삭제하기

        if (isVictory)
        {
            victoryButtonGroup.SetActive(true);
            defeatButtonGroup.SetActive(false);
        }
        else
        {
            victoryButtonGroup.SetActive(false);
            defeatButtonGroup.SetActive(true);
        }

        // BaseUI의 OpenUI를 호출하여 페이드인 등 처리
        base.OpenUI();
    
    //패널을 끄고 실행을 하면 Awake에서 게임매니저에 자기 자신을 넣을 수 없어서 패널을 켜두고 알파값을 0으로 만든 상태에서
    //스테이지 클리어 함수가 실행이 되면 다시 알파값을 1로 만들고 보여지게
        canvasGroup.alpha = 1f; // 다시 보이게
        canvasGroup.interactable = true; // 다시 상호작용 가능하게
        canvasGroup.blocksRaycasts = true; // 다시 클릭을 막도록
    }
    private void OnReformDeckButton()
    {
        // 덱 재편성 화면으로 돌아가는 로직
        Time.timeScale = 1f;
        SceneLoader.Instance.StartLoadScene(SceneState.MainScene);
    }
    private void OnNextStageButton()
    {
        //PlayerDataManager에서 마지막으로 클리어한 스테이지 정보를 가져옴
        (int mainIdx, int subIdx) = PlayerDataManager.Instance.SelectedStageIdx;

        // SettingDataManager에서 전체 스테이지 데이터를 가져와 다음 스테이지가 유효한지 확인
        List<MainStageData> allStageData = SettingDataManager.Instance.MainStageData;

        // 다음 서브 스테이지 인덱스를 계산
        int nextSubIdx = subIdx + 1;
        int nextMainIdx = mainIdx;

        // 만약 현재 메인 스테이지의 마지막 서브 스테이지를 클리어했다면,
        if (nextSubIdx >= allStageData[mainIdx].subStages.Count)
        {
            // 다음 메인 스테이지의 첫 번째 서브 스테이지로 설정
            nextMainIdx = mainIdx + 1;
            nextSubIdx = 0;
        }

        // 다음 메인 스테이지가 존재하는지 확인
        if (nextMainIdx < allStageData.Count)
        {
            // 다음 스테이지 정보를 PlayerDataManager에 새로 저장
            PlayerDataManager.Instance.SelectedStageIdx = (nextMainIdx, nextSubIdx);

            //전투 씬을 다시 로드
            Time.timeScale = 1f;
            SceneLoader.Instance.StartLoadScene(SceneState.BattleScene);
            Debug.Log($"다음 스테이지 ({nextMainIdx + 1}-{nextSubIdx + 1})를 시작합니다.");
        }
        else
        {
            // 마지막 스테이지까지 모두 클리어한 경우
            Debug.Log("모든 스테이지를 클리어했습니다! 메인 화면으로 돌아갑니다.");
            OnReturnToMainButton();
        }
    }
    private void OnRetryButton()
    {
        // 현재 스테이지를 다시 시작하는 로직
        Time.timeScale = 1f;
        SceneLoader.Instance.StartLoadScene(SceneState.BattleScene);
    }

    private void OnReturnToMainButton()
    {
        Time.timeScale = 1f;
        SceneLoader.Instance.StartLoadScene(SceneState.MainScene);
    }

    public override void CloseUI()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}