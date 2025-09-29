//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public enum ESelecStageSlotType
//{ Main, Sub }
//public class UISelecStageSlot : MonoBehaviour
//{
//    [Header("스테이지 선택 슬롯")]
//    [SerializeField] TextMeshProUGUI stageDisplayText;
//    [SerializeField] GameObject unlockedImg;
//    [SerializeField] Button selectButton;
//    [SerializeField] int stageIdx = -1; // 메인 서브 같이 해보기, 얘는 확인용, 인스펙터에서 볼 수 있도록
//    UIStageSelect _uIStageSelect;
//    public Button SelectButton { get { return selectButton; } }

//    public void InitSlot(string dispalyText, int idx, bool isUnrocked, UIStageSelect uIStageSelect, ESelecStageSlotType type)
//    {
//        stageDisplayText.text = dispalyText;
//        stageIdx = idx;
//        unlockedImg.SetActive(!isUnrocked);
//        _uIStageSelect = uIStageSelect;
//        switch (type)
//        {
//            case ESelecStageSlotType.Main:
//                selectButton.onClick.AddListener(SetSelectedMainSlotIdx);
//                break;
//            case ESelecStageSlotType.Sub:
//                selectButton.onClick.AddListener(SetSelectedSubSlotIdx);
//                break;
//        }
//    }
//    public void SetSlot(string dispalyText, int idx, bool isUnrocked)
//    {
//        stageDisplayText.text = dispalyText;
//        stageIdx = idx;
//        SetSlotUnLocked(isUnrocked);
//    }
//    public void SetSlotUnLocked(bool isUnrocked)
//    {
//        unlockedImg.SetActive(!isUnrocked);
//    }
//    void SetSelectedMainSlotIdx()
//    {
//        _uIStageSelect.SelectedMainSlotIdx = stageIdx;
//    }
//    void SetSelectedSubSlotIdx()
//    {
//        _uIStageSelect.SelectedSubSlotIdx = stageIdx;
//    }

//}
