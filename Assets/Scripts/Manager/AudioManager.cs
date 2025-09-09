using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// 팀 컨벤션에 맞춘 싱글톤 오디오 매니저
public class AudioManager : SingletonMono<AudioManager>
{
    [Header("BGM Source")]
    [SerializeField] private AudioSource bgmSource;  // 배경음 재생 전용 오디오소스

    [Header("SFX Sources")]
    [SerializeField] private int sfxPoolSize = 10;   // 동시에 재생 가능한 효과음 개수
    private List<AudioSource> sfxSources;            // 효과음 풀
    private int sfxIndex = 0;                        // 현재 재생할 AudioSource 인덱스

    protected override void Awake()
    {
        base.Awake();   // 싱글톤 초기화 (중복 방지 + DontDestroyOnLoad)
        InitSFXPool(); // 효과음 풀 초기화
    }

    // ================================
    // 초기화
    // ================================
    private void InitSFXPool()
    {
        sfxSources = new List<AudioSource>();
        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            sfxSources.Add(src);
        }
    }

    // ================================
    // BGM 관리
    // ================================
    public void PlayBGM(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        bgmSource.clip = clip;
        bgmSource.volume = volume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // ================================
    // SFX 관리
    // ================================
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        AudioSource src = sfxSources[sfxIndex];
        src.clip = clip;
        src.volume = volume;
        src.pitch = pitch;
        src.Play();

        sfxIndex = (sfxIndex + 1) % sfxSources.Count; // 인덱스 순환
    }
}