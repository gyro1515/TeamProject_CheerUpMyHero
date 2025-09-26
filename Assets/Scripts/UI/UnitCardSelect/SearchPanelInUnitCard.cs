using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SearchPanelInUnitCard : MonoBehaviour
{
    [SerializeField] TMP_InputField inputfield;
    
    [SerializeField] Button noButton;
    [SerializeField] Button yesButton;
    Button touchBackground;

    private CardFilter cardFilter;

    public void Init(CardFilter cardFilter)
    {
        this.cardFilter = cardFilter;
    }

    private void Awake()
    {
        touchBackground = GetComponent<Button>();
    }

    private void OnEnable()
    {
        touchBackground.onClick.AddListener(JustClose);
        noButton.onClick.AddListener(JustClose);
        yesButton.onClick.AddListener(ConfirmSearch);
    }

    private void OnDisable()
    {
        touchBackground.onClick.RemoveListener(JustClose);
        noButton.onClick.RemoveListener(JustClose);
        yesButton.onClick.RemoveListener(ConfirmSearch);
        inputfield.text = string.Empty;
    }

    void JustClose()
    {
        inputfield.text = string.Empty;
        this.gameObject.SetActive(false);
    }

    void ConfirmSearch()
    {
        cardFilter.SetSeacrh(inputfield.text);
        this.gameObject.SetActive(false);
    }

}
