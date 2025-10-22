using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using Unity.Services.Economy;
using Unity.Services.Economy.Model;
using UnityEngine;
using UnityEngine.UI;

public class TestGachaLoad : MonoBehaviour
{

    private string currentWriteLock = "";

    private string goldString = "";

    [SerializeField] Button gachaButton;
    [SerializeField] Button testTicketPlusButton;


    [SerializeField] TMP_Text ticketBalance;

    private void OnEnable()
    {
        gachaButton.onClick.AddListener(OnGachaButton);
        testTicketPlusButton.onClick.AddListener(TestPlusTicket);
    }

    private async void Start()
    {
        gachaButton.interactable = false;
        if (!await BackendManager.EnsureInstanceAndInitializedAsync())
            return;
        await TicketUIRefresh();
    }

    private async void OnGachaButton()
    {
        gachaButton.interactable = false;
        await TestMinusTicket();
        int id = await BackendManager.OneNormalGachaAsync();
        PostProcessGacha(id);
        await TicketUIRefresh();
    }

    private void PostProcessGacha(int id)
    {
        if (id > 125000)
            Debug.Log($"<color=magenta>Epic</color>: {id}");
        else if (id > 115000)
            Debug.Log($"<color=cyan>Rare</color>: {id}");
        else if (id == -1)
            Debug.LogWarning("가챠 실패");
        else
            Debug.Log($"Common: {id}");
    }

    //테스트용 치트. 이런 사악한 코드는 없어져야함
    private async void TestPlusTicket()
    {
        var incrementOptions = new IncrementBalanceOptions { WriteLock = currentWriteLock };
        PlayerBalance incrementResult = await EconomyService.Instance.PlayerBalances.IncrementBalanceAsync(BackendManager.GOLD_ID, 5, incrementOptions);
        await TicketUIRefresh();
    }

    //테스트용 2. 서버로 옮길 예정
    private async UniTask TestMinusTicket()
    {
        var decrementOptions = new DecrementBalanceOptions { WriteLock = currentWriteLock };
        PlayerBalance decrementResult = await EconomyService.Instance.PlayerBalances.DecrementBalanceAsync(BackendManager.GOLD_ID, 1, decrementOptions);
        await TicketUIRefresh();
    }

    private async UniTask TicketUIRefresh()
    {
        try
        {
            GetBalancesResult initialBalances = await EconomyService.Instance.PlayerBalances.GetBalancesAsync();
            PlayerBalance goldBalance = initialBalances.Balances.FirstOrDefault(b => b.CurrencyId == BackendManager.GOLD_ID);
            currentWriteLock = goldBalance?.WriteLock;

            if (goldBalance == null)
            {
                Debug.LogError("Gold 재화 로드 불가능");
                ticketBalance.text = "-";
                gachaButton.interactable = false;
                return;
            }
            goldString = goldBalance.Balance.ToString();

            ticketBalance.text = goldString;
            if (Int32.TryParse(goldString, out int gold) && gold > 0)
            {
                gachaButton.interactable = true;
            }


        }
        catch(EconomyException e) 
        {
            Debug.Log($"Economy 에러 발생: {e}");
        }
                
    }
}
