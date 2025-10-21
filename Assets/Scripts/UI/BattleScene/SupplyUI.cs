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
    [Header("식량 게이지")]
    [SerializeField] Image foodCurGaugeImage;
    [SerializeField] Image foodMaxGaugeImage;
    private void Awake()
    {
        //Debug.Log("SupplyUI초기화");
        //PlayerDataManager.Instance.OnResourceChangedEvent += OnResourceChanged;
    }
    private void OnEnable()
    {
        PlayerDataManager.Instance.OnResourceChangedEvent += OnResourceChanged;
    }

    private void OnDisable()
    {
        //**************** 현재 씬 종료시에도 호출되며 매니저 싱글톤 다시 생성되는 문제 발생, 구독 해제는 필요없음 ***********
        if(PlayerDataManager.Instance) PlayerDataManager.Instance.OnResourceChangedEvent -= OnResourceChanged;
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
        //Debug.Log("SupplyUI호출");

        int currentFood = PlayerDataManager.Instance.CurrentFood;
        int maxFood = PlayerDataManager.Instance.MaxFood;
        foodInfoText.text = $"{currentFood} / {maxFood}";
        int calMaxFood = PlayerDataManager.Instance.CalculatedMaxFood;
        foodCurGaugeImage.fillAmount = (float)currentFood / calMaxFood;
        //foodCurGaugeImage.fillAmount = maxFood != 0 ? (float)currentFood / maxFood : (float)currentFood / 1;
        foodMaxGaugeImage.fillAmount = (float)maxFood / calMaxFood;
    }

    private void UpdateSupplyLevelUI()
    {
        int supplyLevel = PlayerDataManager.Instance.SupplyLevel;
        int currentFood = PlayerDataManager.Instance.CurrentFood;
        int maxFood = PlayerDataManager.Instance.MaxFood;
        supplyLevelText.text = $"Supply Lv. {supplyLevel}";

        bool canLevelUp = PlayerDataManager.Instance.TryGetUpgradeCost(out int cost);

        if (canLevelUp)
        {
            supplyCostText.text = cost.ToString();

            if (currentFood >= cost && maxFood >= cost)
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
