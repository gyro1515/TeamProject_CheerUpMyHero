using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingUI : MonoBehaviour, IBackButtonHandler
{
    [Header("UI")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Button backButton;

    // 저장 키
    private const string KEY_Master = "Master_VOL";
    private const string KEY_BGM = "BGM_VOL";
    private const string KEY_SFX = "SFX_VOL";

    private void Awake()
    {
        // 슬라이더 초기화 & 이벤트 등록
        if (masterSlider)
        {
            float v = PlayerPrefs.GetFloat(KEY_Master, 1f);
            masterSlider.minValue = 0f;
            masterSlider.maxValue = 1f;
            masterSlider.wholeNumbers = false;
            masterSlider.SetValueWithoutNotify(v);
            AudioManager.Instance.SetMasterVolumeLinear(v);
            masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
        }

        if (bgmSlider)
        {
            float v = PlayerPrefs.GetFloat(KEY_BGM, 1f);
            bgmSlider.minValue = 0f;
            bgmSlider.maxValue = 1f;
            bgmSlider.wholeNumbers = false;
            bgmSlider.SetValueWithoutNotify(v);
            AudioManager.Instance.SetBGMVolumeLinear(v);
            bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);
        }

        if (sfxSlider)
        {
            float v = PlayerPrefs.GetFloat(KEY_SFX, 1f);
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.wholeNumbers = false;
            sfxSlider.SetValueWithoutNotify(v);
            AudioManager.Instance.SetSFXVolumeLinear(v);
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        }
        backButton?.onClick.AddListener(OnBackBtn);
    }

    public void OnMasterSliderChanged(float value)
    {
        AudioManager.Instance.SetMasterVolumeLinear(value);
        PlayerPrefs.SetFloat(KEY_Master, value);
    }
    public void OnBgmSliderChanged(float value)
    {
        AudioManager.Instance.SetBGMVolumeLinear(value);
        PlayerPrefs.SetFloat(KEY_BGM, value);
    }

    public void OnSfxSliderChanged(float value)
    {
        AudioManager.Instance.SetSFXVolumeLinear(value);
        PlayerPrefs.SetFloat(KEY_SFX, value);
    }


    private void OnDestroy()
    {
        // 이벤트 해제 & 저장
        if (masterSlider) masterSlider.onValueChanged.RemoveListener(OnMasterSliderChanged);
        if (bgmSlider) bgmSlider.onValueChanged.RemoveListener(OnBgmSliderChanged);
        if (sfxSlider) sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);
        PlayerPrefs.Save();
    }
    public void Open()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
    public void OnBackBtn()
    {
        gameObject.SetActive(false);
    }
    public void OnBackPressed()
    {
        Debug.Log("[AudioSettingUI] OnBackPressed");

    }
}
