using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITest : BaseUI
{
    [Header("조작 패널 UI")]
    [SerializeField] private GameObject _defaultLayoutPrefeb;
    [SerializeField] private GameObject _changedLayoutPrefeb;
    [SerializeField] UIAfExpanationPopup uIAfExpanationPopup;
    [SerializeField] UIUnitexplanationPopup uIUnitexplanationPopup;

    [Header("프리펩이 생성될 위치")]
    [SerializeField] private Transform _layoutParents;

    private GameObject _currentLayoutInstance;
    private void Awake()
    {
        uIAfExpanationPopup.Init();
        uIUnitexplanationPopup.Init();
    }
    private void OnEnable()
    {
        //SettingDataManager.OnControlLayoutChanged += UpdateLayout;
    }

    private void Start()
    {
        //UpdateLayout();
    }

    private void OnDisable()
    {
        //SettingDataManager.OnControlLayoutChanged -= UpdateLayout;
    }

    private void UpdateLayout()
    {
        if (_currentLayoutInstance != null)
        {
            Destroy(_currentLayoutInstance);
        }

        int layoutType = SettingDataManager.Instance.ControlPanelLayoutType;

        GameObject prefabToInstantiate = (layoutType == 0) ? _defaultLayoutPrefeb : _changedLayoutPrefeb;

        if (prefabToInstantiate != null && _layoutParents != null)
        {
            _currentLayoutInstance = Instantiate(prefabToInstantiate, _layoutParents);
        }
    }
}
