using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIDeckChallengePopup : BasePopUpUI
{
    [Header("UI 참조")]
    [SerializeField] private GameObject _challengeElementPrefab;
    [SerializeField] private Transform _challengeElementsCreatePosition;
    [SerializeField] private TextMeshProUGUI _rewardBonusText;

    private Dictionary<int, int> _challenges;
    private List<UIDeckChallengeElement> _elements = new List<UIDeckChallengeElement> ();

    protected override void OnEnable()
    {
        base.OnEnable();

        _challenges = PlayerDataManager.Instance.activeChallenges;
        
        foreach(var element in _elements)
        {
            if (element != null)
            {
                Destroy(element.gameObject);
            }
        }

        _elements.Clear();

        CreateElements();
        RefreshUI();
    }

    private void RefreshUI()
    {
        float totalBonus = CalculateBonus(_challenges);

        _rewardBonusText.text = $"+{totalBonus}%";
    }

    private void CreateElements()
    {
        foreach (var challenge in _challenges)
        {
            StageChallengeData data = DataManager.Instance.StageModifierData.GetData(challenge.Key) as StageChallengeData;

            GameObject element = Instantiate(_challengeElementPrefab, _challengeElementsCreatePosition);
            UIDeckChallengeElement elementUI = element.GetComponent<UIDeckChallengeElement> ();

            elementUI.SetElement(data, challenge.Value);
            
            _elements.Add(elementUI);
        }
    }

    private float CalculateBonus(Dictionary<int, int> challenges)
    {
        float totalPoint = 0f;
        foreach (var challenge in _challenges)
        {
            StageChallengeData data = DataManager.Instance.StageModifierData.GetData(challenge.Key) as StageChallengeData;

            if (data != null)
            {
                totalPoint += data.pointPerLevel * challenge.Value;
            }
        }
        return totalPoint * 3;   
    }
}
