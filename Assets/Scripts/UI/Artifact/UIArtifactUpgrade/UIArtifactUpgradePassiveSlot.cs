using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifactUpgradePassiveSlot : MonoBehaviour
{
    [Header("인스펙터 연결 필수")]
    [SerializeField] private Image _icon;          // 유물 아이콘 이미지
    [SerializeField] private Outline _iconOutline; // 아이콘 외곽선 (유물 등급 색상용)

    // [수정] GetComponent 실수 방지를 위해 인스펙터 연결 권장
    [SerializeField] private Outline _slotOutline; // 슬롯 전체 외곽선
    [SerializeField] private Button _button;

    private int _idNumber;
    public event Action<int> OnPassiveSlotClicked;

    private void Awake()
    {
        // 인스펙터 연결을 깜빡했을 경우, 코드로 찾아봅니다.
        if (_button == null) _button = GetComponent<Button>();
        if (_slotOutline == null) _slotOutline = GetComponent<Outline>();

        if (_button != null)
        {
            _button.onClick.AddListener(OnSlotButtonClicked);
        }
    }

    public void Init(PassiveSlotViewModel vm)
    {
        // [방어 코드] 무엇이 null인지 범인을 찾습니다.
        if (_icon == null)
        {
            Debug.LogError($"[오류] {gameObject.name}의 '_icon'이 인스펙터에서 연결되지 않았습니다!");
            return; // 멈추지 않고 리턴
        }

        // vm 자체가 null인 경우 방어
        if (vm.Artifact == null)
        {
            ClearSlot();
            return;
        }

        // 정상 로직
        _idNumber = vm.Artifact.idNumber;
        _icon.sprite = vm.Icon;
        _icon.color = Color.white;

        // Outline 연결 체크 후 적용
        if (_iconOutline != null)
            _iconOutline.effectColor = vm.BorderColor;
        // else Debug.LogWarning($"{gameObject.name} : _iconOutline이 연결되지 않았습니다. (등급 색상 표시 불가)");

        if (_slotOutline != null)
            _slotOutline.effectColor = vm.BorderColor;
        // else Debug.LogWarning($"{gameObject.name} : _slotOutline(컴포넌트)을 찾을 수 없습니다.");

        if (_button != null) _button.interactable = vm.IsSelectable;
    }

    // 빈 슬롯 처리
    private void ClearSlot()
    {
        if (_icon != null)
        {
            _icon.sprite = null;
            _icon.color = Color.clear;
        }
        if (_iconOutline != null) _iconOutline.effectColor = Color.gray;
        if (_slotOutline != null) _slotOutline.effectColor = Color.gray;
        if (_button != null) _button.interactable = false;
    }

    private void OnSlotButtonClicked()
    {
        OnPassiveSlotClicked?.Invoke(_idNumber);
    }
}