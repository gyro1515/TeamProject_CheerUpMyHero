using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class DestroyConfirmPopup : BasePopUpUI
{
    [SerializeField] private TextMeshProUGUI buildingInfoText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
   
    [Header("환급 자원 텍스트")]
    [SerializeField] private TextMeshProUGUI goldRefundText;
    [SerializeField] private TextMeshProUGUI woodRefundText;
    [SerializeField] private TextMeshProUGUI ironRefundText;
    [SerializeField] private TextMeshProUGUI magicStoneRefundText;

    private BuildingTile _targetTile;
    private MainScreenBuildingController _controller;
    private Dictionary<ResourceType, TextMeshProUGUI> refundTexts;

    protected override void Awake()
    {
        base.Awake();
        yesButton.onClick.AddListener(() => { OnYesButtonClicked().Forget(); });
        noButton.onClick.AddListener(OnNoButtonClicked);

        InitializeDictionary();
    }

    public void OpenPopup(BuildingTile tile, string buildingInfo, Dictionary<ResourceType, int> refundAmounts, MainScreenBuildingController controller)
    {
        if (refundTexts == null)
        {
            InitializeDictionary();
        }

        _targetTile = tile;
        _controller = controller;
        buildingInfoText.text = buildingInfo;

        foreach (var textUI in refundTexts.Values)
        {
            textUI.gameObject.SetActive(false);
        }

        foreach (var pair in refundAmounts)
        {
            if (refundTexts.TryGetValue(pair.Key, out TextMeshProUGUI textUI))
            {
                string resourceName = ConstructionUpgradePanel.GetResourceNameInKorean(pair.Key);
                textUI.text = $"{resourceName} + {pair.Value} 반환";
                textUI.gameObject.SetActive(true);
            }
        }
        base.OpenUI();
    }

    private void InitializeDictionary()
    {
        if (refundTexts != null) return;

        refundTexts = new Dictionary<ResourceType, TextMeshProUGUI>
    {
        { ResourceType.Gold, goldRefundText },
        { ResourceType.Wood, woodRefundText },
        { ResourceType.Iron, ironRefundText },
        { ResourceType.MagicStone, magicStoneRefundText }
    };
    }
    private async UniTaskVoid OnYesButtonClicked()
    {
        yesButton.interactable = false;
        try
        {
            if (_controller != null && _targetTile != null)
            {
                await _controller.ConfirmDestruction(_targetTile);
            }
        }
        catch (Exception ex) 
        {
            Debug.LogException(ex);
            Debug.LogWarning("에러 팝업: 에러가 나서 파괴 명령이 실패했습니다.");
        }
        finally
        {
            yesButton.interactable = true;  
            base.CloseUI();
        }
    }

    private void OnNoButtonClicked()
    {
        base.CloseUI();
    }
}