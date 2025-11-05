using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHQSkills : MonoBehaviour
{
    [SerializeField] Button openButtion;
    [SerializeField] Button closeButtion;
    [SerializeField] GameObject hqSkillIconPrefab;
    [SerializeField] Transform iconContainer;
    [SerializeField] HorizontalLayoutGroup horizontalLayout;

    [SerializeField] CanvasGroup skillPanel;

    private void Start()
    {
        PlayerHQSkill playerHQSkill = GameManager.Instance.PlayerHQ.GetComponent<PlayerHQSkill>();
        playerHQSkill.HQSkillsCooldowns.Clear();
        for (int i = 0; i < playerHQSkill.HQSkills.Count; i++)
        {
            GameObject iconGO = Instantiate(hqSkillIconPrefab, iconContainer);
            HQSkillsCooldown icon = iconGO.GetComponent<HQSkillsCooldown>();
            icon.InitHQSkillCooldowm(playerHQSkill.HQSkills[i], playerHQSkill, playerHQSkill.GetPoolTypeByIdx(i));
            playerHQSkill.HQSkillsCooldowns.Add(icon);
        }
        StartCoroutine(RebuildNextFrame(horizontalLayout.GetComponent<RectTransform>()));
    }
    private void OnEnable()
    {
        openButtion.onClick.AddListener(OnOpenButton);
        closeButtion.onClick.AddListener(OnCloseButton);
    }

    private void OnDisable()
    {
        openButtion.onClick.RemoveAllListeners();
        closeButtion.onClick.RemoveAllListeners();
    }
    IEnumerator RebuildNextFrame(RectTransform layoutRoot)
    {
        yield return null; // 1프레임 대기
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
    }
    void OnOpenButton()
    {
        skillPanel.alpha =1.0f;
        skillPanel.interactable = true;
        skillPanel.blocksRaycasts = true;
        openButtion.gameObject.SetActive(false);        
    }

    void OnCloseButton()
    {
        skillPanel.alpha = 0.0f;
        skillPanel.interactable = false;
        skillPanel.blocksRaycasts = false;
        openButtion.gameObject.SetActive(true);
    }

}
