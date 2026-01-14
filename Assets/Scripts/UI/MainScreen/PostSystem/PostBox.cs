using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using System.Threading.Tasks;

[Serializable]
public class PublicMailData
{
    public string id;
    public string title;
    public string body;
    public List<MailReward> rewards;
    public string expirationDate; // 일단 string으로 받고 나중에 파싱
}

[Serializable]
public class MailReward
{
    public string itemId;
    public int amount;
}

public class PostBox : BasePopUpUI
{
    private const float postCheckDuration = 600f;
    private List<PublicMailData> mailList = new();
    private Dictionary<string, bool> receivedCheckList= new();

    private List<string> alreadyRecivedIdList = new();

    [SerializeField] private Transform contentsTransform;
    [SerializeField] private PostList mailListprefab;

    [SerializeField] private PostContent postContent;

    [SerializeField] private Button backButton;

    [SerializeField] private UIMenu uIMenu;

    protected override void Awake()
    {
        base.Awake();
        backButton.onClick.AddListener(OnBackButtonClicked);
    }


    public async UniTask CheckNewMail(bool forceCheck)
    {
        if ( postCheckDuration <= Time.realtimeSinceStartup - BackendManager.LastPostFetched || forceCheck)
        {
            Debug.Log("메일 새로고침");

            //초기화 
            mailList.Clear();
            receivedCheckList.Clear();
            alreadyRecivedIdList.Clear();

            foreach (Transform child in contentsTransform) 
            { 
                Destroy(child.gameObject); 
            }

            //새 매일 받아오기
            mailList = await BackendManager.CheckMailAsync();

            //이미 수령된 메일 id 불러오기
            alreadyRecivedIdList = await BackendManager.LoadSimpleDataAsync<List<string>>(Constants.ALREADY_RECIEVED_MAIL_KEY);

            int newMailCount = ProcessMailData(mailList);

            //UIMenu에 표시
            uIMenu.DisplayNewPost(newMailCount);
        }
    }

    
    public int ProcessMailData(List<PublicMailData> mailList)
    {
        try
        {
            if (mailList != null)
            {
                int newMailCount = 0;
                
                
                for (int i = 0; i < mailList.Count; i++)
                {
                    // 유효기간 체크 후 유효한 메일만 처리
                    if (IsMailValid(mailList[i].expirationDate))
                    {
                        Debug.Log($"로드된 메일: {mailList[i].title} / 보상 개수: {mailList[i].rewards.Count}");

                        bool isAlreadyRecieved = false;


                        if (alreadyRecivedIdList != null)
                        {
                            isAlreadyRecieved = alreadyRecivedIdList.Contains(mailList[i].id);
                        }


                        PostList postList = Instantiate(mailListprefab, contentsTransform);
                        postList.SetPostListText(i, mailList[i].title, mailList[i].expirationDate, isAlreadyRecieved, this);

                        if (isAlreadyRecieved)
                        {
                            receivedCheckList[mailList[i].id] = true;
                        }
                        else
                        {
                            receivedCheckList[mailList[i].id] = false;
                            newMailCount++;
                        }

                        
                    }
                }
                

                return newMailCount;
            }
            else
            {
                return 0;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"JSON 파싱 실패: {e.Message}");
            return 0;
        }
    }

    // 날짜 유효성 검사 함수
    private bool IsMailValid(string dateString)
    {
        if (string.IsNullOrEmpty(dateString)) return true; // 날짜 없으면 무제한으로 간주

        if (DateTime.TryParse(dateString, out DateTime expiration))
        {
            // 현재 시간과 비교
            return DateTime.Now < expiration;
        }

        Debug.LogWarning($"날짜 형식이 잘못됨: {dateString}");
        return false;
    }

    public void OpenPostContent(int num)
    {
        bool isAlreadyRecieved = receivedCheckList[mailList[num].id];
        postContent.MakePostContent(mailList[num], isAlreadyRecieved);
        postContent.OpenUI();
    }

    public async UniTask OnRewardRecieved(string mailId)
    {
        
        //한 번도 수령을 한적이 없으면 널이 뜸
        if (alreadyRecivedIdList == null)
        {
            alreadyRecivedIdList = new List<string>();
        }
        
        alreadyRecivedIdList.Add(mailId);
        var data = new Dictionary<string, object>();
        data.Add(Constants.ALREADY_RECIEVED_MAIL_KEY, alreadyRecivedIdList);

        await BackendManager.SaveDataAsync(data);

        await CheckNewMail(true);
    }

    private void OnBackButtonClicked()
    {
        this.CloseUI();
    }

}
