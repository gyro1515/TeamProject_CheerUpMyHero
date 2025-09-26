using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIArtifactUpMoney : MonoBehaviour
{
    [Header("골드 수량 표시")]
    [SerializeField] private TextMeshProUGUI _goldText;

    [Header("재화 수량 표시")]       // 뭔가 하나 생기지 않을까요??
    [SerializeField] private TextMeshProUGUI _cashText;

    [Header("티켓 수량 표시")]
    [SerializeField] private TextMeshProUGUI _ticketText;

    private void Awake()
    {
        int goldCount = PlayerDataManager.Instance.GetResourceAmount(ResourceType.Gold);
        _goldText.text = goldCount.ToString();
    }
}
