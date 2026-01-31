using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PostContent : BasePopUpUI
{
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text bodyText;
    [SerializeField] TMP_Text expireText;

    [SerializeField] List<PostReward> postRewards;

    [SerializeField] Button getRewardButton;
    [SerializeField] TMP_Text getRewardButtonText;

    [SerializeField] Button backButton;

    private PublicMailData mailData;

    [SerializeField] PostBox postBox;

    protected override void Awake()
    {
        base.Awake();
        getRewardButton.onClick.AddListener(() => {OnRewardButtonClickAsync().Forget(); });
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    public void MakePostContent(PublicMailData mailData, bool alreadyRewarded)
    {
        //보상 UI 초기화
        foreach (PostReward postReward in postRewards)
        {
            postReward.gameObject.SetActive(false);
        }

        this.mailData = mailData;

        titleText.text = mailData.title;
        bodyText.text = mailData.body;
        expireText.text = "만료: " + mailData.expirationDate;

        getRewardButton.interactable = !alreadyRewarded;

        if (alreadyRewarded)
            getRewardButtonText.text = "수령 완료";

        if (mailData.rewards != null && mailData.rewards.Count >= 1)
        {
            foreach (MailReward mailReward in mailData.rewards)
            {
                int targetIndex = -1;

                switch (mailReward.itemId)
                {
                    case Constants.GOLD_ID: targetIndex = 0; break;
                    case Constants.WOOD_ID: targetIndex = 1; break;
                    case Constants.IRON_ID: targetIndex = 2; break;
                    case Constants.TICKET_ID: targetIndex = 3; break;
                    case Constants.MAGICSTONE_ID: targetIndex = 4; break;
                    case Constants.BM_ID: targetIndex = 5; break;
                }

                if (targetIndex >= 0 && targetIndex < postRewards.Count)
                {
                    PostReward targetUI = postRewards[targetIndex];
                    targetUI.gameObject.SetActive(true);
                    targetUI.rewardIntText.text = mailReward.amount.ToString();
                }
                else
                {
                    Debug.LogWarning($"매칭되지 않는 ID 혹은 배열 범위 초과: {mailReward.itemId}");
                }
            }
        }
    }

    private async UniTaskVoid OnRewardButtonClickAsync()
    {
        if (!getRewardButton.interactable) return;

        getRewardButton.interactable = false;

        UIManager.Instance.ShowLoading();

        try
        {
            var rewardTasks = new List<UniTask>();

            foreach (var mailReward in mailData.rewards)
            {

                ResourceType resourceType = ResourceType.Food;

                switch (mailReward.itemId)
                {
                    case Constants.GOLD_ID: resourceType = ResourceType.Gold; break;
                    case Constants.WOOD_ID: resourceType = ResourceType.Wood; break;
                    case Constants.IRON_ID: resourceType = ResourceType.Iron; break;
                    case Constants.TICKET_ID: resourceType = ResourceType.Ticket; break;
                    case Constants.MAGICSTONE_ID: resourceType = ResourceType.MagicStone; break;
                    case Constants.BM_ID: resourceType = ResourceType.Bm; break;
                }

                if (resourceType == ResourceType.Food)
                {
                    Debug.LogWarning($"매칭되지 않는 ID: {mailReward.itemId}");
                    return;
                }

                rewardTasks.Add(PlayerDataManager.Instance.AddResource(resourceType, mailReward.amount));
            }

            await UniTask.WhenAll(rewardTasks);

            //보상을 받았다고 클라이언트(PostBox)에게 전달
            await postBox.OnRewardRecieved(mailData.id);

            getRewardButtonText.text = "수령 완료";

        }
        catch(Exception ex) 
        {
            Debug.LogException(ex);
            getRewardButton.interactable = true;
        }
        finally
        {
            UIManager.Instance.HideLoading();
        }
        
    }

    private void OnBackButtonClicked()
    {
        this.CloseUI();
    }
}
