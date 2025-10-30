using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

// 팀 컨벤션에 맞춘 싱글톤 오디오 매니저
public class AudioManager : SingletonMono<AudioManager>
{
    [Header("오디오 소스")]
    [SerializeField] private AudioSource bgmSource; // 배경음 재생 전용 오디오소스
    [SerializeField] private AudioSource sfxSource;  // 효과음 오디오소스 

    [Header("볼륨 크기")]
    [SerializeField] private float masterVolume = 1.0f;
    [SerializeField] private float bgmVolume = 1.0f;
    [SerializeField] private float sfxVolume = 1.0f;
    [SerializeField] private float distance = 7.5f; // 거리에 따른 음량 감쇠 기준 거리
    [SerializeField] private float attenuation = 0.2f; // 거리에 따른 음량 감쇠 비율

    /*[Header("BGM 페이드 효과")]
    [SerializeField] private float bgmFadeDuration = 0.5f;*/
    // 저장 키
    private const string KEY_Master = "Master_VOL";
    private const string KEY_BGM = "BGM_VOL";
    private const string KEY_SFX = "SFX_VOL";

    Camera mainCam;
    protected override void Awake()
    {
        base.Awake();
        mainCam = Camera.main;
        LoadMixerAssets();
        InitBGMSource();      // BGM AudioSource 확보
        InitSFXPool();        // SFX 풀 구성 (프리팹 없음도 안전)

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.outputAudioMixerGroup = sfxGroup;
        }
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void Start()
    {
        float v = PlayerPrefs.GetFloat(KEY_Master, 1f);
        //this.masterVolume = v;
        SetMasterVolumeLinear(v);
        v = PlayerPrefs.GetFloat(KEY_BGM, 1f);
        //this.bgmVolume = v;
        SetBGMVolumeLinear(v);
        v = PlayerPrefs.GetFloat(KEY_SFX, 1f);
        //this.sfxVolume = v;
        SetSFXVolumeLinear(v);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (System.Enum.TryParse(scene.name, out SceneState curSceneState))
        {
            PlayBGM(curSceneState, 0.3f);
        }
        else
        {
            Debug.LogWarning($"{scene.name} 씬 없어서 기본 브금 나옴. 씬 로드 구독 문제 있어요");
            PlayBGM(SceneState.MainScene, 0.3f);
        }
    }

    // ============================ 효과음 =============================
    // 기본 효과음 재생
    public static void PlayOneShot(AudioClip clip, float vol = 1.0f)
    {
        float volume = Instance.masterVolume * Instance.sfxVolume;
        Instance.sfxSource.PlayOneShot(clip, vol * volume);
    }
    // 랜덤 효과음 재생
    public static void PlayRandomOneShot(AudioClip[] clip, float vol = 1.0f)
    {
        int randomInt = Random.Range(0, clip.Length);
        //float volume = Instance.masterVolume * Instance.sfxVolume;
        //Instance.sfxSource.PlayOneShot(clip[randomInt], vol * volume);
        PlayOneShot(clip[randomInt], vol);
    }
    // 거리 기반 효과음 재생
    public static void PlayOneShotByCameraDistance(AudioClip clip, Transform pos, float vol = 1.0f)
    {
        if(Instance.mainCam == null) Instance.mainCam = Camera.main;
        float dist = Mathf.Abs(Instance.mainCam.transform.position.x - pos.position.x);
        if(dist > Instance.distance) // 기준 거리 넘으면 감쇠 적용
        {
            PlayOneShot(clip, vol * Instance.attenuation);
            //Debug.Log($"거리 {dist}라서 감쇠 적용됨");
        }
        else
        {
            PlayOneShot(clip, vol);
            //Debug.Log($"거리 {dist}라서 감쇠 없음");
        }
    }
    // 거리 기반 효과음 랜덤 재생
    public static void PlayRandomOneShotByCameraDistance(AudioClip[] clip, Transform pos, float vol = 1.0f)
    {
        int randomInt = Random.Range(0, clip.Length);
        PlayOneShotByCameraDistance(clip[randomInt], pos, vol);
    }
    // =================================================================

