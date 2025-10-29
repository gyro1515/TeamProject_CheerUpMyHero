using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

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
    [SerializeField] private VideoPlayer envelopeVideoPlayer; // EnvelopePanel의 Video Player
    [SerializeField] private RawImage envelopeRawImage; //  EnvelopePanel의 'Raw Image' 컴포넌트
    [SerializeField] private GameObject envelopePanelObject; // EnvelopePanel 게임 오브젝트
    [SerializeField] private GameObject resultCardPanel;
    [SerializeField] private GameObject resultGridPanel;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button envelopeButton;
    [SerializeField] private Sprite videoTexture;
    [SerializeField] private RenderTexture renderTexture;
    //[SerializeField] private int skipFrame = 2;

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
        if (envelopeVideoPlayer != null)
        {
            envelopeVideoPlayer.loopPointReached += OnVideoFinished;
            envelopeVideoPlayer.prepareCompleted += OnVideoPrepared;
        }
    }
    protected virtual void OnDestroy()
    {
        if (envelopeVideoPlayer != null)
        {
            envelopeVideoPlayer.loopPointReached -= OnVideoFinished;
            envelopeVideoPlayer.prepareCompleted -= OnVideoPrepared;
        }
    }
    public override void OnBackPressed()
    {
        Debug.Log($"{gameObject.name} 뒤로가기: ");
        if(skipButton.gameObject.activeSelf)
        {
            OnSkipClicked();
        }
        else if(resultCardPanel.activeSelf && singleConfirmButton.gameObject.activeSelf)
        {
            OnSingleResultConfirmed();
        }
        else if (gridConfirmButton.gameObject.activeSelf)
        {
            OnGridResultConfirmed();
        }
    }
    public void StartGachaSequence(List<int> resultIds)
    {
        _currentGachaResults = resultIds;
        OpenUI(); // 팝업 띄우기
        resultCardPanel.SetActive(false);
        resultGridPanel.SetActive(false);
        envelopePanelObject.SetActive(true);
        skipButton.gameObject.SetActive(true);

        foreach (Transform child in gridContentParent) Destroy(child.gameObject);

        envelopePanelObject.transform.SetAsLastSibling();
        skipButton.transform.SetAsLastSibling();

        if (envelopeRawImage != null)
        {
            envelopeRawImage.gameObject.SetActive(false);
        }

        envelopeButton.gameObject.SetActive(true);
        envelopeButton.interactable = true;

        if (envelopeVideoPlayer != null)
        {
            envelopeVideoPlayer.Stop();
            envelopeVideoPlayer.time = 0.0; 
        }

        currentState = GachaState.Envelope;
    }

    private void OnEnvelopeClicked()
    {
        Debug.Log("봉투 클릭됨! 애니메이션 시작...");
        envelopeButton.interactable = false;
        envelopeButton.gameObject.SetActive(false);

        if (envelopeRawImage != null)
        {
            envelopeRawImage.gameObject.SetActive(true);
        }

        if (envelopeVideoPlayer != null)
        {
            envelopeVideoPlayer.frame = 0;
            envelopeVideoPlayer.Prepare(); // 동영상 재생!
        }
    }
    private void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("비디오 첫 프레임 준비 완료. 클릭 가능 상태로 변경.");
        //if (envelopeRawImage != null)
        //{
        //    envelopeRawImage.texture = renderTexture;
        //}
        vp.frame = 0;
        Graphics.Blit(videoTexture.texture, renderTexture);
        Graphics.Blit(videoTexture.texture, vp.targetTexture);
        StartCoroutine(FrameCoruntine());
        vp.Play();
    }
    IEnumerator FrameCoruntine()
    {
        //yield return new WaitForSeconds(0.03f);
        while (true)
        {
            if (envelopeVideoPlayer.frame == 1)
            {
                Debug.Log(envelopeVideoPlayer.frame);
                if (envelopeRawImage != null)
                {
                    envelopeRawImage.texture = renderTexture;
                }
                yield break;
            }
            else
            {
                yield return null;
            }
        }
  
    }
    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("동영상 재생 완료! 다음 단계로...");
        vp.Stop();
        vp.frame = 0;
        if (envelopeRawImage != null)
        {
            envelopeRawImage.texture = videoTexture.texture;
        }
        OnEnvelopeAnimationFinished();
    }

    public void OnEnvelopeAnimationFinished()
    {

        envelopeVideoPlayer.Stop();
        envelopeVideoPlayer.frame = 0;
        if (envelopeRawImage != null)
        {
            envelopeRawImage.texture = videoTexture.texture;
        }
        envelopePanelObject.SetActive(false);
        skipButton.gameObject.SetActive(false);

        int highestEpicId = FindHighestEpicOrFirstEpic(_currentGachaResults);

        // 그리드를 '미리' 채웁니다. (이때 에픽들은 모두 미리 뒤집어 놓을 수 있음)
        PopulateResultGrid(false, highestEpicId); // flipAll = false

        if (highestEpicId != -1) // 에픽이 하나라도 있다! (1-2-3-4 순서)
        {
            // 3번 (단일 에픽 카드)을 '가장 높은 등급의' 에픽으로 먼저 표시
            ShowSingleResultCard(highestEpicId, true);
            currentState = GachaState.CardReveal;
            skipButton.gameObject.SetActive(false);
        }
        else // 에픽이 없다! (1-2-4-3 순서)
        {
            resultGridPanel.SetActive(true);
            currentState = GachaState.Grid;
            skipButton.gameObject.SetActive(true);
            CheckIfAllCardsFlipped();
        }
    }

    private void PopulateResultGrid(bool flipAll = false, int epicToPreReveal = -1)
    {

        foreach (Transform child in gridContentParent) Destroy(child.gameObject);

        for (int i = 0; i < _currentGachaResults.Count; i++)
        {
            int currentId = _currentGachaResults[i];
            GameObject iconGO = Instantiate(resultIconPrefab, gridContentParent);
            var iconScript = iconGO.GetComponent<GachaResultIcon>();


            bool showFlipped = flipAll || (epicToPreReveal != -1 && currentId == epicToPreReveal);

            iconScript.Setup(currentId, this, showFlipped);
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

    private int FindHighestEpicOrFirstEpic(List<int> results)
    {
        foreach (int id in results)
        {
            if (IsResultEpic(id)) return id;
        }
        return -1; // 에픽 없음
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

    private bool IsResultEpic(int id)
    {
        if (id == -1) return false;
        var unitData = DataManager.PlayerUnitData.GetData(id);
        if (unitData == null) return false;
        return unitData.rarity == Rarity.epic; 
    }
}