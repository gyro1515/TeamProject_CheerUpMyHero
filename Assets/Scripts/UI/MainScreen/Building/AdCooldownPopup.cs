using UnityEngine;
using UnityEngine.UI;

public class AdCooldownPopup : BasePopUpUI
{
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private int _tileX, _tileY;
    private MainScreenBuildingController _controller;

    protected override void Awake()
    {
        yesButton.onClick.AddListener(OnYesButtonClicked);
        noButton.onClick.AddListener(OnNoButtonClicked);
    }

    public void Show(int x, int y, MainScreenBuildingController controller)
    {
        _tileX = x;
        _tileY = y;
        _controller = controller;
        gameObject.SetActive(true);
    }

    private void OnYesButtonClicked()
    {
        if (_controller != null)
        {
           // _controller.RequestAdForCooldownReduction(_tileX, _tileY);
        }
        gameObject.SetActive(false);
    }

    private void OnNoButtonClicked()
    {
        gameObject.SetActive(false);
    }
}