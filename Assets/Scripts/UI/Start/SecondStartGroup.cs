using UnityEngine;
using UnityEngine.UI;

public class SecondStartGroup : MonoBehaviour
{
    [Header("버튼 참조")]
    [SerializeField] private Button googleLoginButton;
    [SerializeField] private Button appleLoginButton;
    [SerializeField] private Button guestLoginButton;

    [SerializeField] private LoginConfirmPopup loginConfirmPopup;
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
        googleLoginButton?.onClick.AddListener(OnLoginClicked);
        appleLoginButton?.onClick.AddListener(OnLoginClicked);
        guestLoginButton?.onClick.AddListener(OnGuestLoginButtonClicked);
    }
    private void OnLoginClicked()
    {
        Debug.Log("로그인 버튼 클릭됨. '추후 업데이트' 팝업 표시.");

        if (loginConfirmPopup != null)
        {
            loginConfirmPopup.Show();
        }
        else
        {
            Debug.LogError("LoginConfirmPopup이 SecondStartGroup에 연결되지 않았습니다!");
            _startUIController?.OnLoginSuccess();
        }
    }
    private void OnGuestLoginButtonClicked()
    {
        _startUIController?.OnLoginSuccess();
    }
    // OnDestroy에서 리스너 제거 (안전 코드)
    private void OnDestroy()
    {
        googleLoginButton?.onClick.RemoveListener(OnLoginClicked);
        appleLoginButton?.onClick.RemoveListener(OnLoginClicked);
        guestLoginButton?.onClick.RemoveListener(OnLoginClicked);
    }
}