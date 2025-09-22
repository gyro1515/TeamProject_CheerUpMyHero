using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SupplyUI : BaseUI
{
    [SerializeField] private Button supplyLvUpButton;
    [SerializeField] private TextMeshProUGUI foodInfoText;
    [SerializeField] private TextMeshProUGUI supplyLevelText;
    [SerializeField] private TextMeshProUGUI supplyCostText;

    [SerializeField] private Color affordableColor = Color.black; 
    [SerializeField] private Color unaffordableColor = Color.red;
    private void OnEnable()
    {
        PlayerDataManager.Instance.OnResourceChangedEvent += OnResourceChanged;
    }

    private void OnDisable()
    {
        PlayerDataManager.Instance.OnResourceChangedEvent -= OnResourceChanged;
    }

    private void Start()
    {
        supplyLvUpButton.onClick.AddListener(OnSupplyLvUpClicked);

        UpdateFoodUI();
        UpdateSupplyLevelUI();
    }

    private void OnResourceChanged(ResourceType type, int newAmount)
    {
        if (type == ResourceType.Food)
        {
            UpdateFoodUI();
        }
        UpdateSupplyLevelUI();
    }

    private void UpdateFoodUI()
    {
        int currentFood = PlayerDataManager.Instance.CurrentFood;
        int maxFood = PlayerDataManager.Instance.MaxFood;
        foodInfoText.text = $"{currentFood} / {maxFood}";
    }

    private void UpdateSupplyLevelUI()
    {
        int supplyLevel = PlayerDataManager.Instance.SupplyLevel;
        int currentFood = PlayerDataManager.Instance.CurrentFood;
        supplyLevelText.text = $"Supply Lv. {supplyLevel}";

        bool canLevelUp = PlayerDataManager.Instance.TryGetUpgradeCost(out int cost);

        if (canLevelUp)
        {
            supplyCostText.text = cost.ToString();

            if (currentFood >= cost)
            {
                supplyCostText.color = affordableColor;
                supplyLvUpButton.interactable = true;
            }
            else
            {
                supplyCostText.color = unaffordableColor;
                supplyLvUpButton.interactable = false;
            }
        }
        else // 최대 레벨일 경우
        {
            supplyCostText.text = "MAX";
            supplyCostText.color = affordableColor;
            supplyLvUpButton.interactable = false;
        }
    }

    private void OnSupplyLvUpClicked()
    {
        PlayerDataManager.Instance.UpgradeSupplyLevel();

        // 레벨업 시도 후 UI를 즉시 갱신
        UpdateFoodUI();
        UpdateSupplyLevelUI();
    }
}
