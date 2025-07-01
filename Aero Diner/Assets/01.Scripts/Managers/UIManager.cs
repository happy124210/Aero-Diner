using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;

public class UIManager : Singleton<UIManager>
{
    [Header("씬별 UI 매핑")]
    [SerializeField] private List<SceneUIEntry> sceneUIPrefabs;

    private Dictionary<string, AssetReference> uiMap;
    private GameObject currentSceneUI;

    private void Awake()
    {
        uiMap = new Dictionary<string, AssetReference>();
        foreach (var entry in sceneUIPrefabs)
        {
            if (!uiMap.ContainsKey(entry.sceneName))
                uiMap.Add(entry.sceneName, entry.uiPrefab);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        EventBus.OnUIEvent += HandleUIEvent;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        EventBus.OnUIEvent -= HandleUIEvent;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadSceneUI(scene.name);
    }

    public async void LoadSceneUI(string sceneName)
    {
        if (currentSceneUI != null)
        {
            Destroy(currentSceneUI);
            currentSceneUI = null;
        }

        if (!uiMap.TryGetValue(sceneName, out var assetRef))
        {
            Debug.LogWarning($"[UIManager] UI 프리팹을 찾을 수 없음: {sceneName}");
            return;
        }

        var handle = assetRef.InstantiateAsync(transform);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            currentSceneUI = handle.Result;
        }
        else
        {
            Debug.LogError($"[UIManager] UI 로드 실패: {sceneName}, Error: {handle.OperationException?.Message}");
        }
    }

    private void HandleUIEvent(UIEventType eventType, object payload)
    {
        // 기존 코드 그대로 유지
        switch (eventType)
        {
            case UIEventType.OpenPause:
                UIRoot.Instance.pausePanel?.SetActive(true);
                break;
            case UIEventType.ClosePause:
                UIRoot.Instance.pausePanel?.SetActive(false);
                break;
            case UIEventType.OpenOption:
                UIRoot.Instance.pausePanel.SetActive(false);
                UIRoot.Instance.optionPanel.SetActive(true);
                UIRoot.Instance.volumePanel.gameObject.SetActive(true);
                break;
            case UIEventType.CloseOption:
                UIRoot.Instance.optionPanel.SetActive(false);
                if (SceneManager.GetActiveScene().name != "StartScene")
                    UIRoot.Instance.pausePanel.SetActive(true);
                break;
            case UIEventType.ShowSoundTab:
                UIRoot.Instance.volumePanel.gameObject.SetActive(true);
                UIRoot.Instance.videoPanel.gameObject.SetActive(false);
                UIRoot.Instance.keysettingPanel.gameObject.SetActive(false);
                break;
            case UIEventType.ShowVideoTab:
                UIRoot.Instance.volumePanel.gameObject.SetActive(false);
                UIRoot.Instance.videoPanel.gameObject.SetActive(true);
                UIRoot.Instance.keysettingPanel.gameObject.SetActive(false);
                break;
            case UIEventType.ShowControlTab:
                UIRoot.Instance.volumePanel.gameObject.SetActive(false);
                UIRoot.Instance.videoPanel.gameObject.SetActive(false);
                UIRoot.Instance.keysettingPanel.gameObject.SetActive(true);
                break;
            case UIEventType.UpdateEarnings:
                currentSceneUI?.GetComponentInChildren<EarningsDisplay>()?.AnimateEarnings((float)payload);
                break;
            case UIEventType.ShowStartMenuWithSave:
                currentSceneUI?.GetComponentInChildren<MenuPanel4>(true)?.gameObject.SetActive(true);
                break;
            case UIEventType.ShowStartMenuNoSave:
                currentSceneUI?.GetComponentInChildren<MenuPanel3>(true)?.gameObject.SetActive(true);
                break;
            case UIEventType.ShowRoundTimer:
            case UIEventType.HideRoundTimer:
                var timer = currentSceneUI?.GetComponentInChildren<RoundTimerUI>(true)?.gameObject;
                if (timer != null)
                    timer.SetActive(eventType == UIEventType.ShowRoundTimer);
                break;
            case UIEventType.LoadMainScene:
                FadeManager.Instance.FadeOutAndLoadSceneWithLoading("MainScene");
                break;
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
