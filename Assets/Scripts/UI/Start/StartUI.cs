using UnityEngine;
using UnityEngine.UI;

public class StartUI : BaseUI
{
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
        if(secondStartGroup != null && storyScrollController != null)
        {
            FadeManager.Instance.SwitchGameObjects(secondStartGroup.gameObject, storyScrollController.gameObject);
        }
        /*if (secondStartGroup != null) secondStartGroup.gameObject.SetActive(false); // 로그인 그룹 끄기
        if (storyScrollController != null) storyScrollController.StartStory(); // 스토리 패널 켜기*/
    }
}