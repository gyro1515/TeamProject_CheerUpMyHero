using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceUI : BaseUI
{
    [Header("자원 UI 텍스트 연결")]

    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI woodText;
    [SerializeField] private TextMeshProUGUI ironText;
    [SerializeField] private TextMeshProUGUI foodText;
    [SerializeField] private TextMeshProUGUI magicStoneText;


    private Dictionary<ResourceType, TextMeshProUGUI> _resourceTexts = new();

    private void Awake()
    {

        _resourceTexts.Add(ResourceType.Gold, goldText);
        _resourceTexts.Add(ResourceType.Wood, woodText);
        _resourceTexts.Add(ResourceType.Iron, ironText);
        _resourceTexts.Add(ResourceType.Food, foodText);
        _resourceTexts.Add(ResourceType.MagicStone, magicStoneText);

         ResourceManager.Instance.OnResourceChangedEvent += OnResourceUpdated;

            // 게임 시작 시 초기 자원 값을 UI에 한 번 반영
            UpdateAllResourceUI();
    }

    private void OnDestroy()
    {
        //ResourceManager.Instance.OnResourceChangedEvent -= OnResourceUpdated;       
    }

    // ResourceManager에서 자원 변경 이벤트가 발생하면 자동으로 호출됩니다.
    private void OnResourceUpdated(ResourceType type, int newAmount)
    {
        UpdateResourceUI(type);
    }

    // 특정 자원의 UI 텍스트를 업데이트
    private void UpdateResourceUI(ResourceType type)
    {
        if (_resourceTexts.TryGetValue(type, out TextMeshProUGUI textComponent))
        {
            int amount = ResourceManager.Instance.GetResourceAmount(type);
            textComponent.text = amount.ToString();
        }
    }

    // 모든 자원의 UI를 한꺼번에 업데이트하는 보조 메서드 
    //이후에는 ResourceManager.AddResource 메서드가 호출될 때마다 업데이트
    private void UpdateAllResourceUI()
    {
        foreach (var type in _resourceTexts.Keys)
        {
            UpdateResourceUI(type);
        }
    }
}