using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIStageClearArtifactSelect : BaseUI
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
    private UIRandomArtifactSlot _currentlySelectedSlot = null;

    private bool is9StageAndFirst;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        _rerollButton.onClick.AddListener(OnRerollButtonClicked);
        _selectButton.onClick.AddListener(OnSelectButtonClicked);
    }

    public void OpenSelectUI(ArtifactType type)
    {
        AudioManager.PlayOneShot(DataManager.AudioData.ClearArtifactSE);
        _type = type;
        FadeManager.FadeInUI(_canvasGroup);
        RandomCreate(type);

        _selectButton.interactable = false;
        isRerolled = false;
    }

    //스테이지 9 끝났을때 전용
    public void OpenSelectUI(ArtifactType type, bool isSub9)
    {
        OpenUI();
        AudioManager.PlayOneShot(DataManager.AudioData.ClearArtifactSE);
        is9StageAndFirst = isSub9;
        _type = type;
        FadeManager.FadeInUI(_canvasGroup);
        RandomCreate(type);

        _selectButton.interactable = false;
        isRerolled = false;
    }


    private void RandomCreate(ArtifactType type)
    {
        int currentChapter = PlayerDataManager.Instance.SelectedStageIdx.mainStageIdx;

        List<ArtifactData> data = new List<ArtifactData>();

        if (type == ArtifactType.Active)
        {
            List<ActiveArtifactData> randomAAf = ArtifactManager.Instance.GetRandomActiveArtifact(ActiveArtifactRandomCreateCount);
            data = randomAAf.Cast<ArtifactData>().ToList();
        }
        else
        {
            List<PassiveArtifactData> randomPAf = ArtifactManager.Instance.GetRandomPassiveArtifact(PassiveArtifactRandomCreateCount, currentChapter);
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
        if (_currentlySelectedSlot != null)
        {
            _currentlySelectedSlot.SetSelected(false);
        }

        _currentlySelectedSlot = _slots.FirstOrDefault(slot => slot.GetData() == data);

        if (_currentlySelectedSlot != null)
        {
            _currentlySelectedSlot.SetSelected(true);
        }
    }

    private void OnRerollButtonClicked()
    {
        selectedArtifact = null;
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

            //9번 스테이지 깨고 첫번째 유물 선택에서만 나옴, 두번째는 안나옴
            if (is9StageAndFirst)
            {
                is9StageAndFirst = false;
                _type = ArtifactType.Active;
                RandomCreate(_type);
                selectedArtifact = null;
                _selectButton.interactable = false;
                return;
            }

            FadeManager.FadeOutUI(_canvasGroup, () => GameManager.Instance.ShowResultUI(true).Forget()); // await 일부러 뺀거에 컴파일 경고 안뜨드록 처리

        }
    }
}
