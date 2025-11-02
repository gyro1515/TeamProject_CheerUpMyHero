using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class UITutorialBase : BaseUI, IBackButtonHandler
{
    [SerializeField] private List<GameObject> tutorialSteps = new List<GameObject>();
    [SerializeField] private Button backBtn;
    [SerializeField] private Button nextBtn;
    [SerializeField] private Button skipBtn;

    int stepIdx = 0;
    int maxStep = -1;

    protected virtual void Awake()
    {
        backBtn.onClick.AddListener(OnLeftButtonClicked);
        nextBtn.onClick.AddListener(OnRightButtonClicked);
        skipBtn.onClick.AddListener(OnSkipButtonClicked);

        maxStep = tutorialSteps.Count;

        if (maxStep > 0 )
        {
            tutorialSteps[0].SetActive(true);

            for (int i = 1; i < maxStep; i++)
            {
                tutorialSteps[i].SetActive(false);
            }
        }
    }

    private void OnEnable()
    {
        UIManager.PubishAddUIStackEvent(this);
    }

    private void OnDisable()
    {
        UIManager.PublishRemoveUIStackEvent();
    }

    void OnLeftButtonClicked()
    {
        if (stepIdx <= 0) return;

        Debug.Log("왼쪽 눌림");

        tutorialSteps[stepIdx--].SetActive(false);
        tutorialSteps[stepIdx].SetActive(true);
    }

    protected virtual void OnRightButtonClicked()
    {
        if (stepIdx >= maxStep - 1)
        {
            OnSkipButtonClicked();
            return;
        }

        Debug.Log("오른쪽 눌림");

        tutorialSteps[stepIdx++].SetActive(false);
        tutorialSteps[stepIdx].SetActive(true);
    }

    protected virtual void OnSkipButtonClicked()
    {
        CloseUI();
    }

    public void OnBackPressed()
    {
        OnSkipButtonClicked();
    }
}

