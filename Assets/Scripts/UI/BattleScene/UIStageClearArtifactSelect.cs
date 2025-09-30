using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIStageClearArtifactSelect : MonoBehaviour
{
    [Header("재생성 버튼")]
    [SerializeField] private Button _rerollButton;

    [Header("결정 버튼")]
    [SerializeField] private Button _selectButton;

    [Header("유물 패널 프리펩")]
    [SerializeField] private GameObject _slotPreFab;

    [Header("프리펩 생성 위치")]
    [SerializeField] private Transform _container;

    private const int ActiveArtifactRandomCreateCount = 2;
    private const int PassiveArtifactRandomCreateCount = 3;

    private CanvasGroup _canvasGroup;

    private List<UIRandomArtifactSlot> _slots = new List<UIRandomArtifactSlot>();

    public ArtifactData selectedArtifact;
    private ArtifactType _type = ArtifactType.Passive;

    private bool isRerolled = false;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        _rerollButton.onClick.AddListener(OnRerollButtonClicked);
        _selectButton.onClick.AddListener(OnSelectButtonClicked);
    }

    public void OpenSelectUI(ArtifactType type)
    {
        _type = type;
        FadeManager.Instance.FadeInUI(_canvasGroup);
        RandomCreate(type);
        isRerolled = false ;
    }

    private void RandomCreate(ArtifactType type)
    {
        List<ArtifactData> data = new List<ArtifactData>();

        if (type == ArtifactType.Active)
        {
            List<ActiveArtifactData> randomAAf = ArtifactManager.Instance.GetRandomActiveArtifact(ActiveArtifactRandomCreateCount);
            data = randomAAf.Cast<ArtifactData>().ToList();
        }
        else
        {
            List<PassiveArtifactData> randomPAf = ArtifactManager.Instance.GetRandomPassiveArtifact(PassiveArtifactRandomCreateCount);
            data = randomPAf.Cast<ArtifactData>().ToList();
        }
        UpdateSlot(data);
    }

    private void UpdateSlot(List<ArtifactData> data)
    {
        while (_slots.Count < data.Count)
        {
            GameObject slot = Instantiate(_slotPreFab, _container);
            UIRandomArtifactSlot newSlot = slot.GetComponent<UIRandomArtifactSlot>();
            newSlot.OnStageClearArtifactSlotClicked += SelectSlot;
            _slots.Add(newSlot);
        }

        for (int i = 0; i < _slots.Count; i++)
        {
            if (i < data.Count)
            {
                _slots[i].Init(data[i]);
                _slots[i].gameObject.SetActive(true);
            }
            else
            {
                _slots[i].gameObject.SetActive(false);
            }
        }
    }

    private void SelectSlot(ArtifactData data)
    {
        selectedArtifact = data;
        _selectButton.interactable = true;
    }

    private void OnRerollButtonClicked()
    {
            Debug.Log("광고 관련 로직 넣어야 함");
            RandomCreate(_type);

            isRerolled = true;
            //_rerollButton.interactable = false;
    }

    private void OnSelectButtonClicked()
    {
        if (selectedArtifact != null)
        {
            ArtifactManager.Instance.AddArtifact(selectedArtifact.idNumber);
            FadeManager.Instance.FadeOutUI(_canvasGroup);
        }
    }
}
