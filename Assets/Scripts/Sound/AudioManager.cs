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

    protected override void Awake()
    {
        base.Awake();  // 싱글톤 초기화 (DontDestroyOnLoad 포함)

        // 씬안에 없이 런타임에 직접 생성
        if (bgmSource == null)
        {
            var bgmObj = new GameObject("BGM Source");
            bgmObj.transform.SetParent(transform);
            bgmSource = bgmObj.AddComponent<AudioSource>();
            bgmSource.loop = true;
        }

        InitSFXPool();
    }

    private void InitSFXPool()
    {
        sfxPool = new List<SFXSource>();

        if (sfxPrefab == null)
        {
            Debug.LogWarning("[AudioManager] sfxPrefab이 연결되지 않았습니다.");
            return;
        }

        for (int i = 0; i < sfxPoolSize; i++)
        {
            var go = Instantiate(sfxPrefab, transform);
            go.name = $"SFXSource_{i}";
            var src = go.GetComponent<SFXSource>();
            sfxPool.Add(src);
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
        if (Input.GetKeyDown(KeyCode.A))
        {
            // BGM 테스트
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>("BGM/Ambience1");
            PlayBGM(clip, 1f);

            if (Input.GetKeyDown(KeyCode.S))
                StopBGM();
        }
    }
}