using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : Singleton<FadeManager>
{
    public Image fadeImage;
    public float defaultFadeTime = 1f;
    private Coroutine currentFade;

    private bool isFadePlanned = false;

    protected override void Awake()
    {
        base.Awake();
        if (fadeImage == null)
        {
            Debug.LogError("FadeImage가 연결되지 않았습니다.");
            enabled = false;
            return;
        }

        // 초기화: 완전 검은 화면
        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;

        //최초 진입일 경우 페이드 인
        if (SceneManager.GetActiveScene().name == "StartScene" || SceneManager.GetActiveScene().buildIndex == 0)
        {
            FadeTo(0f, defaultFadeTime);
        }
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
        if (isFadePlanned && fadeImage != null)
        {
            FadeTo(0f, defaultFadeTime);
        }


        //무조건 false로 리셋
        isFadePlanned = false;
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

    public void FadeOutAndLoadSceneWithLoading(string targetScene)
    {
        isFadePlanned = true; //페이드 계획 설정
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
    public void SetFadePlanned(bool planned)
    {
        isFadePlanned = planned;
    }
}
