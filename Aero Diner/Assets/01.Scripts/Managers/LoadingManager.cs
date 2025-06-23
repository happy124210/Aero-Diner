using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public Slider progressBar;
    private string targetScene;

    private void Start()
    {
        targetScene = LoadingTargetHolder.TargetScene;
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(targetScene);
        asyncOp.allowSceneActivation = false;

        while (asyncOp.progress < 0.9f)
        {
            float progress = Mathf.Clamp01(asyncOp.progress / 0.9f);
            progressBar.value = progress;
            yield return null;
        }
        // 로딩 완료 상태
        progressBar.value = 1f;
        // 사용자 입력 대기
        yield return new WaitUntil(() => Input.anyKeyDown);
        // 여기서 페이드 아웃 (어두워지기)
        yield return StartCoroutine(FadeManager.Instance.FadeToCoroutine(1f));
        // 다 어두워졌으면 씬 전환
        asyncOp.allowSceneActivation = true;
    }
}
