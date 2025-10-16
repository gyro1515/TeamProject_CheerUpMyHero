using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode
{
    Normal,
    Hard
}

public class UIStageDestinyRoullette : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private Transform _wheelContainer;
    [SerializeField] private Image _fortuneSlice;
    [SerializeField] private Image _misfortuneSlice;
    [SerializeField] private Button _startSpinButton;

    [Header("회전 관련 수치")]
    [SerializeField] private float _spinDuration = 1.0f;
    [SerializeField] private int _minSpins = 5;
    [SerializeField] private AnimationCurve _spinCurve;

    [Header("임시값")]
    [SerializeField] private GameMode _gameMode = GameMode.Normal;
    [SerializeField] private int _mainStage = 2;
    [SerializeField] private int _subStage = 3;

    private float fortuneProbability;
    private bool isSpinning = false;
    private (int, int) _stage;

    private const float NormalBaseProbability = 0.50f;
    private const float NormalMinProbability = 0.32f;
    private const float HardBaseProbability = 0.30f;
    private const float HardMinProbability = 0.12f;

    private void Awake()
    {
        _startSpinButton.onClick.AddListener(OnStartSpinButtonClicked);
    }

    private void OnEnable()
    {
        // _stage = PlayerDataManager.Instance.SelectedStageIdx;
        _stage = (_mainStage, _subStage); 
        SetWheel(_gameMode);
    }

    #region  값 설정 메서드 : 확률 계산 + 돌림판 파이 설정
    private void SetWheel(GameMode gameMode)
    {
        fortuneProbability = SetProbability(gameMode, _stage);
        SetWheelVisual();

        float randomStartAngle = Random.Range(0f, 360f);
        _wheelContainer.localEulerAngles = new Vector3(0, 0, randomStartAngle);
    }

    private void SetWheelVisual()
    {
        _fortuneSlice.fillAmount = fortuneProbability;
        _misfortuneSlice.fillAmount = 1.0f - fortuneProbability;
        // _misfortuneSlice.transform.localEulerAngles = new Vector3(0, 0, fortuneProbability * 360f);
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
            Debug.Log("확률 정하는 로직에 문제 있어요");
        }

        float penalty = stageNum * 0.02f;
        float finalProbability = baseProbability - penalty;

        return Mathf.Max(finalProbability, minProbability);
    }
    #endregion

    #region 돌림판 돌리는 메서드
    public void StartSpin()
    {
        if (!isSpinning)
        {
            StartCoroutine(SpinCoroutine());
        }
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
        _startSpinButton.interactable = true;
    }

    private void CheckResult()
    {
        float finalAngle = _wheelContainer.localEulerAngles.z;
        float arrowPoint = (360 - finalAngle) % 360;

        float fortuneAngleRange = fortuneProbability * 360;

        if (arrowPoint <= fortuneAngleRange)
        {
            Debug.Log($"멈춘 각도 : {finalAngle}, 결과 : 행운");
        }
        else
        {
            Debug.Log($"멈춘 각도 : {finalAngle}, 결과 : 불행");
        }
    }
    #endregion

    private void OnStartSpinButtonClicked()
    {
        StartSpin();
    }
}
