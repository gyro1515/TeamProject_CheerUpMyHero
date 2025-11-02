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
        // 1단계: PlayerDataManager 확인
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogError("❌ PlayerDataManager.Instance가 null입니다!");
            return;
        }

        // 2단계: DataManager 확인
        if (DataManager.PlayerData == null)
        {
            Debug.LogError("❌ DataManager.PlayerData가 null입니다!");
            return;
        }

        int PlayerLevelBaseId = 10000000;
        int currentLevel = PlayerDataManager.Instance.PlayerLevel;
        int targetId = PlayerLevelBaseId + currentLevel;

        // 3단계: 자세한 로그 출력
        Debug.Log($"=== 레벨 데이터 조회 디버그 ===");
        Debug.Log($"현재 PlayerLevel: {currentLevel}");
        Debug.Log($"조회할 데이터 ID: {targetId}");

        // 4단계: 데이터 조회
        PlayerData playerData = DataManager.PlayerData.GetData(targetId);

        if (playerData == null)
        {
            // 주변 ID들도 확인해보기
            Debug.LogError($"❌ ID {targetId}에 해당하는 데이터가 없습니다!");
            Debug.Log("주변 ID 확인:");
            for (int i = -2; i <= 2; i++)
            {
                int checkId = targetId + i;
                var testData = DataManager.PlayerData.GetData(checkId);
                Debug.Log($"  ID {checkId}: {(testData != null ? "존재함" : "null")}");
            }
            return;
        }

        // 나머지 UI 업데이트 코드...
        _levelText.text = $"Lv. {currentLevel}";

        int currentExp = PlayerDataManager.Instance.GetResourceAmount(ResourceType.EXP);
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
