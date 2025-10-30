using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDeckDestinyPopup : BasePopUpUI
{
    [Header("UI 참조")]
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _descriptrion;

    StageDestinyData _activeDestiny;

    protected override void OnEnable()
    {
        base.OnEnable();

        _activeDestiny = PlayerDataManager.Instance.currentDestiny;
        RefreshUI(_activeDestiny);
    }

    private void RefreshUI(StageDestinyData destiny)
    {
        if (destiny == null)
        {
            _image.gameObject.SetActive(false);
            _title.text = "현재 선택된 운명 효과가 없습니다.";
            _descriptrion.text = "";
            return;
        }

        _image.gameObject.SetActive(true);
        _image.sprite = Resources.Load<Sprite>(destiny.iconSpritePath);
        _title.text = destiny.name;
        _descriptrion.text = destiny.description;
    }
}
