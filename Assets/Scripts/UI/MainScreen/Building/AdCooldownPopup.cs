using UnityEngine;
using UnityEngine.UI;

public class AdCooldownPopup : BasePopUpUI
{
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private BuildingTile _sourceTile;
    private BuildingTile _destinationTile;
    private MainScreenBuildingController _controller;
    protected override void Awake()
    {
        base.Awake(); 
        yesButton.onClick.AddListener(OnYesButtonClicked);
        noButton.onClick.AddListener(OnNoButtonClicked);
    }

    public void OpenPopup(BuildingTile source, BuildingTile destination, MainScreenBuildingController controller)
    {
        _sourceTile = source;
        _destinationTile = destination;
        _controller = controller;

        base.OpenUI();
    }

    private void OnYesButtonClicked()
    {
        if (_controller != null)
        {
            _controller.ConfirmAdAndMove(_sourceTile, _destinationTile);
        }
        base.CloseUI();
    }

    private void OnNoButtonClicked()
    {
        base.CloseUI();
    }
}