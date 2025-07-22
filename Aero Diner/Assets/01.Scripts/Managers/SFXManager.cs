using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SFXManager : Singleton<SFXManager>
{
    [System.Serializable]
    public class SFXEntry
    {
        public SFXType type;
        public AudioClip clip;
    }

    [Header("SFX 데이터")]
    [SerializeField] private List<SFXEntry> sfxList;
    [SerializeField] private int poolSize = 10;

    [Header("디버그")]
    [SerializeField] private bool showDebugInfo = false;

    private Dictionary<SFXType, AudioClip> sfxDict;
    private Queue<AudioSource> audioPool;
    private AudioSource baseAudioSource; // 볼륨 설정용 기준
    private Dictionary<SFXType, AudioSource> loopSources = new();
    private SFXType? currentLoopType = null;
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        InitSFXDict();
        InitAudioPool();
    }

    private void InitSFXDict()
    {
        sfxDict = new Dictionary<SFXType, AudioClip>();
        foreach (var entry in sfxList)
        {
            if (entry.clip != null && !sfxDict.ContainsKey(entry.type))
                sfxDict.Add(entry.type, entry.clip);
        }
    }

    private void InitAudioPool()
    {
        audioPool = new Queue<AudioSource>();

        baseAudioSource = gameObject.AddComponent<AudioSource>();
        baseAudioSource.playOnAwake = false;

        for (int i = 0; i < poolSize; i++)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            audioPool.Enqueue(source);
        }
    }

    private void OnEnable()
    {
        EventBus.OnSFXRequested += HandleSFXRequest;
        EventBus.OnLoopSFXRequested += PlayLoop;
        EventBus.OnStopLoopSFXRequested += StopLoop;
    }

    private void OnDisable()
    {
        EventBus.OnSFXRequested -= HandleSFXRequest;
        EventBus.OnLoopSFXRequested -= PlayLoop;
        EventBus.OnStopLoopSFXRequested -= StopLoop;
    }

    private void HandleSFXRequest(SFXType type)
    {
        if (showDebugInfo)
            Debug.Log($"[SFXManager] SFX 요청 받음: {type}");

        if (!sfxDict.TryGetValue(type, out var clip) || clip == null)
        {
            if (showDebugInfo)
                Debug.LogWarning($"[SFXManager] {type}에 해당하는 clip이 null입니다.");
            return;
        }

        var source = GetAvailableSource();
        source.volume = baseAudioSource.volume;
        source.PlayOneShot(clip);

        if (showDebugInfo)
            Debug.Log($"[SFXManager] {type} 사운드 재생 시작");
    }

    private AudioSource GetAvailableSource()
    {
        foreach (var source in audioPool)
        {
            if (!source.isPlaying)
                return source;
        }

        // 전부 재생 중이면 새로운 AudioSource 추가
        var extraSource = gameObject.AddComponent<AudioSource>();
        extraSource.playOnAwake = false;
        audioPool.Enqueue(extraSource);

        if (showDebugInfo)
            Debug.LogWarning("[SFXManager] AudioSource 부족 → 새로 생성");

        return extraSource;
    }

    public void SetVolume(float volume)
    {
        baseAudioSource.volume = volume;
        if (showDebugInfo)
            Debug.Log($"[SFXManager] 볼륨 설정: {volume}");
    }
    private void PlayLoop(SFXType type)
    {
        if (!sfxDict.TryGetValue(type, out var clip) || clip == null)
            return;

        if (!loopSources.TryGetValue(type, out var source))
        {
            source = gameObject.AddComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            loopSources[type] = source;
        }

        if (!source.isPlaying)
        {
            source.clip = clip;
            source.volume = baseAudioSource.volume;
            source.Play();

            if (showDebugInfo)
                Debug.Log($"[SFXManager] 루프 SFX 시작: {type}");
        }
    }

    public void StopLoop(SFXType type)
    {
        if (loopSources.TryGetValue(type, out var source) && source.isPlaying)
        {
            source.Stop();

            if (showDebugInfo)
                Debug.Log($"[SFXManager] 루프 SFX 정지: {type}");
        }
    }
    public void StopAllLoops()
    {
        foreach (var source in loopSources.Values)
        {
            if (source.isPlaying)
                source.Stop();
        }

        if (showDebugInfo)
            Debug.Log("[SFXManager] 모든 루프 SFX 정지");
    }

}
