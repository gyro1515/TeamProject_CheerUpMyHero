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
            Color rarityColor = GetColorForRarity(unitData.rarity);

            // 1. 가챠 전용 일러스트(gachaHeroSprite)가 있는지 확인
            if (unitData.gachaHeroSprite != null)
            {
                // 2. 가챠 일러스트가 있으면:
                //    캐릭터 이미지를 '일러스트'로 설정
                characterImage.sprite = unitData.gachaHeroSprite;
                //    '배경(테두리)' 이미지는 숨깁니다.
                rarityBorderImage.gameObject.SetActive(false);
            }
            else
            {
                // 3. 가챠 일러스트가 없으면
                //    캐릭터 이미지를 '작은 아이콘'으로 설정
                characterImage.sprite = unitData.unitIconSprite;
                //    '배경(테두리)' 이미지를 '유닛 배경 스프라이트'로 설정
                rarityBorderImage.sprite = unitData.unitBGSprite;
                //    (스프라이트 자체를 바꾸므로 색상은 기본 흰색으로)
                rarityBorderImage.color = Color.white;
                rarityBorderImage.gameObject.SetActive(true);
            }

            // 4. 카드 뒷면 색상은 등급에 맞게 설정 
            cardBackImage.color = rarityColor;
        }
        else
        {
            // 데이터 못 찾았을 때의 기본 처리
            Debug.LogError($"[GachaResultIcon] ID: {_resultId}에 해당하는 유닛 데이터를 찾을 수 없습니다!");
            _rarityColor = GetColorForRarity(Rarity.common);
            characterImage.sprite = null;
            rarityBorderImage.sprite = null; // 배경도 비움
            rarityBorderImage.color = Color.white;
            rarityBorderImage.gameObject.SetActive(true);
            cardBackImage.color = _rarityColor;
        }

        // 버튼 클릭 이벤트 연결
        GetComponent<Button>().onClick.AddListener(OnClick);

        if (showAsFlipped) Flip(false); // 즉시 앞면 표시 (에픽 선공개용)
        else ShowBack(); // 뒷면 표시
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