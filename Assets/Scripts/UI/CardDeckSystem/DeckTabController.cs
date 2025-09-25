using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;

public class DeckTabController : MonoBehaviour
{
    [SerializeField] private List<DeckPresetController.DeckTabUI> deckTabs;

    public Action<int> OnTabSelected;
    public Action OnEditIconClicked;

    public void Initialize()
    {
        for (int i = 0; i < deckTabs.Count; i++)
        {
            int deckIndex = i + 1;
            deckTabs[i].TabButton.onClick.AddListener(() => OnTabSelected?.Invoke(deckIndex));

            deckTabs[i].EditIconObject.GetComponent<Button>().onClick.AddListener(() => OnEditIconClicked?.Invoke());
        }
    }
    public void UpdateTabs(int activeDeckIndex)
    {
        for (int i = 0; i < deckTabs.Count; i++)
        {
            bool isActive = (i + 1 == activeDeckIndex);

            deckTabs[i].NameText.text = PlayerDataManager.Instance.DeckPresets[i + 1].DeckName;

            deckTabs[i].TabButton.image.color = isActive ? Color.yellow : Color.white;

            deckTabs[i].EditIconObject.SetActive(isActive);
        }
    }
}