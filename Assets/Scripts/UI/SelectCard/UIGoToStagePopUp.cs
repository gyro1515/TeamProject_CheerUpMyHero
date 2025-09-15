using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGoToStagePopUp : MonoBehaviour
{
    private List<int> transferDesckList = new List<int>();
    
    [SerializeField] Button stageButton;
    [SerializeField] Button backButton;

    private void Awake()
    {
        stageButton.onClick.AddListener(GoToStageSelect);
        backButton.onClick.AddListener(ClosePopUP);
    }

    void GoToStageSelect()
    {
        PlayerDataManager.Instance.SetDeckList(transferDesckList);

        //카드 선택 닫고 스테이지 선택 열기
        UIManager.Instance.CloseUI<UISelectCard>();
        UIManager.Instance.OpenUI<UIStageSelect>();

        //근데 뒤로가기 버튼 생각하면 스테이지 선택을 열린 채로 두어야 할수도?
    }

    public void SetTransferDesckList(List<int> list)
    {
        transferDesckList = list;
    }

    void ClosePopUP()
    {
        this.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        stageButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();
    }
}
