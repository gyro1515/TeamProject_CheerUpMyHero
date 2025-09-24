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

    public void SetData(int unitId, int slotNumber)
    {
        // 슬롯 번호 텍스트는 항상 설정
        NumberText.text = (slotNumber + 1).ToString();

        if (unitId == -1) // 빈 슬롯일 때 (ID가 -1일 때)
        {
            _buttonImage.color = Color.white;
            _buttonImage.sprite = _defaultSprite;
            NumberText.gameObject.SetActive(true);
            NumberText.text = (slotNumber + 1).ToString();

        }
        else // 유닛이 채워진 슬롯일 때
        {
            // _buttonImage.sprite = unitData.IconSprite; 
            _buttonImage.color = Color.cyan;
            NumberText.gameObject.SetActive(true);
            NumberText.text = unitId.ToString();

        }
    }
}