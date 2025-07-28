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
    private bool fadeCompleted = false;
    private bool isReadyToActivate = false;
    private bool isClicked = false;

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

        // 로딩 진행도 0 ~ 0.9 동안 업데이트
        while (asyncOp.progress < 0.9f)
        {
            float progress = Mathf.Clamp01(asyncOp.progress / 0.9f);
            progressBar.value = progress;
            yield return null;
        }

        // 로딩 완료 → 진행바 100%, 텍스트 활성화
        progressBar.value = 1f;
          // 클릭 대기 전 준비
        isClicked = false;
           // 예: "화면을 클릭하세요" 텍스트 표시 등 UI 처리
        Debug.Log("로딩 완료. 클릭을 기다립니다.");

        yield return new WaitUntil(() => isClicked); // 클릭 기다림
        // 클릭 시점까지 대기 & 전환
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
            new FadeEventPayload(alpha: 1f, duration: 1f, autoFade: true)
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