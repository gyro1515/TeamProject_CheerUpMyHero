using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // List<> 사용

/// <summary>
/// 이 버튼에 연결된 모든 UI 패널들을 껐다 켰다(토글)합니다.
/// 이 버튼 '자신'은 끄지 않습니다.
/// </summary>
public class ToggleUIController : MonoBehaviour
{
    [Header("토글할 UI 패널 목록")]
    [SerializeField] private List<GameObject> uiPanelsToToggle;

    [Header("버튼 아이콘 (선택 사항)")]
    [SerializeField] private Image buttonIcon; // 버튼의 아이콘

    private Button _toggleButton;
    private bool _isUiVisible = true; // 현재 UI가 보이는지 여부

    private void Awake()
    {
        _toggleButton = GetComponent<Button>();
        if (_toggleButton != null)
        {
            _toggleButton.onClick.AddListener(OnToggleClicked);
        }
    }

    private void Start()
    {
        // 게임 시작 시, 모든 UI를 '켜진' 상태로 강제 설정
        SetUIVisibility(true);
    }

    /// <summary>
    /// 카메라 버튼이 클릭되었을 때 호출됩니다.
    /// </summary>
    private void OnToggleClicked()
    {
        // 현재 상태를 뒤집습니다 (true -> false, false -> true)
        _isUiVisible = !_isUiVisible;

        // 변경된 상태로 모든 UI를 갱신합니다.
        SetUIVisibility(_isUiVisible);
    }

    // 모든 UI 패널의 활성화 상태를 강제로 설정합니다.
    private void SetUIVisibility(bool isVisible)
    {
        _isUiVisible = isVisible; // 현재 상태 저장

        // 연결된 모든 패널을 순회합니다.
        foreach (GameObject panel in uiPanelsToToggle)
        {
            if (panel != null)
            {
                panel.SetActive(isVisible);
            }
        }
    }
}