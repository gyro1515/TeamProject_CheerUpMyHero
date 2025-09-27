using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] GameObject playerGO;
    [Header("카메라 설정")]
    [SerializeField] private float _cameraMoveSpeed = 5f; 

    private Transform _playerTransform;
    // 자동 추적 관련 변수
    private const float IDLE_THRESHOLD = 3f; // 3초간 조작 없으면 자동 모드로 전환
    private float _idleTimer = 0f;
    private bool _isAutoFollowing = false;
    private bool _hasInitializedCamera = false;

    //private void Start()
    //{
    //    playerGO = GameManager.Instance.Player.gameObject;
    //}
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
    }

    private void OnEnable()
    {
        PlayerController.OnPlayerAction += ResetIdleTimer;
    }
    void Update()
    {
        // 조이스틱 직접 입력 감지
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            ResetIdleTimer();
        }
        else
        {
            _idleTimer += Time.deltaTime;
        }

        if (_idleTimer >= IDLE_THRESHOLD && !_isAutoFollowing)
        {
            _isAutoFollowing = true;
            Debug.Log("3초간 입력 없음: 자동 추적 모드로 전환합니다.");
        }
    }
    private void OnDisable()
    {
        PlayerController.OnPlayerAction -= ResetIdleTimer;
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


        Vector3 currentCamPos = transform.position;
        Vector3 targetCamPos = new Vector3(currentTarget.position.x, currentCamPos.y, currentCamPos.z);

        //transform.position = targetCamPos; 
        if (!_hasInitializedCamera)
        {
            // 배틀 시작 후 첫 프레임은 스냅 이동
            transform.position = targetCamPos;
            _hasInitializedCamera = true;
        }
        else
        {
            // 이후에는 부드럽게 이동
            transform.position = Vector3.Lerp(currentCamPos, targetCamPos, Time.deltaTime * _cameraMoveSpeed);
        }
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
        List<BaseCharacter> playerUnits = UnitManager.Instance.PlayerUnitList;
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
}
//이전꺼 카메라 컨트롤러
//[SerializeField] GameObject playerGO;

//private void Start()
//{
//    playerGO = GameManager.Instance.Player.gameObject;
//}
//private void FixedUpdate()
//{
//    if (playerGO == null) return;
//    Vector3 camPos = gameObject.transform.position;
//    camPos.x = playerGO.transform.position.x;
//    gameObject.transform.position = camPos;
//}
//}
