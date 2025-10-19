using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengePopupViewModel
{
    public event Action<string> OnRewardTextChanged;

    private Dictionary<int, int> _tempChallenges = new Dictionary<int, int>();

    private ChallengeModel _model;
    public ChallengePopupViewModel(ChallengeModel model)
    {
        _model = model;
    }

    public void UpdateTempChallenge(int id, int lv)
    {
        if (lv > 0)
        {
            _tempChallenges[id] = lv;
        }
        else
        {
            _tempChallenges.Remove(id);
        }

        TextUpdate();
    }

    public void ClearTempChallenges()
    {
        _tempChallenges.Clear();

        TextUpdate();
    }
    
    public void ApplyChallenges()
    {
        _model.ApplyChallenges(_tempChallenges);
    }

    private void TextUpdate()
    {
        float bonusPer = _model.CalculateRewardBonusPer(_tempChallenges);
        string bonusText = bonusPer.ToString("+#;-#;+0") + "%";
        OnRewardTextChanged?.Invoke(bonusText);
    }
}
