using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIEquipedArtifactSlot : MonoBehaviour
{
    [Header("유물 장착 슬롯 설정")]
    [SerializeField] Image artifactIcon;
    [SerializeField] TextMeshProUGUI cooldownText;
    [SerializeField] TextMeshProUGUI forTestNameText;
    [SerializeField] Outline slotOutline;
    public bool IsNull {  get; private set; } = true;

    // string name = ""은 추후 삭제해야 함
    // 유물 데이터 그 자체를 가져와서 세팅할 수도..?
    public void SetSlot(Sprite iconSprite, float cooldown, string name = "")
    {
        artifactIcon.sprite = iconSprite;
        cooldownText.text = cooldown.ToString();
        forTestNameText.text = name;
        IsNull = false;
        slotOutline.enabled = true;
        artifactIcon.color = new Color(1f, 1f, 1f, 1f);

    }
    public void SetSlotNull()
    {
        artifactIcon.sprite = null;
        cooldownText.text = "";
        artifactIcon.color = new Color(1f, 1f, 1f, 0.5f);
        IsNull = true;
        slotOutline.enabled = false;
    }
}
