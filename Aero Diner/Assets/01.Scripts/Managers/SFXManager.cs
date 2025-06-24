using UnityEngine;
using System.Collections.Generic;

public class SFXManager : Singleton<SFXManager>
{
    public static SFXManager Instance;

    [System.Serializable]
    public class SFXEntry
    {
        public SFXType type;
        public AudioClip clip;
    }

    public List<SFXEntry> sfxList;
    private Dictionary<SFXType, AudioClip> sfxDict;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        sfxDict = new Dictionary<SFXType, AudioClip>();
        foreach (var entry in sfxList)
        {
            sfxDict[entry.type] = entry.clip;
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
        if (sfxDict.TryGetValue(type, out var clip))
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"[SFXManager] 등록되지 않은 SFXType: {type}");
        }
    }

    //호출 예시
    //SFXEventBus.PlaySFX(SFXType.ItemPickup);
}
