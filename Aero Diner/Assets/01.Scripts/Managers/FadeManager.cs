using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : Singleton<FadeManager>
{
    public Image fadeImage;
    public float defaultFadeTime = 1f;
    private Coroutine currentFade;

    protected override void Awake()
    {
        base.Awake();
        if (fadeImage == null)
        {
            Debug.LogError("FadeImage가 연결되지 않았습니다.");
            enabled = false;
            return;
        }

        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;
    }
    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame(); // 화면이 준비된 뒤 실행
        FadeTo(0f, defaultFadeTime);          // 페이드 인
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
        Debug.Log($"[FadeManager] 씬 로드됨: {scene.name}, 페이드 인 시작");
        if (fadeImage)
            FadeTo(0f, defaultFadeTime);
    }

    public void FadeTo(float targetAlpha, float duration = -1f)
    {
        if (currentFade != null)
            StopCoroutine(currentFade);
        if (duration < 0f) duration = defaultFadeTime;
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
    }

    /// <summary>
    /// 로딩창 포함 씬 불러오기
    /// </summary>
    /// <param name="targetScene">불러올 씬 이름</param>
    public void FadeOutAndLoadSceneWithLoading(string targetScene)
    {
        StartCoroutine(FadeAndLoadLoadingScene(targetScene));
    }

    private IEnumerator FadeAndLoadLoadingScene(string targetScene)
    {
        yield return StartCoroutine(FadeToCoroutine(1f)); // 어두워질 때까지 대기
        LoadingTargetHolder.TargetScene = targetScene;
        SceneManager.LoadScene("LoadingScene");
    }

    public IEnumerator FadeToCoroutine(float targetAlpha, float duration = -1f)
    {
        if (currentFade != null)
            StopCoroutine(currentFade);
        if (duration < 0f) duration = defaultFadeTime;

        yield return StartCoroutine(FadeRoutine(targetAlpha, duration));
    }
}