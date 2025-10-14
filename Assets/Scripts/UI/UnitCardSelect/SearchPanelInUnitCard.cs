using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SearchPanelInUnitCard : BasePopUpUI
{
    [SerializeField] TMP_InputField inputfield;
    
    [SerializeField] Button noButton;
    [SerializeField] Button yesButton;

    private CardFilter cardFilter;

    public void Init(CardFilter cardFilter)
    {
        this.cardFilter = cardFilter;
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        noButton.onClick.AddListener(JustClose);
        yesButton.onClick.AddListener(ConfirmSearch);

    }

    protected override void OnDisable()
    {
        base.OnDisable();
        noButton.onClick.RemoveListener(JustClose);
        yesButton.onClick.RemoveListener(ConfirmSearch);
        inputfield.text = string.Empty;
    }

    void JustClose()
    {
        inputfield.text = string.Empty;
        CloseUI();
    }

    void ConfirmSearch()
    {
        cardFilter.SetSeacrh(inputfield.text);
        CloseUI();
    }

}
