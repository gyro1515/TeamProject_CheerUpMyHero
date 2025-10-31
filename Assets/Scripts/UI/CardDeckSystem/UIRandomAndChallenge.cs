using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRandomAndChallenge : MonoBehaviour
{
    [SerializeField] BasePopUpUI randomPopup;
    [SerializeField] BasePopUpUI challengePopup;
    [SerializeField] Button randomButton;
    [SerializeField] Button challengeButton;
    private void Awake()
    {
        randomButton.onClick.AddListener(OnRandomButtonClicked);
        challengeButton.onClick.AddListener(OnChallengeButtonClicked);
    }

    private void OnRandomButtonClicked()
    {
        if (!GameManager.IsTutorialCompleted) return;

        if (PlayerDataManager.Instance.currentDestiny == null || PlayerDataManager.Instance.currentDestiny.destinyType == DestinyType.None) return;

        randomPopup.OpenUI();
    }

    private void OnChallengeButtonClicked()
    {
        if (!GameManager.IsTutorialCompleted) return;

        if (PlayerDataManager.Instance.activeChallenges.Count == 0) return;

        challengePopup.OpenUI();
    }
}
