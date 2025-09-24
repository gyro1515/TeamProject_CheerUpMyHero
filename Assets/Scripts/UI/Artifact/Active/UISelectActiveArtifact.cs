using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class UISelectActiveArtifact : BaseUI
{
    [Header("액티브 유물 선택 UI 세팅")]
    [SerializeField] Button toSelCardBtn;
    [SerializeField] Button toSelPassiveBtn;
    [SerializeField] Button removeAllArtifactBtn;
    [SerializeField] UIRemoveAllAfPanel removeAllArtifactPanel;
    [SerializeField] UIOwnedPanel ownedPanel;
    [SerializeField] UIEquippedPanel equippedPanel;
    [SerializeField] UIDescriptionArtifactPanel descriptionArtifactPanel;
    [SerializeField] UIEquipBtn equipBtn;
    [SerializeField] UIUnEquipBtn unEquipBtn;

    ActiveAfData mainSelectedActiveAfData;

    private void Awake()
    {
        // 여기서 모든 구독 처리하고, 각 클래스에서는 실행만하기
        toSelCardBtn.onClick.AddListener(ToSerlCard);
        toSelPassiveBtn.onClick.AddListener(ToSelPassive);
        // 일괄 해제 이벤트 세팅
        removeAllArtifactBtn.onClick.AddListener(() => { removeAllArtifactPanel?.SetActive(true); });
        removeAllArtifactPanel.OnRemoveAllAAf += equippedPanel.RemoveAllEquippedActiveArtifact;
        // 장착/해제 이벤트 세팅
        // 버튼 클릭시 장착/해제 바인드
        equipBtn.Init(() => SetEquip(true));
        unEquipBtn.Init(() => SetEquip(false));
        /*equipBtn.Init(() => ownedPanel.SetEquip(true));
        unEquipBtn.Init(() => ownedPanel.SetEquip(false));*/
        // 새로고침 시, 선택된 아이템 장착/해체 버튼 활성화 여부 바인드
        ownedPanel.OnEquipStateChange += equipBtn.SetBtnActive;
        ownedPanel.OnEquipStateChange += unEquipBtn.SetBtnActive;
        // 장착/해제 이벤트 바인드
        /*ownedPanel.OnEquip += equippedPanel.EquipActiveArtifact;
        ownedPanel.OnUnEquip += equippedPanel.UnEquipActiveArtifact;*/
        // 리팩토링 해야할 듯 합니다... *********
        // 소유 유물 클릭시 이벤트 바인드, 설명 패널, 장착/해제 버튼, 장착 패널 세팅
        ownedPanel.OnClickBtn += descriptionArtifactPanel.SetDescriptionPanel;
        ownedPanel.OnClickBtn += equipBtn.SetBtnActive;
        ownedPanel.OnClickBtn += unEquipBtn.SetBtnActive;
        ownedPanel.OnClickBtn += SetMainSelectedAfData;
        ownedPanel.OnResetEquippedSelectedData += equippedPanel.ResetSelectedAfData;
        // 장착 유물 클릭시 이벤트 바인드, 설명 패널, 장착/해제 버튼, 장착 패널 세팅
        equippedPanel.OnClickBtn += descriptionArtifactPanel.SetDescriptionPanel;
        equippedPanel.OnClickBtn += equipBtn.SetBtnActive;
        equippedPanel.OnClickBtn += unEquipBtn.SetBtnActive;
        equippedPanel.OnClickBtn += SetMainSelectedAfData;
        equippedPanel.OnResetOwnedSelectedData += ownedPanel.ResetSelectedAfData;
        // *********************
        // 일괄 세팅 이벤트 바인드
        ownedPanel.OnAutoAssign += equippedPanel.AutoAssignAfByList;
        // 일괄 해제 시 소유 슬롯 상태 변경 이벤트 바인드
        equippedPanel.OnSetOwnedSlotEquipState += ownedPanel.SetOwnedSlotEquipState;
    }
    void SetEquip(bool isEquipped)
    {
        if (mainSelectedActiveAfData == null) { Debug.LogWarning("로직 오류"); return; }
        if (isEquipped) // 현재 선택한 슬롯 장착
        {
            if (!equippedPanel.EquipActiveArtifact(mainSelectedActiveAfData)) return; // 장착 불가능 하면 아래 세팅 필요 없음
        }
        else // 해제
        {
            equippedPanel.UnEquipActiveArtifact(mainSelectedActiveAfData);
        }
        ownedPanel.SetOwnedSlotEquipState(mainSelectedActiveAfData);
    }
    public void SetMainSelectedAfData(ActiveAfData data)
    {
        mainSelectedActiveAfData = data;
    }
    void ToSerlCard()
    {
        Debug.Log("카드 선택으로");
    }
    void ToSelPassive()
    {
        Debug.Log("패시브 유물 선택으로");
    }

}
