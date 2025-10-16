using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestSceneSpawnUnitSlot : MonoBehaviour
{
    [Header("유닛 선택 소환 아이콘 설정")]
    [SerializeField] Image unitIcon;
    [SerializeField] Image unitIconTimer; // 쿨타임 표시, 버튼 클릭 방지용
    [SerializeField] Button spawnUnitBtn;
    [SerializeField] TextMeshProUGUI text; // 추후 아이콘만 설정하면 될 듯 합니다.
    float _cooldown = -1f;
    float _cooldownTimer = -1f;
    bool isCooldown = false;
    PoolType playerUnitType;
    private void Awake()
    {
        spawnUnitBtn.onClick.AddListener(OnSpawnUnit);
    }
    private void Update()
    {
        if (!isCooldown) return; // 쿨타임이 아니면 리턴
        _cooldownTimer += Time.deltaTime;
        unitIconTimer.fillAmount = 1 - _cooldownTimer / _cooldown;
        if (_cooldownTimer < _cooldown) return; // 아직 쿨타임이 다 안돌았다면 리턴
        unitIconTimer.fillAmount = 1f;
        SetTimerIconActive(false);
    }

    public void InitSpawnUnitSlot(Sprite sprite, int cardIdx, float cooldown)
    {
        unitIcon.sprite = sprite;
        text.text = cardIdx.ToString();
        _cooldown = cooldown;
        playerUnitType = (PoolType)cardIdx; // 테스트용
        if (cardIdx == -1) // 빈 슬롯은 클릭 안되도록
        {
            text.enabled = false;
            spawnUnitBtn.enabled = false;
            unitIconTimer.fillAmount = 1f;
            return;
        }
        SetTimerIconActive(false);
    }
    void OnSpawnUnit()
    {
        SetTimerIconActive(true);
        // 여기서 유닛 소환, 테스트 용으로 이렇게 형변환
        //if ((int)playerUnitType != -1) GameManager.Instance.PlayerHQ.SpawnUnit(playerUnitType);
    }
    void SetTimerIconActive(bool active)
    {
        _cooldownTimer = 0f;
        isCooldown = active;
        enabled = active;
        unitIconTimer.gameObject.SetActive(active);
        unitIconTimer.fillAmount = active ? 1f : 0f;
    }
}
