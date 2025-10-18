using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode
{
    Normal,
    Hard
}

public class UIDestinyRoullette : BaseUI
{
    [Header("UI 참조 - 인트로")]
    [SerializeField] private GameObject _introPanel;
    [SerializeField] private CanvasGroup _introCanvasGroup;

    [Header("UI 참조 - 돌림판")]
    [SerializeField] private Transform _wheelContainer;
    [SerializeField] private Image _fortuneSlice;
    [SerializeField] private Image _misfortuneSlice;
    [SerializeField] private Button _startSpinButton;

    [Header("UI 참조 - 팝업")]
    [SerializeField] private UIDestinyEffectPopup _effectPopup;
    [SerializeField] private UIChallengePopup _challengePopup;
    [SerializeField] private Button _challengeButton;
    [SerializeField] private Button _confirmButton;

    [Header("수치 설정")]
    [SerializeField] private float _introShowTime = 3.0f;
    [SerializeField] private float _spinDuration = 1.0f;
    [SerializeField] private int _minSpins = 5;
    [SerializeField] private AnimationCurve _spinCurve;

    [Header("임시값")]     // 임시값은 나중에 삭제해야 함.
    [SerializeField] private GameMode _gameMode = GameMode.Normal;
    [SerializeField] private int _mainStage = 0;
    [SerializeField] private int _subStage = 0;

    private StageDestinyData _selectedDestiny;
    private float fortuneProbability;
    private bool isSpinning = false;
    private (int, int) _stage;

    private const float NormalBaseProbability = 0.50f;
    private const float NormalMinProbability = 0.32f;
    private const float HardBaseProbability = 0.30f;
    private const float HardMinProbability = 0.12f;

    private void Awake()
    {
        //gameObject.SetActive(false);

        _startSpinButton.onClick.AddListener(OnStartSpinButtonClicked);
        _confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        _challengeButton.onClick.AddListener(OnChallengeButtonClicked);
    }

    private void OnEnable()
    {
        // _stage = PlayerDataManager.Instance.SelectedStageIdx;
        _stage = (_mainStage, _subStage); // 나중에 삭제
        SetWheel(_gameMode);
        _confirmButton.interactable = false;

        _introCanvasGroup.alpha = 0f;
        _introCanvasGroup.interactable = false;
        _introCanvasGroup.blocksRaycasts = false;

        StartCoroutine(DestinySelectSequenceCoroutine());
    }

    #region  값 설정 메서드 : 확률 계산 + 돌림판 세팅
    private void SetWheel(GameMode gameMode)
    {
        fortuneProbability = SetProbability(gameMode, _stage);

        _fortuneSlice.fillAmount = fortuneProbability;
        _misfortuneSlice.fillAmount = 1.0f - fortuneProbability;
        // _misfortuneSlice.transform.localEulerAngles = new Vector3(0, 0, fortuneProbability * 360f);

        float randomStartAngle = Random.Range(0f, 360f);
        _wheelContainer.localEulerAngles = new Vector3(0, 0, randomStartAngle);
    }

    private float SetProbability(GameMode mode, (int mainStageIdx, int subStageIdx) stage)
    {
        float baseProbability = 0.00f;
        float minProbability = 0.00f;

        int stageNum = stage.mainStageIdx * 9 + stage.subStageIdx;

        if (mode == GameMode.Normal)
        {
            baseProbability = NormalBaseProbability;
            minProbability = NormalMinProbability;
        }
        else if (mode == GameMode.Hard)
        {
            baseProbability = HardBaseProbability;
            minProbability = HardMinProbability;
        }
        else
        {
            Debug.Log("돌림판 확률 정하는 로직에 문제 있어요");
        }

        float penalty = stageNum * 0.02f;
        float finalProbability = baseProbability - penalty;

        return Mathf.Max(finalProbability, minProbability);
    }
    #endregion

    #region 돌림판 메서드 : 인트로 -> 돌림판 돌림 -> 결과 추첨 -> 버튼 누르면 효과 적용

    private IEnumerator DestinySelectSequenceCoroutine()
    {
        FadeManager.FadeInUI(_introCanvasGroup);
        yield return new WaitForSeconds(FadeManager.fadeDuration);

        yield return new WaitForSeconds(_introShowTime - (2 * FadeManager.fadeDuration));
        
        FadeManager.FadeOutUI(_introCanvasGroup);
        yield return new WaitForSeconds(FadeManager.fadeDuration);

        yield return StartCoroutine(SpinCoroutine());
        _confirmButton.interactable = true;
    }

    private IEnumerator SpinCoroutine()
    {
        isSpinning = true;
        _startSpinButton.interactable = false;
        float elapsedTime = 0f;
        float startAngle = _wheelContainer.localEulerAngles.z;
        float totalDegree = 360f * _minSpins + Random.Range(0, 360f);

        while (elapsedTime < _spinDuration)
        {
            elapsedTime += Time.deltaTime;
            float progressRate = elapsedTime / _spinDuration;               // progressRate : 진행률
            float curveProgress = _spinCurve.Evaluate(progressRate);        // 애니메이션 커브에 넣어서 커브가 진행률 따라서 진행되도록 함
            float currentAngle = Mathf.Lerp(0, totalDegree, curveProgress); 
            _wheelContainer.localEulerAngles = new Vector3(0, 0, startAngle + currentAngle);
            yield return null;
        }

        CheckResult();
        isSpinning = false;
    }

    private void CheckResult()
    {
        float finalAngle = _wheelContainer.localEulerAngles.z;
        float arrowPoint = (360 - finalAngle) % 360;
        float fortuneAngleRange = fortuneProbability * 360;

        _selectedDestiny = null;
        DestinyType destinyType = arrowPoint <= fortuneAngleRange ? DestinyType.Fortune : DestinyType.Misfortune;

        List<StageDestinyData> destinyList = new List<StageDestinyData>();
        foreach (StageModifierData modifier in DataManager.Instance.StageModifierData.Values)
        {
            if (modifier is StageDestinyData destiny && destiny.destinyType == destinyType)
            {
                destinyList.Add(destiny);
            }
        }

        if (destinyList.Count > 0)
        {
            int randomIndex = Random.Range(0, destinyList.Count);
            _selectedDestiny = destinyList[randomIndex];
            Debug.Log($"결과 : {destinyType} {_selectedDestiny.name} 효과 추첨");
        }
        else
        {
            Debug.Log("운명 뽑아오는 로직 오류 있음");
            return;
        }


        _effectPopup.OpenPanel(_selectedDestiny);
        _startSpinButton.interactable = true;
    }
    #endregion

    #region 버튼 메서드
    private void OnConfirmButtonClicked()
    {
        if (_selectedDestiny == null)
        {
            Debug.Log("추첨 유물 null임 로직 문제 있어요");
            return;
        }
        _challengePopup.ApplyChanges();
        PlayerDataManager.Instance.SetDestiny(_selectedDestiny);
        Debug.Log($"{_selectedDestiny.name} 효과 잘 들어감");

        CloseUI();

        // 여기서 스테이지로 연결하든 뭐로 연결하든 연결 로직 넣으면 됨.
    }

    private void OnChallengeButtonClicked()
    {
        _challengePopup.OpenUI();
        _effectPopup.CloseUI();
    }
    #endregion


    // ▼▼▼▼▼▼▼▼ 여기 테스트용 임시 코드 ▼▼▼▼▼▼▼▼
    public void StartSpin()
    {
        if (!isSpinning)
        {
            StartCoroutine(SpinCoroutine());
        }
    }

    private void OnStartSpinButtonClicked()
    {
        StartSpin();
    }
}
