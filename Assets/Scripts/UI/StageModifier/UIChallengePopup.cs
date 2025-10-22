using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChallengePopup : BasePopUpUI
{
    [Header("UI 참조")]
    [SerializeField] private GameObject _challengeElementPrefab;
    [SerializeField] private Transform _challengeElementsCreatePosition;
    [SerializeField] private TextMeshProUGUI _rewardBonusText;
    [SerializeField] private Button _resetButton;

    private ChallengeModel _model;
    private ChallengePopupViewModel _viewModel;

    private List<UIChallengeElement> _challengeElements = new List<UIChallengeElement>();   // 하위 프리펩 리스트
    
    protected override void Awake()
    {
        base.Awake();

        _model = new ChallengeModel();
        _viewModel = new ChallengePopupViewModel(_model);

        _viewModel.OnRewardTextChanged += OnRewardTextNeedChange;

        _resetButton.onClick.AddListener(OnResetButtonClicked);

        CreateElements();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        OnResetButtonClicked();
    }

    private void CreateElements()    // 각 챌린지 요소 UI들 재생성해서 리스트에 넣음
    {
        var modifierList = DataManager.Instance.StageModifierData.Values;
        foreach(StageModifierData modifier in modifierList)
        {
            if (modifier is StageChallengeData challenge)
            {
                GameObject elements = Instantiate(_challengeElementPrefab, _challengeElementsCreatePosition);
                UIChallengeElement elementUI = elements.GetComponent<UIChallengeElement>();

                elementUI.SetElements(challenge);
                elementUI.OnElementsLevelChanged += _viewModel.UpdateTempChallenge;

                _challengeElements.Add(elementUI);
            }
        }
    }

    private void OnRewardTextNeedChange(string text)
    {
        _rewardBonusText.text = text;
    }

    public void ApplyChanges()
    {
        _viewModel?.ApplyChallenges();
    }

    private void OnResetButtonClicked()
    {
        _viewModel.ClearTempChallenges();

        foreach (UIChallengeElement element in _challengeElements)
        {
            element.ResetLevel();
        }
    }
}
