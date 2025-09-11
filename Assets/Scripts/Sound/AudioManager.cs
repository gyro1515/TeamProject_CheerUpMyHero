using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// 팀 컨벤션에 맞춘 싱글톤 오디오 매니저
public class AudioManager : SingletonMono<AudioManager>
{
    [Header("BGM Source")]
    [SerializeField] private AudioSource bgmSource;  // 배경음 재생 전용 오디오소스

    [Header("SFX Prefab")]
    [SerializeField] private GameObject sfxPrefab; // SFXSource가 붙은 프리팹
    [SerializeField] private int sfxPoolSize = 10;

    private List<SFXSource> sfxPool;   // 효과음 풀 리스트
    private int sfxIndex = 0;          // 현재 사용할 인덱스

    // 실행 전에 무조건 AudioManager 생성
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateInstance()
    {
        if (Instance == null)
        {
            var go = new GameObject("AudioManager");
            go.AddComponent<AudioManager>();
            Debug.Log("[Bootstrap] AudioManager created at startup.");
        }
    }

    protected override void Awake()
    {
        base.Awake(); // 싱글톤 초기화
        InitBGMSource();
        //InitSFXPool();

        var clip = Resources.Load<AudioClip>("Sound/Light Ambience 1");
        if (clip != null)
        {
            PlayBGM(clip, 1f);
        }
    }

    // BGM 관리
    public void PlayBGM(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        bgmSource.clip = clip;
        bgmSource.volume = volume;
        bgmSource.loop = true;
        bgmSource.Play();
    }
    private void InitBGMSource()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
        }
    }

    public void StopBGM() => bgmSource.Stop();

    // SFX 관리
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        var src = sfxPool[sfxIndex];
        src.Play(clip, volume, pitch);
        sfxIndex = (sfxIndex + 1) % sfxPool.Count;
    }

    private void Update()
    {
        //if (Input.GetKey(KeyCode.A))
        //{
        //    // BGM 테스트
        //    var clip = Resources.Load<AudioClip>("Sound/Light Ambience 1");
        //    PlayBGM(clip, 1f);

        //    if (Input.GetKey(KeyCode.S))
        //        StopBGM();
        //}
    }
}