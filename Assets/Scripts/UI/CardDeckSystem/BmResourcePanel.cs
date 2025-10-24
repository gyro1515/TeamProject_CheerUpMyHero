using UnityEngine;
using TMPro;

public class ResourcePanelUI : BaseUI
{
    [SerializeField] private TextMeshProUGUI goldCountText;
    [SerializeField] private TextMeshProUGUI bmCountText;
    [SerializeField] private TextMeshProUGUI ticketCountText;
    private void Start()
    {
        UpdateAllResources();
    }
    private void OnEnable()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnResourceChangedEvent += UpdateResourceText;
        }
    }

    private void OnDisable()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnResourceChangedEvent -= UpdateResourceText;
        }
    }

    private void UpdateAllResources()
    {
        if (PlayerDataManager.Instance == null) return;

        if (goldCountText != null)
            goldCountText.text = PlayerDataManager.Instance.GetResourceAmount(ResourceType.Gold).ToString();
        if (bmCountText != null)
            bmCountText.text = PlayerDataManager.Instance.GetResourceAmount(ResourceType.Bm).ToString();
        if (ticketCountText != null)
            ticketCountText.text = PlayerDataManager.Instance.GetResourceAmount(ResourceType.Ticket).ToString();

        Debug.Log("[ResourcePanel] 모든 자원 표시 업데이트 완료.");
    }

    private void UpdateResourceText(ResourceType type, int newAmount)
    {
        switch (type)
        {
            case ResourceType.Gold:
                if (goldCountText != null) goldCountText.text = newAmount.ToString();
                break;
            case ResourceType.Bm:
                if (bmCountText != null) bmCountText.text = newAmount.ToString();
                break;
            case ResourceType.Ticket:
                if (ticketCountText != null) ticketCountText.text = newAmount.ToString();
                break;
        }
    }
}