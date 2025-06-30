using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class UIManager : Singleton<UIManager>
{
    // 씬별 UI
    private GameObject currentSceneUI;

    private void OnEnable()
    {
        EventBus.OnUIEvent += HandleUIEvent;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        EventBus.OnUIEvent -= HandleUIEvent;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬 로드 후 UI 자동 로드
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;
        LoadSceneUI(sceneName);
    }

    // Addressable을 통한 씬별 UI 로딩
    public async void LoadSceneUI(string sceneName)
    {
        if (currentSceneUI)
        {
            Destroy(currentSceneUI);
            currentSceneUI = null;
        }

        var handle = Addressables.InstantiateAsync($"UICanvas_{sceneName}", transform);

        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            currentSceneUI = handle.Result;
        }
        else
        {
            Debug.LogError($"UI Load 실패: {sceneName}, Error: {handle.OperationException?.Message}");
        }
    }

    // UI 이벤트 처리
    private void HandleUIEvent(UIEventType eventType, object payload)
    {
        switch (eventType)
        {
            // Pause 관련
            case UIEventType.OpenPause:
                UIRoot.Instance.pausePanel?.SetActive(true);
                break;
            case UIEventType.ClosePause:
                UIRoot.Instance.pausePanel?.SetActive(false);
                break;

            // Option 패널 열기/닫기
            case UIEventType.OpenOption:
                UIRoot.Instance.pausePanel?.SetActive(false);
                UIRoot.Instance.optionPanel?.SetActive(true);
                break;
            case UIEventType.CloseOption:
                UIRoot.Instance.optionPanel?.SetActive(false);
                if (SceneManager.GetActiveScene().name != "StartScene")
                    UIRoot.Instance.pausePanel?.SetActive(true);
                break;

            // 하위 탭 패널들 (선택적으로 확장 가능)
            case UIEventType.ShowSoundTab:
                UIRoot.Instance.volumeHandler?.gameObject.SetActive(true);
                UIRoot.Instance.videoSettingPanel?.gameObject.SetActive(false);
                UIRoot.Instance.keyRebindManager?.gameObject.SetActive(false);
                break;
            case UIEventType.ShowVideoTab:
                UIRoot.Instance.volumeHandler?.gameObject.SetActive(false);
                UIRoot.Instance.videoSettingPanel?.gameObject.SetActive(true);
                UIRoot.Instance.keyRebindManager?.gameObject.SetActive(false);
                break;
            case UIEventType.ShowControlTab:
                UIRoot.Instance.volumeHandler?.gameObject.SetActive(false);
                UIRoot.Instance.videoSettingPanel?.gameObject.SetActive(false);
                UIRoot.Instance.keyRebindManager?.gameObject.SetActive(true);
                break;

            // 수익 UI 업데이트
            case UIEventType.UpdateEarnings:
                currentSceneUI?.GetComponentInChildren<EarningsDisplay>()?.AnimateEarnings((float)payload);
                break;

            // 라운드 타이머
            case UIEventType.ShowRoundTimer:
            case UIEventType.HideRoundTimer:
                var timer = currentSceneUI?.GetComponentInChildren<RoundTimerUI>(true)?.gameObject;
                if (timer != null)
                    timer.SetActive(eventType == UIEventType.ShowRoundTimer);
                break;

            // 게임 종료
            case UIEventType.QuitGame:
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
        }
    }
}
