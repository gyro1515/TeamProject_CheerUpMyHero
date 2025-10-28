using UnityEngine;
using UnityEngine.UI;

public class SecondStartGroup : MonoBehaviour
{
    [Header("버튼 참조")]
    [SerializeField] private Button googleLoginButton;
    [SerializeField] private Button appleLoginButton;
    [SerializeField] private Button guestLoginButton;

    private StartUI _startUIController;

    /// 상위 컨트롤러(StartUI)가 호출하여 초기 설정을 합니다.
    public void Initialize(StartUI controller)
    {
        _startUIController = controller;
    }

    private void Awake()
    {
        // 각 로그인 버튼에 임시 리스너 연결
        // 실제 백엔드 로그인 로직으로 교체 필요
        googleLoginButton?.onClick.AddListener(OnLoginSuccess);
        appleLoginButton?.onClick.AddListener(OnLoginSuccess);
        guestLoginButton?.onClick.AddListener(OnLoginSuccess);
    }

    /// 로그인 버튼 중 하나를 눌렀을 때 호출됩니다 (현재는 성공으로 간주).
    private void OnLoginSuccess()
    {
        Debug.Log("로그인 버튼 클릭됨 (성공으로 간주). 스토리 씬으로 이동 요청.");

        // 상위 컨트롤러(StartUI)에게 다음 단계(스토리 패널)로 넘어가라고 알림
        if (_startUIController != null)
        {
            _startUIController.OnLoginSuccess();
        }
        else
        {
            Debug.LogError("StartUI 컨트롤러가 연결되지 않았습니다!");
        }
    }

    // OnDestroy에서 리스너 제거 (안전 코드)
    private void OnDestroy()
    {
        googleLoginButton?.onClick.RemoveListener(OnLoginSuccess);
        appleLoginButton?.onClick.RemoveListener(OnLoginSuccess);
        guestLoginButton?.onClick.RemoveListener(OnLoginSuccess);
    }
}