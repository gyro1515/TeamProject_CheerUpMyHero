using UnityEngine;
using TMPro;

public class ResourcePanelUI : BaseUI
{
    [SerializeField] private TextMeshProUGUI goldCountText;
    [SerializeField] private TextMeshProUGUI bmCountText;
    [SerializeField] private TextMeshProUGUI ticketCountText;

    private void OnEnable()
    {
        PlayerDataManager.Instance.OnResourceChangedEvent += UpdateResourceText;

        UpdateAllResources();
    }

    private void OnDisable()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnResourceChangedEvent -= UpdateResourceText;
        }
    }

    // 모든 자원 UI를 한 번에 업데이트하는 함수
    private void UpdateAllResources()
    {
        goldCountText.text = PlayerDataManager.Instance.GetResourceAmount(ResourceType.Gold).ToString();
        bmCountText.text = PlayerDataManager.Instance.GetResourceAmount(ResourceType.Bm).ToString();
        ticketCountText.text = PlayerDataManager.Instance.GetResourceAmount(ResourceType.Ticket).ToString();
    }

    // 특정 자원이 변경되었을 때 호출되는 함수
    private void UpdateResourceText(ResourceType type, int newAmount)
    {
        switch (type)
        {
            case ResourceType.Gold:
                goldCountText.text = newAmount.ToString();
                break;
            case ResourceType.Bm:
                bmCountText.text = newAmount.ToString();
                break;
            case ResourceType.Ticket:
                ticketCountText.text = newAmount.ToString();
                break;
        }
    }
}