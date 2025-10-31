using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartUI : BaseUI
{
    private string _userId;
    [SerializeField] private TMP_Text _idText;
    [Header("UI 그룹 참조")]
    [SerializeField] private GameObject firstStartGroup;  // "아무곳이나 클릭하세요" 그룹
    [SerializeField] private SecondStartGroup secondStartGroup;
    [SerializeField] private StoryScrollController storyScrollController;
    [Header("클릭 버튼")]
    [SerializeField] private Button clickToMove;
    

    private void Start()
    {
        // 1. 게임 시작 시 초기 상태 설정
        if (firstStartGroup != null)
            firstStartGroup.SetActive(true);

        if (secondStartGroup != null)
        {
            secondStartGroup.Initialize(this);
            secondStartGroup.gameObject.SetActive(false);
        }
        if (storyScrollController != null)
            storyScrollController.gameObject.SetActive(false);
        // 2. 버튼 리스너 연결
        if (clickToMove != null)
        {
            clickToMove.onClick.AddListener(OnClickToMove);
        }
    }

    private void OnDestroy()
    {
        // 씬이 파괴될 때 리스너 연결 해제
        if (clickToMove != null)
        {
            clickToMove.onClick.RemoveListener(OnClickToMove);
        }
    }

    void OnClickToMove()
    {
        // SceneLoader.Instance.StartLoadScene(SceneState.MainScene); 

        if (firstStartGroup != null)
        {
            firstStartGroup.SetActive(false); // 첫 번째 그룹 끄기
        }
        if (secondStartGroup != null) secondStartGroup.gameObject.SetActive(true); // 로그인 그룹 켜기
    }
    public void OnLoginSuccess()
    {
        Debug.Log("StartUI: 로그인 성공 신호 받음. 스토리 씬 시작.");
        if (secondStartGroup != null && storyScrollController != null)
        {
            if (GameManager.IsTutorialCompleted)
            {
                Debug.Log("StartUI: 튜토리얼 완료됨. 스토리 패널 건너뛰고 메인 씬으로 바로 이동.");
                SceneLoader.Instance.StartLoadScene(SceneState.MainScene);
            }
            else
            {
                // 3b. 튜토리얼을 아직 안 깼다면:
                Debug.Log("StartUI: 튜토리얼 미완료. 스토리 패널 시작.");
                if (storyScrollController != null)
                {
                    FadeManager.Instance.SwitchGameObjects(secondStartGroup.gameObject, storyScrollController.gameObject);
                }
                else
                {
                    // 스토리 패널이 없는 비상시
                    Debug.LogError("StoryScrollController가 연결되지 않았습니다! 메인 씬으로 바로 이동합니다.");
                    SceneLoader.Instance.StartLoadScene(SceneState.MainScene);
                }
            }
        }
    }

    public void SetPlayerId(string userId)
    {
        _userId = userId;
        _idText.text = $"Guest Id:\n{_userId}";
    }
}