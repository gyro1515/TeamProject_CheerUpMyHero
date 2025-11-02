using System;
using UnityEngine;

public class DestinyRoulleteViewModel
{
    #region 이벤트 시스템
    public event Action<bool> OnIntroStateChanged;              // 인트로 상태 변경 이벤트 -> 끝나면 스핀 시작
    public event Action<float, float, float> OnWheelVisualSet;  // 돌림판 다 그려졌는지 여부 이벤트
    public event Action<float> OnWheelStartAngleSet;            // 돌림판 시작 각도 계산되었는지 여부 이벤트
    public event Action<float, float> OnStartSpin;              // 돌림판 회전 시작 이벤트
    public event Action<StageDestinyData> OnResultSet;          // 운명 결과 팝업 표시 이벤트
    
    // 버튼 활성화 이벤트
    public event Action<bool> OnConfirmStateChanged;            // 결정하기 버튼
    public event Action<bool> OnChallengeStateChanged;          // 도전하기 버튼
    public event Action<bool> OnSpinTestChanged;                // 테스트 -> 삭제 예정
    
    public event Action OnCloseView;                             // 전체 UI 닫기 이벤트
    #endregion

    private DestinyModel _model;
    private StageDestinyData _selectedDestiny;
    private float _fortuneProbability;
    private bool _isSpinning = false;
    private GameMode _gameMode;
    private (int, int) _stage;

    private (int, int) OneNine = (0, 8);

    public DestinyRoulleteViewModel(DestinyModel model)
    {
        _model = model;
    }

    public void OnviewEnabled(GameMode mode, (int, int) stage)
    {
        _gameMode = mode;
        _stage = stage;
        _isSpinning = false;
        _selectedDestiny = null;

        OnConfirmStateChanged?.Invoke(false);
        OnChallengeStateChanged?.Invoke(false);
        OnSpinTestChanged?.Invoke(true);

        // 행운 확률 100% 만드는 로직 -> 나중에 필요하면 넣기
        //if (_stage.Item1 == OneNine.Item1 && _stage.Item2 == OneNine.Item2)
        //{
        //    _fortuneProbability = 1f;
        //    OnWheelVisualSet?.Invoke(1.0f, 0.0f, 0.0f);

        //    // 랜덤 시작 각도 만들고 반영하기
        //    float randomStartAngle1 = UnityEngine.Random.Range(0f, 360f);
        //    OnWheelStartAngleSet?.Invoke(randomStartAngle1);

        //    // 인트로 시작 신호 보내기
        //    OnIntroStateChanged?.Invoke(true);

        //    return;
        //}


        // 행운 확률 계산 + 확률 적용해서 돌림판 세팅
        _fortuneProbability = _model.GetFortuneProbability(_gameMode, _stage);
        OnWheelVisualSet?.Invoke(_fortuneProbability, 1.0f - _fortuneProbability, _fortuneProbability * 360);

        // 랜덤 시작 각도 만들고 반영하기
        float randomStartAngle = UnityEngine.Random.Range(0f, 360f);
        OnWheelStartAngleSet?.Invoke(randomStartAngle);

        // 인트로 시작 신호 보내기
        OnIntroStateChanged?.Invoke(true);
        
    }

    // 인트로 코루틴 끝 -> OnIntroFinished 호출 -> 돌림판 코루틴 호출
    public void OnIntroFinished(float startAngel, int minSpins)
    {
        if (_isSpinning) return;
        _isSpinning = true;
        OnSpinTestChanged?.Invoke(false);

        float totalDegree = 360f * minSpins + UnityEngine.Random.Range(0f, 360f);
        OnStartSpin?.Invoke(totalDegree, startAngel);
    }

    // 돌림판 코루틴 끝 -> OnSpinFinished 호출 -> 랜덤 운명 뽑아서 결과창 띄우기 + 결정하기 및 도전하기 버튼 활성화
    public void OnSpinFinished(float finalAngle)
    {
        _isSpinning = false;

        // 특정 라운드에서 운명 효과 고정하는 로직 -> 필요하면 열기
        //if (_stage.Item1 == OneNine.Item1 && _stage.Item2 == OneNine.Item2)
        //{
        //    const int heroTimerFortuneId = 9010003;
        //    _selectedDestiny = _model.GetSpecificDestiny(heroTimerFortuneId);
        //    if (_selectedDestiny != null)
        //    {
        //        OnResultSet?.Invoke(_selectedDestiny);
        //    }
        //    else Debug.Log("운명 뽑아오는 로직 오류 있어요");

        //    OnConfirmStateChanged?.Invoke(true);
        //    OnChallengeStateChanged?.Invoke(true);
        //    OnSpinTestChanged?.Invoke(true);
        //    return;
        //}
        
        // 화살표 위치 고려하여 결과 산출 -> 행운이냐 불행이냐
        float arrowPoint = finalAngle % 360;
        float fortuneAngleRange = _fortuneProbability * 360;
        DestinyType destinyType = arrowPoint <= fortuneAngleRange ? DestinyType.Fortune : DestinyType.Misfortune;
        // 랜덤 운명 뽑아서 결과창 띄움
        _selectedDestiny = _model.GetRandomDestiny(destinyType);
        
        if (_selectedDestiny != null)
        {
            OnResultSet?.Invoke(_selectedDestiny);
        }
        else Debug.Log("운명 뽑아오는 로직 오류 있어요");

        OnConfirmStateChanged?.Invoke(true);
        OnChallengeStateChanged?.Invoke(true);
        OnSpinTestChanged?.Invoke(true);
    }

    // 랜덤 운명 뽑아서 결과창 띄움 -> 운명 적용
    public void ApplyDestiny()
    {
        if (_selectedDestiny == null)
        {
            Debug.Log("추첨 유물 null임 운명 추첨 로직 문제 있어요");
            return;
        }

        _model.ApplyDestiny(_selectedDestiny);  // 추첨된 운명 적용
    }

    //  전체 UI 닫기
    public void CloseView()
    {
        OnCloseView?.Invoke();
    }
}
