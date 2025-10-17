using UnityEngine;
using UnityEngine.UI;

// 1. MonoBehaviour 대신 BasePopUpUI를 상속받습니다.
public class AdCooldownPopup : BasePopUpUI
{
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private int _tileX, _tileY;
    private MainScreenBuildingController _controller;

    protected override void Awake()
    {
        base.Awake(); 
        yesButton.onClick.AddListener(OnYesButtonClicked);
        noButton.onClick.AddListener(OnNoButtonClicked);
    }

    public void OpenPopup(int x, int y, MainScreenBuildingController controller)
    {
        _tileX = x;
        _tileY = y;
        _controller = controller;

        base.OpenUI();
    }

    private void OnYesButtonClicked()
    {
        if (_controller != null)
        {
            _controller.RequestAdForCooldownReduction(_tileX, _tileY);
        }
        base.CloseUI();
    }

    private void OnNoButtonClicked()
    {
        base.CloseUI();
    }
}