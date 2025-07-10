using UnityEngine;
using System.Collections.Generic;

public class SFXManager : Singleton<SFXManager>
{

    [System.Serializable]
    public class SFXEntry
    {
        public SFXType type;
        public AudioClip clip;
    }

    public List<SFXEntry> sfxList;
    private Dictionary<SFXType, AudioClip> sfxDict;
    [SerializeField] private AudioSource audioSource;
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo;
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        sfxDict = new Dictionary<SFXType, AudioClip>();
        foreach (var entry in sfxList)
        {
            sfxDict[entry.type] = entry.clip;
        }
        foreach (var entry in sfxDict)
        {

        }
    }

    private void OnEnable()
    {
        EventBus.OnSFXRequested += HandleSFXRequest;
    }

    private void OnDisable()
    {
        EventBus.OnSFXRequested -= HandleSFXRequest;
    }

    private void HandleSFXRequest(SFXType type)
    {
        if (showDebugInfo)
            Debug.Log($"[SFXManager] SFX 요청 받음: {type}");

        if (sfxDict == null)
        {
            if (showDebugInfo)
                Debug.LogError("[SFXManager] sfxDict가 null입니다! Awake()가 제대로 호출되지 않았을 수 있습니다.");
            return;
        }

        if (sfxDict.TryGetValue(type, out var clip))
        {
            if (clip == null)
            {
            }
            else
            {
                audioSource.PlayOneShot(clip);
            }
        }
        else
        {
            Debug.LogWarning($"[SFXManager] 등록되지 않은 SFXType: {type}");
        }
    }
    public void SetVolume(float volume)
    {
        if (audioSource != null)
            audioSource.volume = volume;
    }

    public float GetVolume()
    {
        return audioSource != null ? audioSource.volume : 0f;
    }
    //호출 예시
    //SFXEventBus.PlaySFX(SFXType.ItemPickup);
}
