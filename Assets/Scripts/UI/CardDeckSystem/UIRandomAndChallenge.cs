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

        randomPopup.OpenUI();
    }

    private void OnChallengeButtonClicked()
    {
        if (!GameManager.IsTutorialCompleted) return;

        challengePopup.OpenUI();
    }
}
