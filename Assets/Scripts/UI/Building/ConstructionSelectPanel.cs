using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ConstructionSelectPanel : BaseUI
{
    [Header("건물 선택 버튼")]
    [SerializeField] private Button buildFarmButton;
    [SerializeField] private Button buildLumberMillButton;
    [SerializeField] private Button buildMineButton;
    [SerializeField] private Button buildBarrackButton;
    [SerializeField] private Button closeButton;

    private CanvasGroup _canvasGroup;
    private BuildingTile _targetTile;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        // 각 버튼 클릭 시 건물 ID를 넘겨주고, 건설 패널 열기
        buildFarmButton.onClick.AddListener(() => OnSelectBuilding(101)); // 농장
        buildLumberMillButton.onClick.AddListener(() => OnSelectBuilding(201)); // 벌목장
        buildMineButton.onClick.AddListener(() => OnSelectBuilding(301)); // 탄광
        buildBarrackButton.onClick.AddListener(() => OnSelectBuilding(401)); // 병영
        closeButton.onClick.AddListener(() => CloseUI());
    }

    public void Initialize(BuildingTile tile)
    {
        _targetTile = tile;
    }

    private void OnSelectBuilding(int buildingBaseID)
    {
        // 🔹 ConstructionUpgradePanel을 가져와서 '건설 모드'로 초기화
        var panel = UIManager.Instance.GetUI<ConstructionUpgradePanel>();
        if (panel != null)
        {
            panel.InitializeForConstruction(_targetTile, buildingBaseID); // 건설 모드
            panel.OpenUI();
        }

        // 선택 후 이 패널은 닫기
        CloseUI();
    }

    public override void OpenUI()
    {
        base.OpenUI();
        _canvasGroup.alpha = 0;
        transform.localScale = Vector3.one * 0.8f;

        _canvasGroup.interactable = false;
        _canvasGroup.DOFade(1, 0.3f).SetUpdate(true)
            .OnComplete(() => _canvasGroup.interactable = true);
        transform.DOScale(1, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public override void CloseUI()
    {
        _canvasGroup.interactable = false;
        _canvasGroup.DOFade(0, 0.2f).SetUpdate(true);
        transform.DOScale(0.8f, 0.2f).SetUpdate(true)
            .OnComplete(() => base.CloseUI());
    }
}
