using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    public Slider progressBar;
    private string targetScene;
    private bool fadeCompleted;
    private bool isClicked;

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
            elapsed += Time.deltaTime;

            float rawProgress = asyncOp.progress / 0.9f;
            float timeBasedProgress = Mathf.Clamp01(elapsed / minLoadTime);

            float displayProgress = Mathf.Min(rawProgress, timeBasedProgress);
            progressBar.value = displayProgress;

            yield return null;
        }

        // 로딩 완료 처리
        progressBar.value = 1f;
        isClicked = false;

        // yield return new WaitUntil(() => isClicked);
        StartCoroutine(WaitForClickThenFadeOutAndActivate(asyncOp));
    }

    private void Update()
     {
         if (!isClicked && Input.GetMouseButtonDown(0))
         { 
             isClicked = true;
         }
     }
    private IEnumerator WaitForClickThenFadeOutAndActivate(AsyncOperation asyncOp)
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