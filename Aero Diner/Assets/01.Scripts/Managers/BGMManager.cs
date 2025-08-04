using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class BGMManager : Singleton<BGMManager>
{
    private Tween currentFadeTween; // 현재 재생 중인 페이드 트윈
    private float fadeDuration = 0.5f; // 기본 페이드 시간
    private float targetVolume = 1f;
   
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
        if (type == BGMEventType.StopBGM)
        {
            FadeOutAndStop();
            return;
        }

        var entry = bgmClips.Find(b => b.type == type);
        if (entry == null || entry.bgmClip == null)
        {
            Debug.LogWarning($"[BGMManager] {type}에 해당하는 BGM이 없습니다.");
            return;
        }

        // 여기에 BGMEventType별로 페이드 유무를 지정
        bool useFade = type switch
        {
            BGMEventType.Intro1 => true,
            BGMEventType.Intro2 => true,
            BGMEventType.Intro3 => true,
            _ => false
        };

        PlayBGM(entry.bgmClip, useFade);
    }

    public void PlayBGM(AudioClip clip, bool useFade = true)
    {
        if (audioSource.clip == clip && audioSource.isPlaying)
            return;
        audioSource.loop = true;
        if (useFade)
        {
            FadeOutAndPlayNew(clip);
        }
        else
        {
            currentFadeTween?.Kill();
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.volume = targetVolume;
            audioSource.Play();
        }
    }
    private void FadeOutAndPlayNew(AudioClip newClip)
    {
        currentFadeTween?.Kill();

        currentFadeTween = DOTween.To(() => audioSource.volume, x => audioSource.volume = x, 0f, fadeDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                audioSource.Stop();
                audioSource.clip = newClip;
                audioSource.volume = 0f;
                audioSource.loop = true;
                audioSource.Play();

                currentFadeTween = DOTween.To(() => 0f, x => audioSource.volume = x, targetVolume, fadeDuration)
                    .SetEase(Ease.InQuad);
            });
    }
    private void FadeOutAndStop()
    {
        currentFadeTween?.Kill();

        currentFadeTween = DOTween.To(() => audioSource.volume, x => audioSource.volume = x, 0f, fadeDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                audioSource.Stop();
                audioSource.clip = null;
            });
    }

    public void SetVolume(float volume)
    {
        targetVolume = volume;            
        audioSource.volume = volume;    
    }

    public float GetVolume()
    {
        return audioSource.volume;
    }
}