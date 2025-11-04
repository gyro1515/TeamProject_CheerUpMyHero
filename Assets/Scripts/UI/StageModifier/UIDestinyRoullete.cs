using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum GameMode
{
    Normal,
    Hard
}

public class UIDestinyRoullette : BaseUI
{
    #region 변수 참조
    [Header("UI 참조 - 인트로")]
    [SerializeField] private BasePopUpUI _introPanel;
    [SerializeField] private float _introShowTime = 1f;

    #region 돌림판 변수
    //[Header("UI 참조 - 돌림판")]
    //[SerializeField] private Transform _wheelContainer;
    //[SerializeField] private Image _fortuneSlice;
    //[SerializeField] private Image _misfortuneSlice;
    //[SerializeField] private float _textDistance = 300f;
    //[SerializeField] private TextMeshProUGUI _fortuneText;
    //[SerializeField] private TextMeshProUGUI _misfortuneText;
    //[SerializeField] private float _perTextDistance = 150f;
    //[SerializeField] private TextMeshProUGUI _fortunePerText;
    //[SerializeField] private TextMeshProUGUI _misfortunePerText;

    //[Header("수치 설정")]
    //[SerializeField] private float _spinDuration = 1.0f;
    //[SerializeField] private int _minSpins = 5;
    //[SerializeField] private AnimationCurve _spinCurve;
    #endregion

    [Header("UI 참조 - 팝업")]
    [SerializeField] private UIDestinyEffectPopup _effectPopup;
    [SerializeField] private UIChallengePopup _challengePopup;
    [SerializeField] private Button _challengeButton;
    [SerializeField] private Button _confirmButton;

    [Header("임시값")]
    [SerializeField] private GameMode _gameMode = GameMode.Normal;

    private (int mainStage, int subStage) _stage;

    private DestinyModel _model;
    private DestinyRoulleteViewModel _viewModel;

    private float fadeDuration = FadeManager.fadeDuration;  // 이거 없애고 싶은데

    private DeckPresetController _deckPresetController;
    #endregion

    private void Awake()
    {
        _model = new DestinyModel();
        _viewModel = new DestinyRoulleteViewModel(_model);

        _viewModel.OnIntroStateChanged += SetIntroPanel;
        _viewModel.OnResultSet += _effectPopup.OpenPanel;
        _viewModel.OnCloseView += CloseUI;

        #region 돌림판 이벤트 구독
        //_viewModel.OnWheelStartAngleSet += SetWheelStartAngel;
        //_viewModel.OnWheelVisualSet += SetWheelVisuals;
        //_viewModel.OnStartSpin += StartSpin;

        //_fortuneText.text = "행운";
        //_misfortuneText.text = "불행";
        #endregion

        // 버튼 활성화 or 비활성화용 구독
        _viewModel.OnConfirmStateChanged += (interactable) => { _confirmButton.interactable = interactable; };
        _viewModel.OnChallengeStateChanged += (interactable) => { _challengeButton.interactable = interactable; };

        _confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        _challengeButton.onClick.AddListener(OnChallengeButtonClicked);

    }
    private void Start()
    {
        _deckPresetController = UIManager.Instance.GetUI<DeckPresetController>();

    }
    private void OnEnable()
    {
        _stage = PlayerDataManager.Instance.SelectedStageIdx;

        _viewModel.OnviewEnabled(_gameMode, _stage);

        StartCoroutine(DestinySequenceCoroutine());
    }

    private IEnumerator DestinySequenceCoroutine()
    {
        yield return new WaitForSeconds(_introShowTime);

        _introPanel.CloseUI();
        yield return new WaitForSeconds(fadeDuration);

        _viewModel.OnIntroSequenceFinished();

        // _viewModel.OnIntroFinished(_wheelContainer.localEulerAngles.z, _minSpins); -> 원래 인트로 끝나고 돌림판 그리는 로직이었음
    }

    private void SetIntroPanel(bool show)
    {
        if (show)
        {
            _introPanel.OpenUI();
        }
    }

    private void OnChallengeButtonClicked()
    {
        _effectPopup.gameObject.SetActive(false);

        _challengePopup.OpenUI();
    }

