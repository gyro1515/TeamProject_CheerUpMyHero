using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXSource : MonoBehaviour
{
    private AudioSource audioSource; // 실제 재생을 담당하는 AudioSource
    //private bool wasKeyPressed = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>(); // 보장
        audioSource.playOnAwake = false; // 자동 재생 방지
    }

    // 효과음 1회 재생
    public void Play(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;          // 안전 가드
        audioSource.clip = clip;           // 클립 지정
        audioSource.volume = volume;       // 볼륨
        audioSource.pitch = pitch;         // 피치
        audioSource.Play();                // 재생 시작
    }

    public bool IsPlaying => audioSource != null && audioSource.isPlaying; // 현재 재생 중 여부

    void Update()
    {
        //숫자 1번키 테스트용
        //if (Input.GetKeyUp(KeyCode.Alpha1))
        //{
        //wasKeyPressed = false;
        //    AudioManager.Instance.PlaySFXByResource("Sound/Swing", 1f);
        //}
    }
}
