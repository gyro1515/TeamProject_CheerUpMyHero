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
    
    private List<UIChallengeElement> _challengeElements = new List<UIChallengeElement>();   // 하위 프리펩 리스트
    private Dictionary<int, int> _tempChallenges = new Dictionary<int, int>();              // 적용시킬 챌린지 담은 딕셔너치
    
    private const float RewardPerPoint = 3.0f;      // 포인트당 보상률

    protected override void Awake()
    {
        base.Awake();
        _resetButton.onClick.AddListener(OnResetButtonClicked);

        PopulateChallengeList();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        OnResetButtonClicked();
    }

    private void PopulateChallengeList()    // 각 챌린지 요소 UI들 재생성해서 리스트에 넣음
    {
        var modifierList = DataManager.Instance.StageModifierData.Values;
        foreach(StageModifierData modifier in modifierList)
        {
            if (modifier is StageChallengeData challenge)
            {
                GameObject elements = Instantiate(_challengeElementPrefab, _challengeElementsCreatePosition);
                UIChallengeElement elementUI = elements.GetComponent<UIChallengeElement>();

                elementUI.SetElements(challenge);
                elementUI.OnElementsLevelChanged += OnChallengeLevelChanged;

                _challengeElements.Add(elementUI);
            }
        }
    }

    #region 챌린지 선택 바꼈을 때 메서드
    private void OnChallengeLevelChanged(int challengeId, int level)
    {
        if (level > 0)
        {
            _tempChallenges[challengeId] = level;
        }
        else
        {
            _tempChallenges.Remove(challengeId);
        }

        UpdateRewardBonusText();
    }

    private void UpdateRewardBonusText()
    {
        float totalPoints = 0;
        foreach(var challenge in _tempChallenges)
        {
            StageChallengeData data = DataManager.Instance.StageModifierData.GetData(challenge.Key) as StageChallengeData;
            if (data != null)
            {
                totalPoints += data.pointPerLevel * challenge.Value;
            }
        }
        float bonusPercent = totalPoints * RewardPerPoint;
        _rewardBonusText.text = $"+{bonusPercent}%";
    }

    public void ApplyChanges()
    {
        if (PlayerDataManager.Instance.activeChallenges != null)
        {
            PlayerDataManager.Instance.ClearChallenge();
        }

        foreach(var challenge in _tempChallenges)
        {
            PlayerDataManager.Instance.SetChallenges(challenge.Key, challenge.Value);
        }
        Debug.Log($"챌린지 {_tempChallenges.Count}개 저장됨");
    }
    #endregion

    private void OnResetButtonClicked()
    {
        _tempChallenges.Clear();

        foreach (UIChallengeElement element in _challengeElements)
        {
            element.ResetLevel();
        }

        UpdateRewardBonusText();
    }
}
