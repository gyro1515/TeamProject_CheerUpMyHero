using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GachaSequenceController : BasePopUpUI
{
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

        // --- 3. (수정) 그리드를 '미리 채우고' (Populate) ---
        PopulateResultGrid(false); // flipAll = false

        int firstEpicIndex = FindFirstEpicIndex(_currentGachaResults);

        if (firstEpicIndex != -1) // 에픽이 있다 (1-2-3-4 순서)
        {
            // 3번 (단일 에픽 카드) 먼저 표시
            ShowSingleResultCard(_currentGachaResults[firstEpicIndex], true);
        }
        else // 에픽이 없다 (1-2-4-3 순서)
        {
            resultGridPanel.SetActive(true);
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
    }

    // 3-2. 단일 결과 카드 표시 (데이터 접근 수정)
    private void ShowSingleResultCard(int resultId, bool isEpicPreReveal = false)
    {
        var unitData = DataManager.PlayerUnitData.GetData(resultId);

        if (unitData != null)
        {
            singleUnitNameText.text = unitData.unitName;
            singleRarityText.text = unitData.rarity.ToString();
            singleResultImage.sprite = unitData.unitIconSprite;
            singleRarityBorder.color = GetColorForRarity(unitData.rarity);
        }
        else // 데이터 못 찾음
        {
            Debug.LogError($"[GachaSequence] ID: {resultId} 데이터 없음!");
            singleUnitNameText.text = "???";
            singleRarityText.text = "Unknown";
            singleResultImage.sprite = null;
            singleRarityBorder.color = Color.grey;
        }
        resultCardPanel.SetActive(true);
    }


    // 4. (에픽 없을 때) 그리드에서 뒤집힌 카드를 클릭하면 호출됨
    public void OnGridCardClicked(GachaResultIcon clickedIcon, int resultId)
    {
        _lastClickedIcon = clickedIcon;
        resultGridPanel.SetActive(false); // 그리드 숨기고
        ShowSingleResultCard(resultId);   // 단일 카드 표시
    }

    // 5-1. 단일 카드(3번)의 "확인" 버튼 클릭 시
    private void OnSingleResultConfirmed()
    {
        resultCardPanel.SetActive(false);

        // --- (수정) 그리드를 '새로 만들지 않고' 켜기만 합니다. ---
        resultGridPanel.SetActive(true); // 4번 (그리드)로 복귀

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
        envelopeAnimator.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
        resultCardPanel.SetActive(false);
        resultGridPanel.SetActive(false);

        PopulateResultGrid(true); // flipAll = true

        if (_currentGachaResults.Count == 1) // 1회 뽑기 스킵
        {
            resultGridPanel.SetActive(true); // 10회 뽑기처럼 그리드 화면을 보여줌 
        }
        else // 10회 뽑기 스킵
        {
            resultGridPanel.SetActive(true); // 모두 뒤집힌 그리드 표시
        }
    }

    // --- 헬퍼 함수 (데이터 접근 수정) ---
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