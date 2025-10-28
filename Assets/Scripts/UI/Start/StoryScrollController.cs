using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StoryScrollController : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button skipButton;
    [SerializeField] private GameObject storyPanelRoot;

    [Header("스크롤 설정")]
    [SerializeField] private float scrollDuration = 10.0f; // 총 스크롤 시간 

    private Coroutine scrollCoroutine;

    private void Start()
    {
        skipButton?.onClick.AddListener(OnSkipClicked);
    }

    public void StartStory()
    {
        storyPanelRoot.SetActive(true);
        
        // 2. 스크롤 위치를 맨 위(1.0)로 즉시 설정
        scrollRect.verticalNormalizedPosition = 1f;

        // 3. 자동 스크롤 코루틴 시작
        if (scrollCoroutine != null) StopCoroutine(scrollCoroutine);
        scrollCoroutine = StartCoroutine(AutoScroll());
    }

    private IEnumerator AutoScroll()
    {
        scrollRect.verticalNormalizedPosition = 0f;

        yield return new WaitForEndOfFrame();
        float startPosition = 1f; // 맨 아래
        float endPosition = 0f;   // 맨 위

        float timer = 0f;
        while (timer < scrollDuration)
        {
            
            timer += Time.deltaTime;
            float progress = timer / scrollDuration;

            // Lerp를 사용하여 부드럽게 스크롤 위치 변경 (0 -> 1)
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(startPosition, endPosition, progress);
            yield return null; // 다음 프레임까지 대기
        }

        // 스크롤이 끝나면 자동으로 스킵 처리 (메인 씬 로드)
        OnSkipClicked();
    }

    public void OnSkipClicked()
    {
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
        }

        Debug.Log("스토리 스킵! 메인 씬으로 이동합니다.");
        SceneLoader.Instance.StartLoadScene(SceneState.MainScene);

        storyPanelRoot.SetActive(false);
    }
}