    // ============================ 배경음 =============================
    public static void PlayBGM(SceneState sceneState, float volume = 1.0f)
    {
        AudioClip clip = SelectBGM(sceneState, volume);

        if (Instance.bgmSource.clip != clip)
        {
            Instance.bgmSource.clip = clip;
            Instance.bgmSource.volume = volume * Instance.masterVolume * Instance.bgmVolume;
            if(Instance.bgmSource.loop == false) Instance.bgmSource.loop = true;
            Instance.bgmSource.Play();
        }
    }

    public static AudioClip SelectBGM(SceneState sceneState, float volume = 0.8f)
    {
        AudioClip clip = null;

        if (sceneState == SceneState.BattleScene)
        {
            (int mainIdx, int subIdx) = PlayerDataManager.Instance.SelectedStageIdx; 
            if (subIdx == 8)
            {
                clip = DataManager.AudioData.BossStageBGM;
                Debug.Log($"스테이지 {mainIdx + 1}-{subIdx + 1}이라서 보스 BGM 나옴. 서브가 9면 맞는 거임. 아니면 오류)");
            }
            else
            {
                switch (mainIdx)
                {
                    case 0:
                        clip = DataManager.AudioData.chapter1BGM;
                        break;
                    case 1:
                        clip = DataManager.AudioData.chapter2BGM;
                        break;
                    case 2:
                        clip = DataManager.AudioData.chapter3BGM;
                        break;
                    case 3:
                        clip = DataManager.AudioData.chapter4BGM;
                        break;
                    case 4:
                        clip = DataManager.AudioData.chapter5BGM;
                        break;
                    default:
                        clip = DataManager.AudioData.mainBGM;
                        break;
                }
            }
        }
        else if (sceneState == SceneState.MainScene)
        {
            clip = DataManager.AudioData.mainBGM;
        }
        else if (sceneState == SceneState.StartScene)
        {
            clip = DataManager.AudioData.startBGM;
        }

        return clip;
    }
    // =================================================================


    /*protected override void Awake()
    {
        base.Awake();

        if (bgmSource == null )
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
        }

        
    }*/

    [Header("SFX Prefab")]
    [SerializeField] private GameObject sfxPrefab;   // SFXSource가 붙은 프리팹 (없어도 런타임 생성)
    [SerializeField] private int sfxPoolSize;   // SFX 풀 크기
    
    private List<SFXSource> sfxPool;   // 효과음 풀 리스트
    private int sfxIndex = 0;          // 현재 사용할 인덱스 (라운드로빈)
    
    [Header("Mixer & Groups")]
    [SerializeField] private AudioMixer mixer;         // GameMixer 에셋
    [SerializeField] private AudioMixerGroup masterGroup; // GameMixer/Master 그룹
    [SerializeField] private AudioMixerGroup bgmGroup; // GameMixer/BGM 그룹
    [SerializeField] private AudioMixerGroup sfxGroup; // GameMixer/SFX 그룹
    
    [Header("Mixer Params (Exposed Names)")]
    [SerializeField] private string paramMaster = "MasterVolume";
    [SerializeField] private string paramBGM = "BGMVolume";
    [SerializeField] private string paramSFX = "SFXVolume";
    
    
    // 실행 전에 무조건 AudioManager 생성
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateInstance()
    {
        if (Instance == null) // 아직 인스턴스가 없으면
        {
            var go = new GameObject("AudioManager");   // AudioManager 게임오브젝트 생성
            go.AddComponent<AudioManager>();           // 컴포넌트 붙이기 (싱글톤 초기화는 base.Awake에서)
        }
    }
    
    
    
    // -------------------- BGM --------------------
    private void InitBGMSource()
    {
        if (bgmSource == null) // 인스펙터 미할당 시 런타임 추가
        {
            bgmSource = gameObject.AddComponent<AudioSource>(); // BGM 재생용 AudioSource 추가
            bgmSource.playOnAwake = false; // 자동재생 방지
            bgmSource.loop = true;         // 기본 루프
        }
        if (bgmGroup != null)
            bgmSource.outputAudioMixerGroup = bgmGroup; // Mixer 그룹 연결
    }
    
    public void PlayBGM(AudioClip clip, float volume)
    {
        if (clip == null) return;     // 안전 가드
        bgmSource.clip = clip;        // 재생할 클립 지정
        bgmSource.volume = volume;    // 로컬 볼륨 (Mixer 전 단계)
        bgmSource.Play();             // 재생 시작
    }
    
    public void StopBGM() => bgmSource.Stop(); // BGM 정지
    
