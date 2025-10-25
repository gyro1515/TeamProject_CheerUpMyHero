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

        // --- 1. ID로 등급 및 데이터 가져오기 (임시) ---
        // DataManager에서 유닛 데이터(스프라이트, 등급) 가져오기
        // var unitData = DataManager.Instance.GetUnitData(_resultId);
        _rarityColor = GetColorForRarity(id); // 임시: ID로 등급 판별
        // ------------------------------------

        // --- 2. 앞면/뒷면 색상 설정 ---
        rarityBorderImage.color = _rarityColor;
        cardBackImage.color = _rarityColor; // 사용자 요청: 뒷면도 등급 색상으로
        // characterImage.sprite = unitData.sprite; //  실제 스프라이트 설정
        // -----------------------------

        // 버튼 클릭 이벤트 연결
        GetComponent<Button>().onClick.AddListener(OnClick);

        if (showAsFlipped) Flip(false); // 즉시 앞면 표시 (에픽 선공개용)
        else ShowBack(); // 뒷면 표시
    }

    // 카드를 클릭했을 때
    private void OnClick()
    {
        if (_isFlipped) return; // 이미 뒤집혔으면 무시
        _controller.OnGridCardClicked(this, _resultId); // 연출 감독에게 알림
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
    private Color GetColorForRarity(int id)
    {
        if (id > 125000) return Color.yellow; // Epic 
        if (id > 115000) return Color.magenta;    // Rare 
        return Color.blue;                   // Common
    }
}