using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRandomAndChallenge : MonoBehaviour
{
    [SerializeField] BasePopUpUI randomPopup;
    [SerializeField] BasePopUpUI challengePopup;
    [SerializeField] Button randomButton;
    [SerializeField] Button challengeButton;

    [Header("운명, 도전 없을 때")]
    [SerializeField] BasePopUpUI noModifierPopup;
    [SerializeField] TextMeshProUGUI noModifierText;

    private void Awake()
    {
        randomButton.onClick.AddListener(OnRandomButtonClicked);
        challengeButton.onClick.AddListener(OnChallengeButtonClicked);
    }

    private void OnRandomButtonClicked()
    {
        if (!GameManager.IsTutorialCompleted)
        {
            noModifierText.text = "활성화된 운명 효과가 없습니다.";
            noModifierPopup.OpenUI();
            return;
        }

        if (PlayerDataManager.Instance.currentDestiny == null || PlayerDataManager.Instance.currentDestiny.destinyType == DestinyType.None)
        {
            noModifierText.text = "활성화된 운명 효과가 없습니다.";
            noModifierPopup.OpenUI();
            return;
        }

        randomPopup.OpenUI();
    }

    private void OnChallengeButtonClicked()
    {
        if (!GameManager.IsTutorialCompleted || PlayerDataManager.Instance.activeChallenges.Count == 0)
        {
            noModifierText.text = "활성화된 도전 효과가 없습니다.";
            noModifierPopup.OpenUI();
            return;
        }

        challengePopup.OpenUI();
    }
}
