using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GachaSequenceController : BasePopUpUI
{
    [Header("UI 참조")]
    [SerializeField] private Animator envelopeAnimator;   // 봉투 (Animator)
    [SerializeField] private GameObject resultCardPanel; // 단일 카드 (GameObject)
    [SerializeField] private GameObject resultGridPanel; // 10회 그리드 (GameObject)
    [SerializeField] private Button skipButton;
    [SerializeField] private Button envelopeButton; 

    [Header("단일 카드 UI")]
    [SerializeField] private Image singleResultImage;
    [SerializeField] private TextMeshProUGUI singleRarityText;
    [SerializeField] private TextMeshProUGUI singleNameText; // ID 표시
    [SerializeField] private Button singleConfirmButton;

    [Header("10회 그리드 UI")]
    [SerializeField] private Transform gridContentParent;  // Grid Layout Group이 있는 곳
    [SerializeField] private GameObject resultIconPrefab; // 카드 1장 프리팹
    [SerializeField] private Button gridConfirmButton;

    private List<int> _currentGachaResults;
    private GachaResultIcon _lastClickedIcon;

    protected override void Awake()
    {
        base.Awake();
        singleConfirmButton?.onClick.AddListener(OnSingleResultConfirmed);
        gridConfirmButton?.onClick.AddListener(OnGridResultConfirmed);
        skipButton?.onClick.AddListener(OnSkipClicked);
        envelopeButton?.onClick.AddListener(OnEnvelopeClicked);
    }

    // 1. GachaUIPanel에서 이 함수를 호출하여 연출 시작
    public void StartGachaSequence(List<int> resultIds)
    {
        _currentGachaResults = resultIds;

        resultCardPanel.SetActive(false);
        resultGridPanel.SetActive(false);
        envelopeAnimator.gameObject.SetActive(true);
        skipButton.gameObject.SetActive(true);
        envelopeAnimator.transform.SetAsLastSibling();
        skipButton.transform.SetAsLastSibling();

        OpenUI(); // 4. 팝업 띄우기

        // 5. 봉투 클릭 가능하게 설정
        envelopeButton.interactable = true;
    }
    private void OnEnvelopeClicked()
    {
        Debug.Log("봉투 클릭됨! 애니메이션 시작...");

        envelopeButton.interactable = false;

        envelopeAnimator.SetTrigger("Open");
    }
    // 2. 봉투 "Open" 애니메이션의 마지막 프레임 이벤트로 호출됨 
    public void OnEnvelopeAnimationFinished()
    {
        envelopeAnimator.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);

        int firstEpicIndex = FindFirstEpicIndex(_currentGachaResults);

        if (firstEpicIndex != -1) // 에픽이 있다! (1-2-3-4 순서)
        {
            ShowSingleResultCard(_currentGachaResults[firstEpicIndex], true);
        }
        else // 에픽이 없다! (1-2-4-3 순서)
        {
            ShowResultGrid(false);
        }
    }

    // 3-1. 10회 뽑기 그리드 채우기
    private void ShowResultGrid(bool flipAll = false)
    {
        // 1. 그리드에 이미 카드가 채워져 있는지 확인합니다 (카드가 1개 이상 있는지).
        if (gridContentParent.childCount > 0)
        {
            // 2. 이미 카드가 있다면, 새로 만들지 않고 그냥 패널만 켭니다.
            resultGridPanel.SetActive(true);
            return; // 함수를 즉시 종료합니다.
        }

        // 3. 그리드 비우기 
        foreach (Transform child in gridContentParent) Destroy(child.gameObject);

        int firstEpicIndex = FindFirstEpicIndex(_currentGachaResults);

        for (int i = 0; i < _currentGachaResults.Count; i++)
        {
            GameObject iconGO = Instantiate(resultIconPrefab, gridContentParent);
            var iconScript = iconGO.GetComponent<GachaResultIcon>();

            bool showFlipped = (i == firstEpicIndex) || flipAll;
            iconScript.Setup(_currentGachaResults[i], this, showFlipped);
        }

        resultGridPanel.SetActive(true);
    }

    // 3-2. 단일 결과 카드 표시
    private void ShowSingleResultCard(int resultId, bool isEpicPreReveal = false)
    {
        // TODO: resultId로 유닛 데이터(스프라이트, 등급) 가져오기
        // var unitData = DataManager.Instance.GetUnitData(resultId);
        // singleResultImage.sprite = unitData.sprite;

        PostProcessGachaResult(resultId); 
        singleNameText.text = resultId.ToString(); 

        resultCardPanel.SetActive(true);
    }

    // 4. (에픽 없을 때) 그리드에서 뒤집힌 카드를 클릭하면 호출됨
    public void OnGridCardClicked(GachaResultIcon clickedIcon, int resultId)
    {
        _lastClickedIcon = clickedIcon; // 방금 클릭한 아이콘 기억

        resultGridPanel.SetActive(false); // 그리드 숨기고
        ShowSingleResultCard(resultId);   // 3번 (단일 카드) 표시
    }

    // 5-1. 단일 카드(3번)의 "확인" 버튼 클릭 시
    private void OnSingleResultConfirmed()
    {
        resultCardPanel.SetActive(false);

        ShowResultGrid(); // 4번 (그리드)로 복귀

        _lastClickedIcon?.Flip(false);
        _lastClickedIcon = null; // 클릭했던 아이콘 정보 리셋
    }

    // 5-2. 10회 그리드(4번)의 "확인" 버튼 클릭 시
    private void OnGridResultConfirmed()
    {
        resultGridPanel.SetActive(false);
        CloseUI(); // 연출 종료
    }

    // 5-3. 스킵 버튼 클릭 시
    private void OnSkipClicked()
    {
        // 1. 봉투 애니메이션과 스킵 버튼을 즉시 숨깁니다.
        envelopeAnimator.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);

        // 2. (안전장치) 다른 패널들도 모두 숨깁니다.
        resultCardPanel.SetActive(false);
        resultGridPanel.SetActive(false);

        // 3. 뽑기 결과가 1개인지 여러 개인지 확인합니다.
        if (_currentGachaResults.Count == 1) // 1회 뽑기 스킵
        {
            // 3번 화면 (단일 카드)을 바로 표시합니다.
            ShowSingleResultCard(_currentGachaResults[0]);
        }
        else // 10회 뽑기 스킵
        {
            // 4번 화면 (그리드)을 "모든 카드 즉시 뒤집기" 모드로 표시합니다.
            ShowResultGrid(true);
        }
    }

    // --- 헬퍼 함수 ---
    private int FindFirstEpicIndex(List<int> results)
    {
        for (int i = 0; i < results.Count; i++)
        {
            if (IsResultEpic(results[i])) return i;
        }
        return -1;
    }
    private bool IsResultEpic(int id) { return id > 125000; }
    private void PostProcessGachaResult(int id)
    {
        if (id > 125000) Debug.Log($"<color=magenta>Epic 결과:</color> {id}");
        else if (id > 115000) Debug.Log($"<color=cyan>Rare 결과:</color> {id}");
        else if (id == -1) Debug.LogWarning("가챠 실패 (-1)");
        else Debug.Log($"Common 결과: {id}");
    }
}