    // -------------------- SFX Pool --------------------
    private void InitSFXPool()
    {
        if (sfxPoolSize <= 0)               
        sfxPool = new List<SFXSource>(sfxPoolSize);             // 용량 예약
    
        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject obj = (sfxPrefab != null)
                ? Instantiate(sfxPrefab, transform)             // 프리팹으로 생성
                : new GameObject($"SFX Source {i}");            // 프리팹이 없으면 빈 객체 생성
    
            obj.transform.SetParent(transform, false);          // AudioManager 하위로 정리
    
            var aud = obj.GetComponent<AudioSource>();          // AudioSource 보장
            if (aud == null) aud = obj.AddComponent<AudioSource>();
            aud.playOnAwake = false;                            // 수동 재생
            if (sfxGroup != null) aud.outputAudioMixerGroup = sfxGroup; // Mixer 그룹 연결
    
            var src = obj.GetComponent<SFXSource>();            // SFXSource 보장
            if (src == null) src = obj.AddComponent<SFXSource>();
    
            obj.name = $"SFX Source {i}";                       
            sfxPool.Add(src);                                   
        }
    }
    
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null || sfxPool == null || sfxPool.Count == 0) return; // 안전 가드
        var src = sfxPool[sfxIndex];   // 현재 인덱스의 소스 선택
        src.Play(clip, volume, pitch); // 재생
        sfxIndex = (sfxIndex + 1) % sfxPool.Count; // 다음 인덱스 (라운드로빈)
    }
    
    // 편의 함수: Resources 경로로 SFX 재생
    public void PlaySFXByResource(string path, float volume = 1f, float pitch = 1f)
    {
        var clip = Resources.Load<AudioClip>("Sound/Swing"); // 예: "Sound/Swing"
        if (clip != null) PlaySFX(clip, volume, pitch);
        else Debug.LogWarning($"[AudioManager] SFX not found at path: {path}");
    }
    
    public void StopAllSFX()
    {
        if (sfxPool == null) return;               // 안전 가드
        foreach (var src in sfxPool)
        {
            var aud = src.GetComponent<AudioSource>();
            if (aud != null) aud.Stop();           // 재생 중이면 정지
        }
    }
    
    // -------------------- Mixer Volume API --------------------
    public void SetMasterVolumeLinear(float v) => SetDb(paramMaster, v); // 0~1 선형을 dB로 세팅
    public void SetBGMVolumeLinear(float v) => SetDb(paramBGM, v);
    public void SetSFXVolumeLinear(float v) => SetDb(paramSFX, v);
    
    public float GetMasterVolumeLinear() => GetLinear(paramMaster); // dB를 0~1 선형으로 환산해 반환
    public float GetBGMVolumeLinear() => GetLinear(paramBGM);
    public float GetSFXVolumeLinear() => GetLinear(paramSFX);
    
    private void SetDb(string param, float linear01)
    {
        if (mixer == null || string.IsNullOrEmpty(param)) return; // Mixer/이름 미지정 시 무시
        float value = Mathf.Clamp(linear01, 0.0001f, 1f); // 0은 -∞dB → 안전 가드 값 사용
        float dB = Mathf.Log10(value) * 20f;              // 선형 → dB 변환
        mixer.SetFloat(param, dB);                        // 파라미터 적용
        //Debug.Log("사운드 초기화");
    }
    
    private float GetLinear(string param)
    {
        if (mixer != null && !string.IsNullOrEmpty(param) && mixer.GetFloat(param, out float dB))
        {
            return Mathf.Clamp01(Mathf.Pow(10f, dB / 20f)); // dB → 선형 변환
        }
        return 1f; // 못 찾으면 기본 1
    }
    
    private void LoadMixerAssets()
    {
        mixer = Resources.Load<AudioMixer>("Sound/GameMixer");
    
        if (mixer != null)
        {
            var groups = mixer.FindMatchingGroups(string.Empty);
            foreach (var group in groups)
            {
                if (group.name == "BGM") bgmGroup = group;
    
                else if (group.name == "SFX") sfxGroup = group;
            }
    
        }
    
        if (mixer == null) Debug.LogWarning("[AudioManager] GameMixer not found in Resources/Sound/");
        if (bgmGroup == null) Debug.LogWarning("[AudioManager] BGM group not found in mixer");
        if (sfxGroup == null) Debug.LogWarning("[AudioManager] SFX group not found in mixer");
    }
}