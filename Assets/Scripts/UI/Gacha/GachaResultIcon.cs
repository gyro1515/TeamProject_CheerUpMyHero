using UnityEngine;
using UnityEngine.UI;

public class GachaResultIcon : MonoBehaviour
{
    [SerializeField] private Image cardBackImage;      // 카드 뒷면 이미지 (색상 변경용)
    [SerializeField] private GameObject cardFrontGroup;   // 카드 앞면 그룹 (RarityBorder, CharacterImage 포함)
    [SerializeField] private Image rarityBorderImage;  // 앞면 등급 테두리
    [SerializeField] private Image characterImage;     // 앞면 캐릭터 이미지

    private int _resultId;
    private bool _isFlipped = false;
    private GachaSequenceController _controller;
    private Color _rarityColor;

    // 카드를 생성할 때 연출 감독이 호출하는 함수
    public void Setup(int id, GachaSequenceController controller, bool showAsFlipped = false)
    {
        _resultId = id;
        _controller = controller;

        var unitData = DataManager.PlayerUnitData.GetData(_resultId);

        if (unitData != null)
        {
            _rarityColor = GetColorForRarity(unitData.rarity);
            characterImage.sprite = unitData.gachaHeroSprite ?? unitData.unitIconSprite;
        }
        else
        {
            Debug.LogError($"[GachaResultIcon] ID: {_resultId}에 해당하는 유닛 데이터를 찾을 수 없습니다!");
            _rarityColor = GetColorForRarity(Rarity.common);
            characterImage.sprite = null;
        }
        // --- 4. 앞면/뒷면 색상 설정 ---
        rarityBorderImage.color = _rarityColor;
        cardBackImage.color = _rarityColor; 

        GetComponent<Button>().onClick.AddListener(OnClick);
        if (showAsFlipped) Flip(false);
        else ShowBack();
    }
    // 카드를 클릭했을 때
    private void OnClick()
    {
        if (_isFlipped) return; // 이미 뒤집혔으면 무시
        _controller.OnGridCardClicked(this, _resultId); 
    }

    public void ShowBack()
    {
        cardBackImage.gameObject.SetActive(true);
        cardFrontGroup.SetActive(false);
        _isFlipped = false;
    }

    // 카드를 앞면으로 뒤집음
    public void Flip(bool withAnimation = true)
    {
        if (_isFlipped) return;
        _isFlipped = true;

        if (withAnimation) { /*  뒤집히는 애니메이션 */ }

        cardBackImage.gameObject.SetActive(false);
        cardFrontGroup.SetActive(true);
    }

    // ID로 등급별 색상 반환
    private Color GetColorForRarity(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.epic: return Color.yellow;
            case Rarity.rare: return Color.magenta;
            case Rarity.common: return Color.blue;
            default: return Color.white;
        }
    }
}