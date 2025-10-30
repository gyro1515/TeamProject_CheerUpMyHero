using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckUnitSlot : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI InfoText;
    [SerializeField] private Image bg;
    [SerializeField] private Image icon;
    [SerializeField] private Sprite nullBg;
    [SerializeField] private GameObject opaqueOverlay; 
   // [SerializeField] private Image unitIconImage;      //유닛 아이콘을 표시할 이미지

    private Image _buttonImage;
    private Sprite _defaultSprite;

    private void Awake()
    {
        _buttonImage = GetComponent<Image>();
        _defaultSprite = _buttonImage.sprite;
    }

    public void SetData(BaseUnitData unitData, int slotNumber)
    {
        SetValidationState(true);
        if (unitData == null) // 빈 슬롯일 때
        {
           // unitIconImage.gameObject.SetActive(false); // 아이콘 숨기기
            InfoText.gameObject.SetActive(true);
            icon.gameObject.SetActive(false);
            bg.sprite = nullBg; // 빈 슬롯 배경 스프라이트로 변경
            InfoText.text = (slotNumber + 1).ToString(); // 슬롯 번호 표시
        }
        else // 유닛이 채워진 슬롯일 때
        {
            //  unitIconImage.gameObject.SetActive(true);
            // unitIconImage.sprite = unitData.IconSprite; //아이콘 스프라이트 연결
            icon.gameObject.SetActive(true);
            InfoText.gameObject.SetActive(false);
            InfoText.text = unitData.unitName; // 유닛 이름 표시 (또는 ID: unitData.poolType)
            bg.sprite = unitData.unitBGSprite;
            icon.sprite = unitData.unitIconSprite;
        }
    }
    public void SetValidationState(bool isValid)
    {
        if (opaqueOverlay != null)
        {
            // 유효하지 않으면(false) -> 오버레이를 켠다(true)
            opaqueOverlay.SetActive(!isValid);
        }
    }
}
