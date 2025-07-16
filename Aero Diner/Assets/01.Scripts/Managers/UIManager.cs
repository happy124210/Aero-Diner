using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;

public class UIManager : Singleton<UIManager>
{
    //이 리스트에 있는 UI는 모두 비활성화 상태로 시작.
    private readonly System.Type[] initiallyDisabledTypes = new System.Type[]
{
    typeof(ResultPanel),
    typeof(MenuPanel3),
    typeof(MenuPanel4),
    typeof(Inventory),
    // 필요한 타입 추가 가능
};

    private List<IUIEventHandler> uiHandlers = new();
    [System.Serializable]
    public class SceneUIEntry
    {
        public string sceneName;
        public List<AssetReferenceGameObject> uiPrefabs;
    }
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo;

    [Header("씬별 UI 매핑")]
    [SerializeField] private List<SceneUIEntry> sceneUIPrefabs;

    private Dictionary<string, List<AssetReferenceGameObject>> uiMap;
    private List<GameObject> currentSceneUIs = new();
    public List<GameObject> CurrentSceneUIs => currentSceneUIs;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

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
            if (showDebugInfo)
                Debug.LogWarning($"[UIManager] UI 프리팹을 찾을 수 없음: {sceneName}");
            return;
        }

        foreach (var assetRef in assetRefs)
        {
            var handle = assetRef.InstantiateAsync(transform);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var instance = handle.Result;
                instance.SetActive(true);
                currentSceneUIs.Add(instance);
            }
            else
            {
                if (showDebugInfo)
                    Debug.LogError($"[UIManager] UI 로딩 실패: {assetRef.RuntimeKey}");
            }
        }
        foreach (var ui in currentSceneUIs)
        {
            foreach (var type in initiallyDisabledTypes)
            {
                var target = ui.GetComponentInChildren(type, true) as MonoBehaviour;
                if (target != null)
                {
                    target.gameObject.SetActive(false);
                }
            }
        }
        if (showDebugInfo)
            Debug.Log($"[UIManager] {sceneName} 씬 UI 로딩 시작, 프리팹 수: {assetRefs.Count}");
        if (sceneName == "StartScene")
        {
            foreach (var ui in currentSceneUIs)
            {
                var blinker = ui.GetComponentInChildren<PressAnyKeyBlinker>(true);
                if (blinker != null && !blinker.gameObject.activeSelf)
                    if (showDebugInfo)
                        Debug.Log($"[UIManager] PressAnyKeyBlinker 찾음: {blinker.name}, activeSelf: {blinker.gameObject.activeSelf}");
                {
                    blinker.gameObject.SetActive(true);
                    if (showDebugInfo)
                        Debug.Log("[UIManager] PressAnyKeyBlinker 강제 활성화");
                }
            }
        }
        RegisterHandlersForScene(SceneManager.GetActiveScene().name);
    }
    private void RegisterHandlersForScene(string sceneName)
    {
        uiHandlers.Clear();

        // 공통 핸들러 (항상 등록)
        uiHandlers.Add(new OptionPanelHandler());

        // 씬별 핸들러
        if (sceneName == "StartScene")
        {
            uiHandlers.Add(new StartSceneUIHandler(currentSceneUIs));
        }
        else if (sceneName == "MainScene")
        {
            uiHandlers.Add(new MainSceneUIHandler(currentSceneUIs));
        }

        // 필요 시 다른 씬별 핸들러도 추가
    }
    private void HandleUIEvent(UIEventType type, object payload)
    {
        foreach (var handler in uiHandlers)
        {
            if (handler.Handle(type, payload))
                break; // 이벤트 처리 완료된 핸들러 있으면 종료
        }
    }
}
