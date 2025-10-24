using UnityEngine;
using UnityEngine.UI;

public class GachaUIPanel : BasePopUpUI // BasePopUpUI 상속
{
    [Header("UI 참조")]
    [SerializeField] private Button backButton; 
    [SerializeField] private ContractListPanel contractListPanel;


    protected override void Awake()
    {
        base.Awake(); 

        backButton?.onClick.AddListener(OnBackButtonClicked);

        if (contractListPanel == null)
        {
            Debug.LogError("ContractPagesController가 GachaUIPanel에 연결되지 않았습니다!");
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable(); 
    }

    private void OnBackButtonClicked()
    {
        CloseUI();
    }

    public override void OnBackPressed()
    {
        OnBackButtonClicked();
    }
}
