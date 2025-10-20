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
    #region 변수 참조
    [Header("UI 참조 - 인트로")]
    [SerializeField] private BasePopUpUI _introPanel;

    [Header("UI 참조 - 돌림판")]
    [SerializeField] private Transform _wheelContainer;
    [SerializeField] private Image _fortuneSlice;
    [SerializeField] private Image _misfortuneSlice;

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

    private (int mainStage, int subStage) _stage;

    private DestinyModel _model;
    private DestinyRoulleteViewModel _viewModel;

    private float fadeDuration = FadeManager.fadeDuration;  // 이거 없애고 싶은데
    #endregion

    private void Awake()
    {
        _model = new DestinyModel();
        _viewModel = new DestinyRoulleteViewModel(_model);

        _viewModel.OnIntroStateChanged += SetIntroPanel;
        _viewModel.OnWheelStartAngleSet += SetWheelStartAngel;
        _viewModel.OnWheelVisualSet += SetWheelVisuals;
        _viewModel.OnStartSpin += StartSpin;
        _viewModel.OnResultSet += _effectPopup.OpenPanel;
        _viewModel.OnCloseView += CloseUI;

        // 버튼 활성화 or 비활성화용 구독
        _viewModel.OnConfirmStateChanged += (interactable) => { _confirmButton.interactable = interactable; };
        _viewModel.OnChallengeStateChanged += (interactable) => { _challengeButton.interactable = interactable; };

        _confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        _challengeButton.onClick.AddListener(OnChallengeButtonClicked);
    }

    private void OnEnable()
    {
        _stage = PlayerDataManager.Instance.SelectedStageIdx;

        _viewModel.OnviewEnabled(_gameMode, _stage);

        StartCoroutine(DestinySequenceCoroutine());
    }

    #region 돌림판 메서드 : 인트로 -> 돌림판 돌림 -> 결과 추첨 -> 버튼 누르면 효과 적용

    private IEnumerator DestinySequenceCoroutine()
    {
        yield return new WaitForSeconds(_introShowTime);
        
        _introPanel.CloseUI();
        yield return new WaitForSeconds(fadeDuration);

        _viewModel.OnIntroFinished(_wheelContainer.localEulerAngles.z, _minSpins);
    }

    private void StartSpin(float totalDegree, float startAngle)
    {
        StartCoroutine(SpinCoroutine(totalDegree, startAngle));
    }

    private IEnumerator SpinCoroutine(float totalDegree, float startAngle)
    {
        float elapsedTime = 0f;
        while (elapsedTime < _spinDuration)
        {
            elapsedTime += Time.deltaTime;
            float progressRate = elapsedTime / _spinDuration;
            float curveProgress = _spinCurve.Evaluate(progressRate);
            float currentAngle = Mathf.Lerp(0, totalDegree, curveProgress);
            _wheelContainer.localEulerAngles = new Vector3(0, 0, startAngle + currentAngle);

            yield return null;
        }

        _viewModel.OnSpinFinished(_wheelContainer.localEulerAngles.z);
    }
    #endregion

    #region UI 세팅 메서드 : 최초 위치 랜덤 + 돌림판 그리기 + 인트로 띄우기
    private void SetWheelStartAngel(float angle)
    {
        _wheelContainer.localEulerAngles = new Vector3(0, 0, angle);
    }

    private void SetWheelVisuals(float fortune, float misfortune, float misfortuneRotation)
    {
        _fortuneSlice.fillAmount = fortune;
        _misfortuneSlice.fillAmount = misfortune;
        _misfortuneSlice.transform.localEulerAngles = new Vector3(0, 0, misfortuneRotation);
    }

    private void SetIntroPanel(bool show)
    {
        if (show)
        {
            _introPanel.OpenUI();
        }
    }
    #endregion

    private void OnChallengeButtonClicked()
    {
        _challengePopup.OpenUI();
        _effectPopup.CloseUI();
    }

    private void OnConfirmButtonClicked()
    {
        _viewModel.ApplyDestiny();
        _challengePopup.ApplyChanges();
        _viewModel.CloseView();

        SceneLoader.Instance.StartLoadScene(SceneState.BattleScene);
    }
}
