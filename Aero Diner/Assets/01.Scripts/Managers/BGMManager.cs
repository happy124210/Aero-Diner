using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class BGMManager : Singleton<BGMManager>
{
    [Header("씬별 배경음악")]
    public List<SceneBGM> sceneBGMs = new List<SceneBGM>();

    private AudioSource audioSource;

    [System.Serializable]
    public class SceneBGM
    {
        public string sceneName;
        public AudioClip bgmClip;
    }

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBGMForScene(scene.name);
    }

    private void PlayBGMForScene(string sceneName)
    {
        var entry = sceneBGMs.Find(b => b.sceneName == sceneName);
        if (entry != null && entry.bgmClip != null)
        {
            if (audioSource.clip != entry.bgmClip)
            {
                audioSource.clip = entry.bgmClip;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            Debug.LogWarning($"[BGMManager] {sceneName}에 해당하는 BGM이 설정되지 않았습니다.");
            audioSource.Stop();
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