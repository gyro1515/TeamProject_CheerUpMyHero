using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; 

public class GachaResultIcon : MonoBehaviour
{
    [SerializeField] private Image cardBackImage;      // 카드 뒷면 이미지 (색상 변경용)
    [SerializeField] private GameObject cardFrontGroup;   // 카드 앞면 그룹 (RarityBorder, CharacterImage 포함)
    [SerializeField] private Image rarityBorderImage;  // 앞면 등급 테두리
    [SerializeField] private Image characterImage;     // 앞면 캐릭터 이미지
    [SerializeField] private Image backgroundMaskImage; // BackgroundMask 오브젝트의 Image 컴포넌트

    [Header("등급별 카드 뒷면 스프라이트")]
    [SerializeField] private Sprite commonBackSprite; // Common 등급용 뒷면
    [SerializeField] private Sprite rareBackSprite;   // Rare 등급용 뒷면
    [SerializeField] private Sprite epicBackSprite;   // Epic 등급용 뒷면
    private int _resultId;
    public bool IsFlipped { get; private set; } = false;
    private GachaSequenceController _controller;

    // 카드를 생성할 때 연출 감독이 호출하는 함수
    public void Setup(int id, GachaSequenceController controller, bool showAsFlipped = false)
    {
        _resultId = id;
        _controller = controller;

        var unitData = DataManager.PlayerUnitData.GetData(_resultId);

        if (unitData != null)
        {
            cardBackImage.sprite = GetBackSpriteForRarity(unitData.rarity);

            // 1. 가챠 전용 일러스트(gachaHeroSprite)가 있는지 확인
            if (unitData.gachaHeroSprite != null)
            {
                characterImage.sprite = unitData.gachaHeroSprite;
                //가챠 일러스트가 있으면 배경 마스크와 배경 이미지 모두 비활성화
                backgroundMaskImage.gameObject.SetActive(false);
                rarityBorderImage.gameObject.SetActive(false);
            }
            else
            {
                characterImage.sprite = unitData.unitIconSprite;
                //배경 마스크와 배경 이미지 활성화
                backgroundMaskImage.gameObject.SetActive(true);
                rarityBorderImage.gameObject.SetActive(true);
                rarityBorderImage.sprite = unitData.unitBGSprite; // 유닛 배경 스프라이트 할당
                rarityBorderImage.color = Color.white; // 색상 보정 (이미지 자체 색상 사용)
            }
        }
        else
        {
            Debug.LogError($"[GachaResultIcon] ID: {_resultId}에 해당하는 유닛 데이터를 찾을 수 없습니다!");
            characterImage.sprite = null;
            backgroundMaskImage.gameObject.SetActive(false); // 오류 시 배경 숨김
            rarityBorderImage.gameObject.SetActive(false); // 오류 시 배경 숨김
            cardBackImage.sprite = commonBackSprite; // 오류 시 기본 뒷면
        }

        GetComponent<Button>().onClick.AddListener(OnClick);
        if (showAsFlipped) FlipSimple(false);
        else ShowBack();
    }
    // 카드를 클릭했을 때
    private void OnClick()
    {
        if (IsFlipped) return; // 이미 뒤집혔으면 무시
        _controller.OnGridCardClicked(this, _resultId);
    }

    public void ShowBack()
    {
        cardBackImage.gameObject.SetActive(true);
        cardFrontGroup.SetActive(false);
        IsFlipped = false;
    }

    // 카드를 앞면으로 뒤집음
    public void FlipSimple(bool withAnimation = true)
    {
        if (IsFlipped) return;
        IsFlipped = true;

        if (withAnimation && gameObject.activeInHierarchy)
        {
            // 애니메이션 재생
            transform.DOScaleX(0f, 0.15f).SetEase(Ease.InQuad).OnComplete(() => {
                cardBackImage.gameObject.SetActive(false);
                cardFrontGroup.SetActive(true);
                transform.DOScaleX(1f, 0.15f).SetEase(Ease.OutQuad).OnComplete(() => {
                    // ✨ 1. 애니메이션 끝나면 '단순 보고' (CheckIfAllFlipped용)
                    _controller?.OnCardFlipped(this);
                });
            });
        }
        else // 즉시 뒤집기
        {
            transform.localScale = Vector3.one;
            cardBackImage.gameObject.SetActive(false);
            cardFrontGroup.SetActive(true);
            _controller?.OnCardFlipped(this); // ✨ '단순 보고'
        }
    }

    // --- ✨ "상세보기용 뒤집기" 함수 (카드 클릭 전용) ✨ ---
    /// <summary>
    /// 카드를 앞면으로 뒤집고, 애니메이션이 끝나면 '상세보기'를 요청합니다.
    /// </summary>
    public void FlipForDetail()
    {
        if (IsFlipped) return; // 이미 뒤집히는 중/완료면 무시
        IsFlipped = true;

        // 1. 뒤집기 애니메이션 시작
        transform.DOScaleX(0f, 0.15f).SetEase(Ease.InQuad).OnComplete(() => {
            cardBackImage.gameObject.SetActive(false);
            cardFrontGroup.SetActive(true);
            // 2. 카드가 완전히 뒤집힌 후
            transform.DOScaleX(1f, 0.15f).SetEase(Ease.OutQuad).OnComplete(() => {
                // 3. 상세보기 콜백 호출
                _controller?.OnCardFlipAnimationFinished(this);
            });
        });
    }
    private Sprite GetBackSpriteForRarity(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.epic: return epicBackSprite;
            case Rarity.rare: return rareBackSprite;
            case Rarity.common: return commonBackSprite;
            default: return commonBackSprite; // 기본값
        }
    }
}