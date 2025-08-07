using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections;

public class FadeManager : Singleton<FadeManager>
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float defaultFadeTime = 1f;

    public static event Action OnFadeCompleted;
    
    private Coroutine currentFade;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        if (fadeImage == null)
        {
            Debug.LogError("[FadeManager] fadeImage가 연결되지 않았습니다.");
            enabled = false;
            return;
        }

        // 씬 진입 시 검은 화면으로 시작
        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;
    }

    private void OnEnable()
    {
        EventBus.OnFadeRequested += HandleFadeRequest;
    }

    private void OnDisable()
    {
        EventBus.OnFadeRequested -= HandleFadeRequest;
    }

    private void HandleFadeRequest(FadeEventType type, FadeEventPayload payload)
    {
        float alpha = payload?.targetAlpha ?? 1f;
        float duration = payload?.duration ?? -1f;
        string sceneName = payload?.targetScene;

        switch (type)
        {
            case FadeEventType.FadeIn:
                FadeTo(0f, duration);
                break;
            case FadeEventType.FadeOut:
                FadeTo(1f, duration);
                break;
            case FadeEventType.FadeTo:
                FadeTo(alpha, duration);
                break;
            case FadeEventType.FadeOutAndLoadScene:
                if (!string.IsNullOrEmpty(sceneName))
                    StartCoroutine(FadeOutAndLoadScene(sceneName));
                break;
            case FadeEventType.FadeToSceneDirect:
                if (!string.IsNullOrEmpty(sceneName))
                    StartCoroutine(FadeAndLoadScene(sceneName));
                break;
        }
    }

    public void FadeTo(float targetAlpha, float duration = -1f)
    {
        if (Mathf.Approximately(fadeImage.color.a, targetAlpha))
            return;

        if (currentFade != null)
            StopCoroutine(currentFade);

        if (duration < 0f)
            duration = defaultFadeTime;

        currentFade = StartCoroutine(FadeRoutine(targetAlpha, duration));
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        Color color = fadeImage.color;
        float startAlpha = color.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        fadeImage.color = color;
        currentFade = null;

        OnFadeCompleted?.Invoke();
    }

    private IEnumerator FadeOutAndLoadScene(string targetScene)
    {
        yield return StartCoroutine(FadeRoutine(1f, defaultFadeTime));

        //다음에 로드할 씬 이름 설정
        LoadingTargetHolder.TargetScene = targetScene;

        //로딩 씬으로 전환
        SceneManager.LoadScene("LoadingScene");
    }

    private IEnumerator FadeAndLoadScene(string targetScene)
    {
        yield return StartCoroutine(FadeRoutine(1f, defaultFadeTime));
        SceneManager.LoadScene(targetScene);
    }
}