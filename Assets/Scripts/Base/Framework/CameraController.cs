using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] GameObject playerGO;
    [Header("카메라 설정")]
    [SerializeField] private float _cameraMoveSpeed = 5f;
    [SerializeField] GameObject FXRainGO;

    [Header("카메라 흔들림 설정")]
    [SerializeField] private float _shakeDuration = 2f;
    //[SerializeField] private float _shakeMagnitude = 0.1f;
    private float _shakeMagnitude = 0.1f;

    private Transform _playerTransform;
    // 자동 추적 관련 변수
    private const float IDLE_THRESHOLD = 3f; // 3초간 조작 없으면 자동 모드로 전환
    private float _idleTimer = 0f;
    private bool _isAutoFollowing = false;
    private bool _hasInitializedCamera = false;
    Vector3 targetCamPos;
    IEventSubscriber<HeroSpawnEvent> heroSpawnEventSub;

    private IEventSubscriber<StartWaveEvent> _waveStartSubscriber;
    private Vector3 _shakeOffset = Vector3.zero;

    private Vector3 _cleanCameraPosition;

    private void Awake()
    {
        heroSpawnEventSub = EventManager.GetSubscriber<HeroSpawnEvent>();
        _waveStartSubscriber = EventManager.GetSubscriber<StartWaveEvent>();

        _cleanCameraPosition = transform.position;
    }
    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            playerGO = GameManager.Instance.Player.gameObject;

            _playerTransform = playerGO.transform;
        }
        else
        {
            Debug.LogError("플레이어를 찾을 수 없어 카메라가 동작할 수 없습니다.");
        }

        //TODO: 운명데이터에 비 추가해야 함
        if(PlayerDataManager.Instance.currentDestiny.idNumber == 09020001) FXRainGO.SetActive(true);
    }

    private void OnEnable()
    {
        PlayerController.OnPlayerAction += ResetIdleTimer;
        heroSpawnEventSub.Subscribe(SpawnHero);

        _waveStartSubscriber.Subscribe(OnWaveStarted);
    }
    void Update()
    {
        // 조이스틱 직접 입력 감지 -> 251017: 왜 안지우셨죠?????
        /*if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            ResetIdleTimer();
        }
        else
        {
            _idleTimer += Time.deltaTime;
        }*/
        _idleTimer += Time.deltaTime;

        if (_idleTimer >= IDLE_THRESHOLD && !_isAutoFollowing)
        {
            _isAutoFollowing = true;
            Debug.Log("3초간 입력 없음: 자동 추적 모드로 전환합니다.");
        }
    }
    private void OnDisable()
    {
        PlayerController.OnPlayerAction -= ResetIdleTimer;
        heroSpawnEventSub.Unsubscribe(SpawnHero);

        _waveStartSubscriber.Unsubscribe(OnWaveStarted);
    }
    //private void FixedUpdate()
    //{
    //    if (playerGO == null) return;
    //    Vector3 camPos = gameObject.transform.position;
    //    camPos.x = playerGO.transform.position.x;
    //    gameObject.transform.position = camPos;
    //}

    private void LateUpdate()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsBattleStarted)
        {
            return;
        }

        if (_playerTransform == null) return;

        Transform currentTarget = _playerTransform;

        if (_isAutoFollowing)
        {
            Transform frontUnit = FindFrontMostUnit();
            if (frontUnit != null)
            {
                currentTarget = frontUnit;
            }
        }

        targetCamPos = new Vector3(currentTarget.position.x, _cleanCameraPosition.y, _cleanCameraPosition.z);

        //transform.position = targetCamPos; 
        //Vector3 finalPosition; // 251106: 사용 안하는 거 같아서 주석 처리
        if (!_hasInitializedCamera)
        {
            // 배틀 시작 후 첫 프레임은 스냅 이동
            _cleanCameraPosition = targetCamPos;
            _hasInitializedCamera = true;
        }
        else
        {
            // 이후에는 부드럽게 이동
            _cleanCameraPosition = Vector3.Lerp(_cleanCameraPosition, targetCamPos, Time.unscaledDeltaTime * _cameraMoveSpeed);
        }

        transform.position = _cleanCameraPosition + _shakeOffset;
    }
    void SpawnHero(HeroSpawnEvent heroSpawnEvent)
    {
        // 타겟 캠포스에 용사 소환
        //Debug.Log($"테스트로 용사1을 카메라 최종 추적 위치에 소환");
        PoolType heroPoolType = heroSpawnEvent.selectedHero.poolType;
        GameObject heroGO = ObjectPoolManager.Instance.Get(heroPoolType);
        Vector3 spawnPos = targetCamPos;
        spawnPos.y += UnityEngine.Random.Range(20, 80) / 100f;
        spawnPos.z = 0f;
        heroGO.transform.position = spawnPos;
    }
    // 플레이어가 행동했을 때 호출될 함수
    private void ResetIdleTimer()
    {
        _idleTimer = 0f;
        if (_isAutoFollowing)
        {
            _isAutoFollowing = false;
            Debug.Log("플레이어 조작 감지: 플레이어 추적 모드로 복귀합니다.");
        }
    }

    // 가장 오른쪽에 있는 아군 유닛을 찾는 함수
    private Transform FindFrontMostUnit()
    {
        List<BaseCharacter> playerUnits = UnitManager.PlayerUnitList;
        if (playerUnits == null || playerUnits.Count == 0) return null;

        Transform frontMostUnit = null;

        for (int i = 0; i < playerUnits.Count; i++)
        {
            if (playerUnits[i] == null) continue;               // null 체크
            if (playerUnits[i].gameObject == null) continue;    // Destroy 되었는지 체크

            Transform unitTransform = playerUnits[i].transform;
            if (frontMostUnit == null || unitTransform.position.x > frontMostUnit.position.x)
            {
                frontMostUnit = unitTransform;
            }
        }

        return frontMostUnit;
    }

    // 카메라 흔들림 로직 -> 이벤트 바인딩해서 코루틴 호출용
    private void OnWaveStarted(StartWaveEvent e)
    {
        StartCoroutine(CameraShakeCoroutine(_shakeDuration, _shakeMagnitude));
    }

    // 카메라 흔들리는 코루틴
    private IEnumerator CameraShakeCoroutine(float duration, float magnitude)
    {
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            _shakeOffset = new Vector3(x, y, 0);

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        _shakeOffset = Vector3.zero;
    }
}
