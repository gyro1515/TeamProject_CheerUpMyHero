using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionSelectPanel : BaseUI
{
    [SerializeField] private GameObject buildingSelectItemPrefab;
    [SerializeField] private Transform contentParent;

    private CanvasGroup _canvasGroup;

    private BuildingTile _targetTile;
    private bool _isClosing = false;



    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }


    // 패널이 열릴 때 호출되는 함수
    // 패널이 열릴 때 호출되는 함수
    public void Initialize(BuildingTile tile, ConstructionUpgradePanel upgradePanel)
    {
        _targetTile = tile;

        // 1. 기존에 있던 버튼들을 모두 삭제
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        List<BuildingUpgradeData> buildableList = PlayerDataManager.Instance.GetBuildableList();

        // 3. 각 건물 데이터에 해당하는 버튼을 생성하여 Content 자식으로 추가
        foreach (BuildingUpgradeData data in buildableList)
        {
            GameObject itemGO = Instantiate(buildingSelectItemPrefab, contentParent);
            BuildingSelectItem item = itemGO.GetComponent<BuildingSelectItem>();
            // data.idNumber를 사용하여 버튼을 초기화
            item.Initialize(data.idNumber, _targetTile, this, upgradePanel); 
        }
    }

    public override void OpenUI()
    {
        if (_isClosing) return; //중복 호출 방지
        _isClosing = true;
        base.OpenUI();
        FadeManager.Instance.FadeInUI(_canvasGroup);
    }

    public override void CloseUI()
    {
        if (_targetTile != null)
        {
            MainScreenBuildingController.Instance.DeselectTile();
            _targetTile = null; 
        }
        FadeManager.Instance.FadeOutUI(_canvasGroup);
        StartCoroutine(CoCloseAfterDelay(0.3f));
    }

    private IEnumerator CoCloseAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        base.CloseUI();
        _isClosing = false; //완료 후 플래그 초기화
    }
}