using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDropDownInUnitCard : MonoBehaviour
{
    private CardFilter cardFilter;

    [SerializeField] Button close;

    [SerializeField] Button reset;
    [SerializeField] Button rarity;
    [SerializeField] Button cost;
    [SerializeField] Button health;
    [SerializeField] Button atkPower;
    [SerializeField] Button coolTime;

    Button[] buttons;

    private void Awake()
    {
        buttons = new Button[] { close, reset, rarity, cost, health, atkPower, coolTime};
    }

    public void Init(CardFilter cardFilter)
    {
        this.cardFilter = cardFilter;
    }

    private void OnEnable()
    {
        close.onClick.AddListener(OnClose);
        reset.onClick.AddListener(OnReset);
        rarity.onClick.AddListener(OnRarity);
        cost.onClick.AddListener(OnCost);
        health.onClick.AddListener(OnHealth);
        atkPower.onClick.AddListener(OnAtkPower);
        coolTime.onClick.AddListener(OnCoolTime);
    }

    private void OnDisable()
    {
        foreach (Button button in buttons)
        {
            button.onClick.RemoveAllListeners();
        }
    }

    void OnClose()
    {
        this.gameObject.SetActive(false);
    }

    void OnReset()
    {
        cardFilter.SetFilter(SelectedFilter.None);
    }

    void OnRarity()
    {
        cardFilter.SetFilter(SelectedFilter.Rarity);
    }

    void OnCost()
    {
        cardFilter.SetFilter(SelectedFilter.Cost);
    }

    void OnHealth()
    {
        cardFilter.SetFilter(SelectedFilter.Health);
    }

    void OnAtkPower()
    {
        cardFilter.SetFilter(SelectedFilter.AtkPower);
    } 

    void OnCoolTime()
    {
        cardFilter.SetFilter(SelectedFilter.CoolTime);
    }

}
