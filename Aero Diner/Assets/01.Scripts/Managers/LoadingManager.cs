using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public Slider progressBar;
    private string targetScene;
    private bool fadeCompleted;

    private void Start()
    {
        EventBus.PlayBGM(BGMEventType.StopBGM);

        targetScene = LoadingTargetHolder.TargetScene;
        StartCoroutine(LoadSceneAsync());
        EventBus.RaiseFadeEvent(FadeEventType.FadeIn, new FadeEventPayload(0f, 1f));
    }

    private IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(targetScene);
        asyncOp.allowSceneActivation = false;

        float elapsed = 0f;
        float minLoadTime = 2f;

        while (asyncOp.progress < 0.9f || elapsed < minLoadTime)
        {
            elapsed += Time.unscaledDeltaTime; //타임스케일 영향 방지(선택)
            float rawProgress = asyncOp.progress / 1f;              // 0~1 정규화
            float timeBasedProgress = Mathf.Clamp01(elapsed / minLoadTime);
            float displayProgress = Mathf.Min(rawProgress, timeBasedProgress);
            progressBar.value = displayProgress;
            yield return null;
        }

        // 로딩 종료: 바를 확실히 1.0으로 보여주고 한 프레임 양보
        progressBar.value = 1f;
        yield return null;

        // 바로 페이드아웃 → 씬 활성화
        StartCoroutine(FadeOutThenActivate(asyncOp));
    }

    private IEnumerator FadeOutThenActivate(AsyncOperation asyncOp)
    {
        fadeCompleted = false;
        FadeManager.OnFadeCompleted += OnFadeComplete;

        EventBus.RaiseFadeEvent(
            FadeEventType.FadeOut,
            new FadeEventPayload(alpha: 1f, duration: 1.5f, autoFade: true)
        );

        yield return new WaitUntil(() => fadeCompleted);

        FadeManager.OnFadeCompleted -= OnFadeComplete;
        asyncOp.allowSceneActivation = true;
    }

    private void OnFadeComplete()
    {
        fadeCompleted = true;
    }
}
