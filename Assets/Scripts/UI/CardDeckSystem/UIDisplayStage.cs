using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIDisplayStage : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI stageText;
    private void OnEnable()
    {
        (int mainStageIdx, int subStageIdx) = PlayerDataManager.Instance.SelectedStageIdx;
        stageText.text = $"{mainStageIdx + 1}-{subStageIdx + 1}";
    }
}
