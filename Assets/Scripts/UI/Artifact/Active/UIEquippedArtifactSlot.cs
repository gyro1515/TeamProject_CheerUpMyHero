using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIEquippedArtifactSlot : MonoBehaviour
{
    [Header("유물 장착 슬롯 설정")]
    [SerializeField] Image artifactIcon;
    [SerializeField] Image slotBg;
    [SerializeField] Button slotBtn;
    [SerializeField] TextMeshProUGUI cooldownText;
    [SerializeField] TextMeshProUGUI forTestNameText;
    [SerializeField] Outline slotOutline;
    public ActiveAfData ActiveAfData;
    
    public void SetSlot(ActiveAfData data, UnityAction action = null)
    {
        ActiveAfData = data;
        slotOutline.enabled = false; // 아웃라인은 선택될 때만 활성화 상태에서 클릭됐을 때만 활성화
        slotBtn.onClick.RemoveAllListeners();
        if (ActiveAfData != null)
        {
            artifactIcon.sprite = ActiveAfData.icon;
            artifactIcon.color = new Color(1f, 1f, 1f, 1f);
            slotBg.color = new Color(1f, 1f, 1f, 1f);
            cooldownText.text = $"{ActiveAfData.cooldown}s"; // 이정도는 스트링 빌더 불필요
            forTestNameText.text = ActiveAfData.name;
            slotBtn.enabled = true;
            slotBtn.onClick.AddListener(action);
        }
        else
        {
            artifactIcon.sprite = null;
            cooldownText.text = "";
            forTestNameText.text = "빈 슬롯";
            artifactIcon.color = new Color(1f, 1f, 1f, 0.5f);
            slotBg.color = new Color(1f, 1f, 1f, 0.5f);
            slotBtn.enabled = false;
        }
    }
    public void SetOutLine(bool active)
    {
        slotOutline.enabled = active;
    }
    
}
