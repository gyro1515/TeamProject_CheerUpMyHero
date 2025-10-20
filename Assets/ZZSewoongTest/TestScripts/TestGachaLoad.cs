using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using Cysharp.Threading.Tasks;

public class TestGachaLoad : MonoBehaviour
{
    [SerializeField] Button GachaButton;

    private void OnEnable()
    {
        GachaButton.onClick.AddListener(OnGachaButton);
    }


    private async void OnGachaButton()
    {
        GachaButton.interactable = false;
        int id = await BackendManager.OneNormalGachaAsync();
        PostProcessGacha(id);
        GachaButton.interactable = true;
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
}
