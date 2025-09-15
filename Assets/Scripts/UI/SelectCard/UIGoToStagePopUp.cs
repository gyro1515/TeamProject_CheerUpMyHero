using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGoToStagePopUp : MonoBehaviour
{
    [SerializeField] Button stageButton;
    [SerializeField] Button backButton;

    private void Awake()
    {
        stageButton.onClick.AddListener(GoToStageSelect);
        backButton.onClick.AddListener(ClosePopUP);
    }

    void GoToStageSelect()
    {
        Debug.Log("카드 선택 UI 끄고 스테이지 UI 키기");
    }


    void ClosePopUP()
    {
        this.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        stageButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();
    }
}
