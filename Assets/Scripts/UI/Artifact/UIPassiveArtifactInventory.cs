using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPassiveArtifactInventory : MonoBehaviour
{
    [Header("인벤토리 타이틀")]
    [SerializeField] private TextMeshProUGUI _title;

    [Header("닫기 버튼")]
    [SerializeField] private Button _closeButton;

    [Header("인벤토리 버튼들")]
    [SerializeField] private Button _sortButton;
    [SerializeField] private Button _equipButton;
    [SerializeField] private Button _noEquipButton;

    [Header("인벤토리 슬롯")]
    [SerializeField] private GameObject _slotPrefab;
    [SerializeField] private Transform _slotCreatPosition;

    private CanvasGroup _canvasGroup;

    private List<UIArtifactSlot> _slotList = new List<UIArtifactSlot>();

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        _closeButton.onClick.AddListener(OnCloseButtonClicked);
    }

    public void OpenInventory(EffectTarget target)
    {
        FadeManager.Instance.FadeInUI(_canvasGroup);
        RefreshInventory(target);
    }

    private void OnCloseButtonClicked()
    {
        FadeManager.Instance.FadeOutUI(_canvasGroup);
    }

    private void RefreshInventory(EffectTarget target)
    {
        List<ArtifactData> ownedList = PlayerDataManager.Instance.OwnedArtifacts;

        List<PassiveArtifactData> filteredData = ownedList.OfType<PassiveArtifactData>()    // 패시브 유물임 && target 타입임 필터링함
                                                          .Where(artifact => artifact.effectTarget == target)
                                                          .ToList();

        switch (target)
        {
            case EffectTarget.Player:
                _title.text = "플레이어 패시브 유물";
                break;

            case EffectTarget.MeleeUnit:
                _title.text = "근거리 유닛 패시브 유물";
                break;

            case EffectTarget.RangedUnit:
                _title.text = "원거리 유닛 패시브 유물";
                break;

            default:
                _title.text = "패시브 유물";
                break;
        }

        foreach (UIArtifactSlot slot in _slotList)      // 원래 넣어뒀던 슬롯 싹 없애기 -> 다른 종류 유물일 수 있으니까
        {
            slot.gameObject.SetActive(false);
        }

        while (_slotList.Count < filteredData.Count)    // 여기서 딱 걸러낸 개수만큼만 슬롯 만들어둠
        {
            GameObject createdSlot = Instantiate(_slotPrefab, _slotCreatPosition);
            _slotList.Add(createdSlot.GetComponent<UIArtifactSlot>());
        }

        for (int i = 0; i < filteredData.Count; i++)    // 각 슬롯에 데이터 넣고 
        {
            _slotList[i].Init(filteredData[i]);
            _slotList[i].gameObject.SetActive(true);
        }
    }
}
