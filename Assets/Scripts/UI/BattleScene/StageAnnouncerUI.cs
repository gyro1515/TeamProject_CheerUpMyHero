using UnityEngine;
using TMPro;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class StageAnnouncerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stageNameText;
    [SerializeField] private float displayDuration = 6f; // 표시될 시간
    [SerializeField] private float fadeOutDuration = 0.5f; // 사라지는 데 걸리는 시간

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        StartCoroutine(ShowAndFadeOutRoutine());
    }

    private IEnumerator ShowAndFadeOutRoutine()
    {
        // PlayerDataManager에서 선택된 스테이지 정보를 가져옵니다.
        (int mainIdx, int subIdx) = PlayerDataManager.Instance.SelectedStageIdx;

        // 텍스트를 "1-1" 과 같은 형식으로 설정합니다.
        stageNameText.text = $"{mainIdx + 1} - {subIdx + 1}";

        // UI를 즉시 보이게 합니다
        _canvasGroup.alpha = 1f;

        //설정된 시간만큼 화면에 그대로 보여줍니다
        yield return new WaitForSeconds(displayDuration);

        _canvasGroup.DOFade(0, fadeOutDuration);

        yield return new WaitForSeconds(fadeOutDuration);

        gameObject.SetActive(false);
    }
}