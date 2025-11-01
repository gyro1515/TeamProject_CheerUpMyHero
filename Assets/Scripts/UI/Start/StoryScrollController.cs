using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StoryScrollController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("UI 참조")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button skipButton;
    [SerializeField] private GameObject storyPanelRoot;

    [Header("스크롤 설정")]
    [SerializeField] private float scrollDuration = 10.0f; // 총 스크롤 시간 
    [SerializeField] private float fastScrollMultiplier = 3.0f; // 터치 시 3배 빨라짐
    private bool isHolding = false;
    private Coroutine scrollCoroutine;

    private void OnEnable()
    {
        StartStory();
    }
    private void Start()
    {
        skipButton?.onClick.AddListener(OnSkipClicked);
    }

    void StartStory()
    {
        storyPanelRoot.SetActive(true);
        isHolding = false; // 시작 시 터치 상태 초기화
        // 2. 스크롤 위치를 맨 위(1.0)로 즉시 설정
        scrollRect.verticalNormalizedPosition = 1f;

        // 3. 자동 스크롤 코루틴 시작
        if (scrollCoroutine != null) StopCoroutine(scrollCoroutine);
        scrollCoroutine = StartCoroutine(AutoScroll());
    }

    private IEnumerator AutoScroll()
    {
        //scrollRect.verticalNormalizedPosition = 0f;

        yield return new WaitForEndOfFrame();
        float startPosition = 1f; // 맨 아래
        float endPosition = 0f;   // 맨 위

        float timer = 0f;
        while (timer < scrollDuration)
        {
            float speed = isHolding ? fastScrollMultiplier : 1.0f;
            timer += Time.deltaTime * speed;
            float progress = timer / scrollDuration;

            // Lerp를 사용하여 부드럽게 스크롤 위치 변경 (0 -> 1)
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(startPosition, endPosition, progress);
            yield return null; // 다음 프레임까지 대기
        }

        // 스크롤이 끝나면 자동으로 스킵 처리 (메인 씬 로드)
        OnSkipClicked();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("터치 시작! 스크롤 가속");
        isHolding = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("터치 종료. 스크롤 감속");
        isHolding = false;
    }
    public void OnSkipClicked()
    {
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
        }
        if(GameManager.IsTutorialCompleted)
        {
            Debug.Log("스토리 스킵! 튜토리얼 클리어로 메인 씬으로 이동합니다.");
            SceneLoader.Instance.StartLoadScene(SceneState.MainScene);
        }
        else
        {
            Debug.Log("스토리 스킵! 튜토리얼으로 이동합니다.");
            SceneLoader.Instance.StartLoadScene(SceneState.BattleScene);
        }
        

        //storyPanelRoot.SetActive(false);
    }
}