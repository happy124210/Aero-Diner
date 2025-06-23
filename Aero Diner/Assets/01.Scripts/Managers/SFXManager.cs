using UnityEngine;
using System.Collections.Generic;

public class SFXManager : Singleton<SFXManager>
{
    public static SFXManager Instance;

    [Header("효과음 목록")]
    public List<SFXEntry> sfxList = new List<SFXEntry>();

    private AudioSource audioSource;
    private Dictionary<string, AudioClip> sfxDict = new Dictionary<string, AudioClip>();

    [System.Serializable]
    public class SFXEntry
    {
        public string key;
        public AudioClip clip;
    }

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        foreach (var entry in sfxList)
        {
            if (!sfxDict.ContainsKey(entry.key))
                sfxDict.Add(entry.key, entry.clip);
        }
    }

    /// <summary>
    /// 키 값으로 효과음을 재생합니다.
    /// </summary>
    public void Play(string key)
    {
        if (sfxDict.TryGetValue(key, out AudioClip clip))
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"[SFXManager] '{key}' 키에 해당하는 효과음이 없습니다.");
        }
    }

    /// <summary>
    /// 특정 위치에서 효과음을 재생합니다 (3D 환경에서).
    /// </summary>
    public void PlayAt(string key, Vector3 position)
    {
        if (sfxDict.TryGetValue(key, out AudioClip clip))
        {
            AudioSource.PlayClipAtPoint(clip, position);
        }
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }

    public float GetVolume()
    {
        return audioSource.volume;
    }
}
