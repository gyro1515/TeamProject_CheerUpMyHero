using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoticeContent : BasePopUpUI
{
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text bodyText;

    [SerializeField] Button backButton;

    private PublicMailData mailData;


    protected override void Awake()
    {
        base.Awake();
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    public void MakePostContent(PublicMailData mailData)
    {
        this.mailData = mailData;

        titleText.text = mailData.title;
        bodyText.text = mailData.body;
    }

    private void OnBackButtonClicked()
    {
        this.CloseUI();
    }
}
