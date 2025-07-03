using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;

public class UIManager : Singleton<UIManager>
{
    [System.Serializable]
    public class SceneUIEntry
    {
        public string sceneName;
        public List<AssetReferenceGameObject> uiPrefabs;
    }

    [Header("씬별 UI 매핑")]
    [SerializeField] private List<SceneUIEntry> sceneUIPrefabs;

    private Dictionary<string, List<AssetReferenceGameObject>> uiMap;
    private List<GameObject> currentSceneUIs = new();
    public List<GameObject> CurrentSceneUIs => currentSceneUIs;

    private void Awake()
    {
        uiMap = new();
        foreach (var entry in sceneUIPrefabs)
        {
            if (!uiMap.ContainsKey(entry.sceneName))
                uiMap[entry.sceneName] = new();
            uiMap[entry.sceneName].AddRange(entry.uiPrefabs);
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
        // 항상 모든 UI를 비활성화 상태로 생성만 하고, 이후 EventBus로 활성화 제어
        LoadSceneUI(scene.name);
    }

    public async void LoadSceneUI(string sceneName)
    {
        foreach (var ui in currentSceneUIs)
        {
            if (ui != null)
                Destroy(ui);
        }
        currentSceneUIs.Clear();

        if (!uiMap.TryGetValue(sceneName, out var assetRefs))
        {
            Debug.LogWarning($"[UIManager] UI 프리팹을 찾을 수 없음: {sceneName}");
            return;
        }

        foreach (var assetRef in assetRefs)
        {
            var handle = assetRef.InstantiateAsync(transform); // SetActive 상태 유지
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                currentSceneUIs.Add(handle.Result); // 프리팹에서 지정한 활성화 상태 그대로 들어옴
            }
            else
            {
                Debug.LogError($"[UIManager] UI 로드 실패: {sceneName}, Error: {handle.OperationException?.Message}");
            }
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
                foreach (var ui in currentSceneUIs)
                    ui?.GetComponentInChildren<EarningsDisplay>()?.AnimateEarnings((float)payload);
                break;
            case UIEventType.ShowStartMenuWithSave:
                foreach (var ui in currentSceneUIs)
                    ui?.GetComponentInChildren<MenuPanel4>(true)?.gameObject.SetActive(true);
                break;
            case UIEventType.ShowStartMenuNoSave:
                foreach (var ui in currentSceneUIs)
                    ui?.GetComponentInChildren<MenuPanel3>(true)?.gameObject.SetActive(true);
                break;
            case UIEventType.ShowMenuPanel:
                foreach (var ui in currentSceneUIs)
                    ui?.GetComponentInChildren<MenuPanel>(true)?.gameObject.SetActive(true);
                break;
            case UIEventType.UpdateMenuPanel:
                foreach (var ui in currentSceneUIs)
                    ui?.GetComponentInChildren<MenuPanel>(true)?.GenerateFoodList();
                break;
            case UIEventType.HideMenuPanel:
                foreach (var ui in currentSceneUIs)
                    ui?.GetComponentInChildren<MenuPanel>(true)?.gameObject.SetActive(false);
                break;
            case UIEventType.ShowResultPanel:
                foreach (var ui in currentSceneUIs)
                    ui?.GetComponentInChildren<ResultPanel>(true)?.gameObject.SetActive(true);
                break;
            case UIEventType.HideResultPanel:
                foreach (var ui in currentSceneUIs)
                    ui?.GetComponentInChildren<ResultPanel>(true)?.gameObject.SetActive(false);
                break;
            case UIEventType.ShowInventory:
                foreach (var ui in currentSceneUIs)
                    ui?.GetComponentInChildren<Inventory>(true)?.gameObject.SetActive(true);
                break;
            case UIEventType.HideInventory:
                foreach (var ui in currentSceneUIs)
                    ui?.GetComponentInChildren<Inventory>(true)?.gameObject.SetActive(false);
                break;
            case UIEventType.ShowRoundTimer:
            case UIEventType.HideRoundTimer:
                foreach (var ui in currentSceneUIs)
                {
                    var timer = ui?.GetComponentInChildren<RoundTimerUI>(true)?.gameObject;
                    if (timer != null)
                        timer.SetActive(eventType == UIEventType.ShowRoundTimer);
                }
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
