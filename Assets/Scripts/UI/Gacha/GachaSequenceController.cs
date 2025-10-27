using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GachaSequenceController : BasePopUpUI
{
    private enum GachaState
    {
        Idle,      // 대기
        Envelope,  // 1-2. 봉투 애니메이션 중
        Grid,      // 4. 그리드 화면 (카드 뒤집기 대기)
        CardReveal // 3. 단일 카드 상세 보기
    }
    private GachaState currentState = GachaState.Idle;

    [Header("UI 참조")]
    [SerializeField] private Animator envelopeAnimator;
    [SerializeField] private GameObject resultCardPanel;
    [SerializeField] private GameObject resultGridPanel;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button envelopeButton;

    [Header("단일 카드 UI")]
    [SerializeField] private Image singleResultImage;
    [SerializeField] private Image singleRarityBorder;
    [SerializeField] private TextMeshProUGUI singleRarityText;
    [SerializeField] private TextMeshProUGUI singleUnitNameText;
    [SerializeField] private Button singleConfirmButton;
    [SerializeField] private Image singleCardBackgroundMaskImage;

    [Header("10회 그리드 UI")]
    [SerializeField] private Transform gridContentParent;
    [SerializeField] private GameObject resultIconPrefab;
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

    public void StartGachaSequence(List<int> resultIds)
    {
        _currentGachaResults = resultIds;

        resultCardPanel.SetActive(false);
        resultGridPanel.SetActive(false);
        envelopeAnimator.gameObject.SetActive(true);
        skipButton.gameObject.SetActive(true);

        foreach (Transform child in gridContentParent) Destroy(child.gameObject);

        envelopeAnimator.transform.SetAsLastSibling();
        skipButton.transform.SetAsLastSibling();

        Image envelopeImage = envelopeAnimator.GetComponent<Image>();
        if (envelopeImage != null)
        {
            Color color = envelopeImage.color;
            color.a = 1f; 
            envelopeImage.color = color;
        }
        envelopeAnimator.transform.localScale = Vector3.one; // 크기를 (1, 1, 1)로 되돌림
        OpenUI(); // 팝업 띄우기
        envelopeButton.interactable = true; // 봉투 클릭 가능하게
        currentState = GachaState.Envelope;
        skipButton.gameObject.SetActive(true); // 스킵 버튼 표시
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

        // --- 3. (수정) 그리드를 '미리 채우고' (Populate) ---
        PopulateResultGrid(false); // flipAll = false

        int firstEpicIndex = FindFirstEpicIndex(_currentGachaResults);

        if (firstEpicIndex != -1) // 에픽이 있다 (1-2-3-4 순서)
        {
            // 3번 (단일 에픽 카드) 먼저 표시
            ShowSingleResultCard(_currentGachaResults[firstEpicIndex], true);
            currentState = GachaState.CardReveal;
            skipButton.gameObject.SetActive(false);
        }
        else // 에픽이 없다 (1-2-4-3 순서)
        {
            resultGridPanel.SetActive(true);
            currentState = GachaState.Grid;
            skipButton.gameObject.SetActive(true);
            CheckIfAllCardsFlipped();
        }
    }

    private void PopulateResultGrid(bool flipAll = false)
    {

        int firstEpicIndex = FindFirstEpicIndex(_currentGachaResults);

        for (int i = 0; i < _currentGachaResults.Count; i++)
        {
            GameObject iconGO = Instantiate(resultIconPrefab, gridContentParent);
            var iconScript = iconGO.GetComponent<GachaResultIcon>();

            bool showFlipped = (i == firstEpicIndex) || flipAll;
            iconScript.Setup(_currentGachaResults[i], this, showFlipped);
        }
        CheckIfAllCardsFlipped();
    }

    // 3-2. 단일 결과 카드 표시 (데이터 접근 수정)
    private void ShowSingleResultCard(int resultId, bool isEpicPreReveal = false)
    {
        var unitData = DataManager.PlayerUnitData.GetData(resultId);

        if (unitData != null)
        {
            singleUnitNameText.text = unitData.unitName;
            singleRarityText.text = unitData.rarity.ToString();

            // 1. 가챠 전용 일러스트(gachaHeroSprite)가 있는지 확인
            if (unitData.gachaHeroSprite != null)
            {
                singleResultImage.sprite = unitData.gachaHeroSprite;
                singleRarityBorder.sprite = null;
                singleRarityBorder.color = Color.clear;
                singleRarityBorder.gameObject.SetActive(true);
            }
            else
            {
                singleResultImage.sprite = unitData.unitIconSprite;
                singleRarityBorder.sprite = unitData.unitBGSprite;
                singleRarityBorder.color = Color.white;
                singleRarityBorder.gameObject.SetActive(true);
            }
        }
        else // 데이터를 못 찾았을 경우
        {
            Debug.LogError($"[GachaSequence] ID: {resultId} 데이터 없음!");
            singleUnitNameText.text = "???";
            singleRarityText.text = "Unknown";
            singleResultImage.sprite = null;
            singleCardBackgroundMaskImage.gameObject.SetActive(false);
            singleRarityBorder.gameObject.SetActive(false);
        }

        resultCardPanel.SetActive(true);
    }


    // 4. (에픽 없을 때) 그리드에서 뒤집힌 카드를 클릭하면 호출됨
    public void OnGridCardClicked(GachaResultIcon clickedIcon, int resultId)
    {
        _lastClickedIcon = clickedIcon;
        resultGridPanel.SetActive(false); // 그리드 숨기고
        ShowSingleResultCard(resultId);   // 단일 카드 표시
        currentState = GachaState.CardReveal;
        skipButton.gameObject.SetActive(false);
    }

    // 5-1. 단일 카드(3번)의 "확인" 버튼 클릭 시
    private void OnSingleResultConfirmed()
    {
        resultCardPanel.SetActive(false);
        resultGridPanel.SetActive(true); // 4번 (그리드)로 복귀
        currentState = GachaState.Grid;
        skipButton.gameObject.SetActive(true); // "모두 뒤집기" 버튼 다시 표시
        _lastClickedIcon?.Flip(false);
        _lastClickedIcon = null; // 클릭했던 아이콘 정보 리셋
        CheckIfAllCardsFlipped();
    }

    // 5-2. 10회 그리드(4번)의 "확인" 버튼 클릭 시
    private void OnGridResultConfirmed()
    {
        resultGridPanel.SetActive(false);
        CloseUI(); // 연출 종료
    }
    private void OnSkipClicked()
    {
        // --- 1. 현재 상태 확인 ---
        if (currentState == GachaState.Envelope)
        {
            // --- 1단계 스킵: 봉투 애니메이션 스킵 ---
            Debug.Log("스킵 1단계: 봉투 애니메이션 스킵");

            // 봉투 애니메이션이 끝난 것처럼 OnEnvelopeAnimationFinished()를 즉시 호출
            OnEnvelopeAnimationFinished();
        }
        else if (currentState == GachaState.Grid)
        {
            // --- 2단계 스킵: 그리드의 모든 카드 뒤집기 ---
            Debug.Log("스킵 2단계: 모든 카드 뒤집기");

            FlipAllRemainingCards(); // 모든 카드를 뒤집는 새 함수 호출
            skipButton.gameObject.SetActive(false); // 모든 카드를 뒤집었으니 스킵 버튼 숨기기
        }
    }


    // --- 헬퍼 함수 (데이터 접근 수정) ---
    private void FlipAllRemainingCards()
    {
        foreach (Transform child in gridContentParent)
        {
            GachaResultIcon iconScript = child.GetComponent<GachaResultIcon>();
            if (iconScript != null)
            {
                // false: 애니메이션 없이 즉시 / true: 애니메이션 재생
                iconScript.Flip(false);
            }
        }
        CheckIfAllCardsFlipped();
    }
    public void OnCardFlipped(GachaResultIcon icon)
    {
        CheckIfAllCardsFlipped();
    }
    private void CheckIfAllCardsFlipped()
    {
        // 그리드에 자식이 없으면 (아직 생성 전) 그냥 리턴
        if (gridContentParent.childCount == 0 || _currentGachaResults.Count == 0)
        {
            gridConfirmButton.gameObject.SetActive(false);
            return;
        }

        // 모든 자식(카드)을 순회합니다.
        foreach (Transform child in gridContentParent)
        {
            GachaResultIcon iconScript = child.GetComponent<GachaResultIcon>();
            if (iconScript != null && !iconScript.IsFlipped)
            {
                gridConfirmButton.gameObject.SetActive(false); // 확인 버튼 숨기기
                return; // 함수 즉시 종료
            }
        }

        Debug.Log("모든 카드가 뒤집혔습니다. 확인 버튼 활성화.");
        gridConfirmButton.gameObject.SetActive(true); // 확인 버튼 보이기
        skipButton.gameObject.SetActive(false); // 스킵 버튼은 숨기기
    }
    private int FindFirstEpicIndex(List<int> results)
    {
        for (int i = 0; i < results.Count; i++)
        {
            if (IsResultEpic(results[i])) return i;
        }
        return -1;
    }

    private bool IsResultEpic(int id)
    {
        if (id == -1) return false;
        var unitData = DataManager.PlayerUnitData.GetData(id);
        if (unitData == null) return false;
        return unitData.rarity == Rarity.epic; 
    }

    private Color GetColorForRarity(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.epic: return Color.yellow;
            case Rarity.rare: return Color.magenta;
            case Rarity.common: return Color.blue;
            default: return Color.white;
        }
    }
}