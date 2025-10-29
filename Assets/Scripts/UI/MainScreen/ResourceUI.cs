using System.Collections.Generic;
using TMPro;
using UnityEngine;
// using YourNamespaceContainingResourceType; // ResourceType 사용 위해

public class ResourceUI : BaseUI
{
    [Header("자원 UI 텍스트 연결")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI woodText;
    [SerializeField] private TextMeshProUGUI ironText;
    [SerializeField] private TextMeshProUGUI magicStoneText;
    [SerializeField] private TextMeshProUGUI bmText;
    [SerializeField] private TextMeshProUGUI ticketText;

    protected Dictionary<ResourceType, TextMeshProUGUI> _resourceTexts = new();

    protected virtual void Awake()
    {
        // 기존 딕셔너리 초기화
        _resourceTexts.Clear(); // Awake가 여러 번 호출될 경우 대비
        if (goldText) _resourceTexts.Add(ResourceType.Gold, goldText);
        if (woodText) _resourceTexts.Add(ResourceType.Wood, woodText);
        if (ironText) _resourceTexts.Add(ResourceType.Iron, ironText);
        if (magicStoneText) _resourceTexts.Add(ResourceType.MagicStone, magicStoneText);

        if (bmText) _resourceTexts.Add(ResourceType.Bm, bmText);
        if (ticketText) _resourceTexts.Add(ResourceType.Ticket, ticketText);

      
    }
    protected virtual void OnEnable()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnResourceChangedEvent += OnResourceUpdated;
            UpdateAllResourceUI(); // 초기 값 설정
        }
        else
        {
            Debug.LogError("PlayerDataManager 인스턴스를 찾을 수 없습니다! (ResourceUI Awake)");
        }
    }


    protected virtual void OnDisable()
    {
        // 이벤트 구독 해지 (PlayerDataManager 인스턴스 확인 후)
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnResourceChangedEvent -= OnResourceUpdated;
        }
    }

    protected virtual void OnResourceUpdated(ResourceType type, int newAmount)
    {
        UpdateResourceUI(type);
    }

    public virtual void UpdateResourceUI(ResourceType type)
    {
        if (_resourceTexts.TryGetValue(type, out TextMeshProUGUI textComponent))
        {
            // PlayerDataManager 인스턴스 확인
            if (PlayerDataManager.Instance != null)
            {
                int amount = PlayerDataManager.Instance.GetResourceAmount(type);
                textComponent.text = amount.ToString();
            }
        }
    }

    public virtual void UpdateAllResourceUI()
    {
        if (PlayerDataManager.Instance == null) return;

        foreach (var type in _resourceTexts.Keys)
        {
            UpdateResourceUI(type);
        }
    }
}