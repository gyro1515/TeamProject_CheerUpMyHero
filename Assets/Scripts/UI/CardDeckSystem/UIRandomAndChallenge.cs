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
        randomButton.onClick.AddListener(() => randomPopup.OpenUI());
        challengeButton.onClick.AddListener(() => challengePopup.OpenUI());
    }

}