    private void OnConfirmButtonClicked()
    {
        _viewModel.ApplyDestiny();
        _challengePopup.ApplyChanges();
        //_viewModel.CloseView();

        Debug.Log("운명 효과 적용 완료, 덱 선택으로 이동");
        GameManager.IsStageAndDestinySelected = true; // 스테이지/운명 선택 완료 플래그 설정
        if (_deckPresetController != null)
        {
            FadeManager.Instance.SwitchGameObjects(gameObject, _deckPresetController.gameObject);
        }
    }


    #region 돌림판 메서드 : 인트로 -> 돌림판 돌림 -> 결과 추첨 -> 버튼 누르면 효과 적용

    //private void StartSpin(float totalDegree, float startAngle)
    //{
    //    StartCoroutine(SpinCoroutine(totalDegree, startAngle));
    //}

    //private IEnumerator SpinCoroutine(float totalDegree, float startAngle)
    //{
    //    float elapsedTime = 0f;
    //    while (elapsedTime < _spinDuration)
    //    {
    //        elapsedTime += Time.deltaTime;
    //        float progressRate = elapsedTime / _spinDuration;
    //        float curveProgress = _spinCurve.Evaluate(progressRate);
    //        float currentAngle = Mathf.Lerp(0, totalDegree, curveProgress);
    //        _wheelContainer.localEulerAngles = new Vector3(0, 0, startAngle + currentAngle);

    //        yield return null;
    //    }

    //    _viewModel.OnSpinFinished(_wheelContainer.localEulerAngles.z);
    //}
    #endregion

    #region 돌림판 UI 세팅 메서드 : 최초 위치 랜덤 + 돌림판 그리기 + 인트로 띄우기
    //private void SetWheelStartAngel(float angle)
    //{
    //    _wheelContainer.localEulerAngles = new Vector3(0, 0, angle);
    //}

    //private void SetWheelVisuals(float fortune, float misfortune, float misfortuneRotation)
    //{
    //    _fortuneSlice.fillAmount = fortune;
    //    _misfortuneSlice.fillAmount = 1.0f;
    //    _misfortuneSlice.transform.localEulerAngles = Vector3.zero;
    //    //_misfortuneSlice.fillAmount = misfortune;
    //    //_misfortuneSlice.transform.localEulerAngles = new Vector3(0, 0, misfortuneRotation);

    //    float fortuneAngle = fortune * 360f;
    //    float fortuneCenterDegree = fortuneAngle / 2f;
    //    float misfortuneCenterDegree = fortuneAngle + (misfortune * 360f) / 2f;
    //    //float misfortuneCenterDegree = misfortuneRotation + (misfortune * 360f) / 2f;


    //    _fortuneText.rectTransform.localPosition = SetTextPosition(fortuneCenterDegree, _textDistance);
    //    _fortuneText.rectTransform.localEulerAngles = new Vector3(0, 0, -fortuneCenterDegree);

    //    _misfortuneText.rectTransform.localPosition = SetTextPosition(misfortuneCenterDegree, _textDistance);
    //    _misfortuneText.rectTransform.localEulerAngles = new Vector3(0, 0, -misfortuneCenterDegree);

    //    _fortunePerText.rectTransform.localPosition = SetTextPosition(fortuneCenterDegree, _perTextDistance);
    //    _fortunePerText.rectTransform.localEulerAngles = new Vector3(0, 0, -fortuneCenterDegree);
    //    _fortunePerText.text = $"{fortune * 100}%";

    //    _misfortunePerText.rectTransform.localPosition = SetTextPosition(misfortuneCenterDegree, _perTextDistance);
    //    _misfortunePerText.rectTransform.localEulerAngles = new Vector3(0, 0, -misfortuneCenterDegree);

    //    if (misfortune < 0.1f)
    //    {
    //        _misfortunePerText.text = "";
    //        _misfortuneText.text = "";
    //    }
    //    else
    //    {

    //        _misfortunePerText.text = $"{misfortune * 100}%";
    //    }

    //}

    //private Vector2 SetTextPosition(float angle, float distance)
    //{
    //    float radianAngle = angle * Mathf.Deg2Rad;

    //    float x = distance * Mathf.Sin(radianAngle);
    //    float y = distance * Mathf.Cos(radianAngle);

    //    return new Vector2(x, y);
    //}
    #endregion
}
