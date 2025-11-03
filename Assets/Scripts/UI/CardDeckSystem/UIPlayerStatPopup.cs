using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerStatPopup : BasePopUpUI
{
    [Header("경험치")]
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _expText;
    [SerializeField] private Image _expBarImage;

    [Header("스텟 텍스트")]
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private TextMeshProUGUI _atkText;
    [SerializeField] private TextMeshProUGUI _moveSpeedText;
    [SerializeField] private TextMeshProUGUI _auraRangeText;
    [SerializeField] private TextMeshProUGUI _auraAtkText;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        RefreshUI();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    private void RefreshUI()
    {
        int currentLevel = PlayerDataManager.Instance.PlayerLevel;

        // 레벨 유효성 검사
        if (currentLevel <= 0)
        {
            Debug.LogWarning($"PlayerLevel이 {currentLevel}임. 레벨 불러오는 로직 오류 있음. 일단 1로 보정");
            currentLevel = 1;
            PlayerDataManager.Instance.PlayerLevel = 1;
        }

        PlayerData playerData = DataManager.PlayerData.GetData(currentLevel);

        if (playerData == null)
        {
            Debug.LogError($"레벨이 {currentLevel}라서 데이터 null 뜸. 레벨 가져오는 로직 오류 있어요");
            return;
        }

        _levelText.text = $"Lv. {currentLevel}";

        int currentExp = PlayerDataManager.Instance.CurExp;
        int expToNextLevel = playerData.exp;

        if (expToNextLevel > 0)
        {
            _expText.text = $"{currentExp} / {expToNextLevel}";
            _expBarImage.fillAmount = (float)currentExp / expToNextLevel;
        }
        else
        {
            _expText.text = "MAX";
            _expBarImage.fillAmount = 1f;
        }

        _hpText.text = $"체력: {playerData.health}";
        _atkText.text = $"공격력: {playerData.atkPower}";
        _moveSpeedText.text = $"이동 속도: {playerData.moveSpeed}";
        _auraRangeText.text = $"오라 범위: {playerData.auraRange}";
        _auraAtkText.text = $"오라 공격력 버프: {playerData.auraAtkBonus}%";
    }
}