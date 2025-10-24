using UnityEngine;
using UnityEngine.UI;
using System;

public class GachaUIPanel : BasePopUpUI 
{
    [Header("UI 참조")]
    [SerializeField] private Button backButton; 
    [SerializeField] private ContractListPanel contractListPanel;
    [SerializeField] private Button pullOneButton;        // 1회 뽑기 버튼
    [SerializeField] private Button pullTenButton;        // 10회 뽑기 버튼


    protected override void Awake()
    {
        base.Awake(); 

        backButton?.onClick.AddListener(OnBackButtonClicked);
        pullOneButton?.onClick.AddListener(OnPullOneClicked);
        pullTenButton?.onClick.AddListener(OnPullTenClicked);

        if (contractListPanel == null)
        {
            Debug.LogError("ContractPagesController가 GachaUIPanel에 연결되지 않았습니다!");
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable(); 
    }
    private async void OnPullOneClicked()
    {
        pullOneButton.interactable = false;
        Debug.Log("--- 1회 뽑기 버튼 클릭됨 ---");
        try
        {
            int resultId = await BackendManager.OneNormalGachaAsync();
            Debug.Log(resultId);
            PlayerDataManager.Instance.AddResource(ResourceType.Ticket, -1);
        } 
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
        pullOneButton.interactable = true;
    }

    private void OnPullTenClicked()
    {
        Debug.Log("--- 10회 뽑기 버튼 클릭됨 ---");
        //pullOneButton.interactable = false;
    //    try
    //    {
    //        int resultId = await BackendManager.OneNormalGachaAsync();
    //        Debug.Log(resultId);
    //        PlayerDataManager.Instance.AddResource(ResourceType.Ticket, -1);
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogException(ex);
    //    }
    //    pullOneButton.interactable = true;
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
