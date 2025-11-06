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
    [SerializeField] private GameObject expRewardGroup;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private TMP_Text resultText;           // 승리 실패 뜨는 텍스트. 결과창 분리되면 없애기
    [SerializeField] private TMP_Text penaltyInfoText;

    [Header("버튼 그룹")]
    [SerializeField] private GameObject victoryButtonGroup;
    [SerializeField] private GameObject defeatButtonGroup;

    [Header("승리 버튼")]
    [SerializeField] private Button nextStageButton;
    [SerializeField] private Button retryButton_Victory;
    [SerializeField] private Button returnButton_Victory;

    [Header("패배 버튼")]
    [SerializeField] private Button reformDeckButton; // 251029: 재도전으로 변경: 덱 재편성으로 이동
    [SerializeField] private Button retryButton_Defeat; // 251029: 사용 안함
    [SerializeField] private Button returnButton_Defeat; // 251029: 스테이지 선택으로 변경: 메인으로 이동하여 스테이지 선택 가능하도록

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();


        if (GameManager.Instance != null)
        {
            GameManager.Instance.RewardPanelUI = this;
        }

        nextStageButton.onClick.AddListener(OnNextStageButton);
        reformDeckButton.onClick.AddListener(OnReformDeckButton);

        retryButton_Victory.onClick.AddListener(OnRetryButton);
        retryButton_Defeat.onClick.AddListener(OnRetryButton);
        returnButton_Victory.onClick.AddListener(OnReturnToMainButton);
            
        returnButton_Defeat.onClick.AddListener(() => 
        {
            GameManager.IsStageAndDestinySelected = false; // 스테이지 선택 가능하도록 설정
            OnReturnToMainButton();  
        });
    }

    public void OpenUI(int gold, int wood, int iron, int magicStone, bool isVictory, int exp = 0)
    {
        //goldText.text = $"골드 + {gold}";
        //woodText.text = $"목재 + {wood}";
        //ironText.text = $"철괴 + {iron}";
        //magicStoneText.text = $"마력석 + {magicStone}";
        // 골드 보상/페널티
        victoryButtonGroup?.SetActive(isVictory);
        defeatButtonGroup?.SetActive(!isVictory);
        penaltyInfoText?.gameObject.SetActive(!isVictory && GameManager.IsTutorialCompleted);
     
        if (!isVictory && GameManager.IsTutorialCompleted)
        {
            penaltyInfoText.text = "랜덤한 영지가 황폐화되었습니다.";
            penaltyInfoText.color = Color.red;
        }
        if (resultText != null)
        {
            resultText.text = isVictory ? "스테이지 승리" : "스테이지 패배";
            resultText.color = isVictory ? Color.black : Color.red;
        }

        // 골드
        goldRewardGroup.SetActive(gold != 0);
        if (gold != 0)
        {
            goldText.richText = true; // 리치 텍스트 기능 활성화
            goldText.text = isVictory ? $"골드 +{gold}" : $"골드 <color=red>{gold}</color> 감소";
        }

        // 목재
        woodRewardGroup.SetActive(wood != 0);
        if (wood != 0)
        {
            woodText.richText = true;
            woodText.text = isVictory ? $"목재 +{wood}" : $"목재 <color=red>{wood}</color> 감소";
        }

        // 철괴
        ironRewardGroup.SetActive(iron != 0);
        if (iron != 0)
        {
            ironText.richText = true;
            ironText.text = isVictory ? $"철괴 +{iron}" : $"철괴 <color=red>{iron}</color> 감소";
        }

        // 마력석
        magicStoneRewardGroup.SetActive(magicStone != 0);
        if (magicStone != 0)
        {
            magicStoneText.richText = true;
            magicStoneText.text = isVictory ? $"마력석 +{magicStone}" : $"마력석 <color=red>{magicStone}</color> 감소";
        }

        expRewardGroup.SetActive(exp != 0);
        if (exp != 0)
        {
            expText.richText = true;
            expText.text = isVictory ? $"경험치 +{exp}" : "";
        }

        base.OpenUI();

        // 튜토리얼시에서 
        if(!GameManager.IsTutorialCompleted)
        {
            //승리시 돌아가기 버튼만 활성화되도록
            if (isVictory)
            {
                nextStageButton.gameObject.SetActive(false);
                retryButton_Victory.gameObject.SetActive(false);
                // 테스트로 튜토리얼 클리어
            }
            // 패배시 다시하기만 활성화
            else
            {
                reformDeckButton.gameObject.SetActive(false);
                retryButton_Defeat.gameObject.SetActive(true);
                returnButton_Defeat.gameObject.SetActive(false);
            }
        }

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
        GameManager.Instance.LoadMain = LoadMain.DeckPresetController;

        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.activeChallenges.Clear();
        }

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
            //SceneLoader.Instance.StartLoadScene(SceneState.BattleScene);
            GameManager.Instance.LoadMain = LoadMain.UIDestinyRoullette; // 운명 선택부터 시작
            SceneLoader.Instance.StartLoadScene(SceneState.MainScene); // 운명 선택부터
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

        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.activeChallenges.Clear();
        }
        if(!GameManager.IsTutorialCompleted && GameManager.Instance != null)
        {
            GameManager.Instance.LoadMain = LoadMain.TutorialInWisdom;
        }
        SceneLoader.Instance.StartLoadScene(SceneState.MainScene);
    }

    public override void CloseUI()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        OnReturnToMainButton();
    }
}