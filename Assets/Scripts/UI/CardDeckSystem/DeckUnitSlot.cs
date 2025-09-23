using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckUnitSlot : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI NumberText;

    private Image _buttonImage;
    private Sprite _defaultSprite;

    private void Awake()
    {
        _buttonImage = GetComponent<Image>();
        _defaultSprite = _buttonImage.sprite;
    }

    public void SetData(bool unitData, int slotNumber)
    {
        // 슬롯 번호 텍스트는 항상 설정
        NumberText.text = (slotNumber + 1).ToString();

        if (unitData) // 빈 슬롯일 때
        {
            _buttonImage.sprite = _defaultSprite;
            NumberText.gameObject.SetActive(true);
        }
        else // 유닛이 채워진 슬롯일 때
        {
            // _buttonImage.sprite = unitData.IconSprite; 
            NumberText.gameObject.SetActive(false);
        }
    }
}