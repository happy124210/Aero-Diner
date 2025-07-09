using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class BGMManager : Singleton<BGMManager>
{
    [System.Serializable]
    public class NamedBGM
    {
        public BGMEventType type;
        public AudioClip bgmClip;
    }

    public List<NamedBGM> bgmClips = new();

    private AudioSource audioSource;
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        EventBus.OnBGMRequested += HandleBGMEvent;
    }

    private void OnDisable()
    {
        EventBus.OnBGMRequested -= HandleBGMEvent;
    }



    private void HandleBGMEvent(BGMEventType type)
    {
        var entry = bgmClips.Find(b => b.type == type);
        if (entry != null && entry.bgmClip != null)
        {
            if (audioSource.clip != entry.bgmClip)
            {
                audioSource.clip = entry.bgmClip;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else if (type == BGMEventType.StopBGM)
        {
            audioSource.Stop();
        }
        else
        {
            Debug.LogWarning($"[BGMManager] {type}에 해당하는 BGM이 없습니다.");
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