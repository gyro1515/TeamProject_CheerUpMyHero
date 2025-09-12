using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeedBtn : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speedText; // 배속 텍스트

    private void Start()
    {
        UpdateUI(SpeedGameManager.Instance.CurrentSpeed);
    }

    public void OnClickSpeed()
    {
        SpeedGameManager.Instance.ToggleSpeed();
        UpdateUI(SpeedGameManager.Instance.CurrentSpeed);
    }

    private void UpdateUI(SpeedGameManager.SpeedState speed)
    {
        speedText.text = $"x{(int)speed}";
        Debug.Log($"[SpeedBtn] 현재 배속: {speed}");
    }
}
