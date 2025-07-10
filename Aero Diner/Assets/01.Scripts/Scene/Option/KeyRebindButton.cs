using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeyRebindButton : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private TMP_Text keyText;

    [Header("바인딩 설정")]
    public InputActionReference actionRef;
    public int bindingIndex;

    private Button button;
    private bool waitingForKey = false;

    private string originalOverridePath;
    public string BindingSaveKey => $"{actionRef.action.name}_binding_{bindingIndex}";
    public string GetCurrentPath() => actionRef.action.bindings[bindingIndex].effectivePath;
    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(StartRebind);
        UpdateKeyText();
    }

    private void Start()
    {
        LoadBinding();
    }
    void OnEnable()
    {
        UIRoot.Instance.tabButtonController.ApplyTabSelectionVisuals();
    }
    private void StartRebind()
    {
        if (waitingForKey) return;
        EventBus.PlaySFX(SFXType.ButtonClick);
        keyText.text = "Press any key...";
        waitingForKey = true;

        // 먼저 Action 비활성화
        actionRef.action.Disable();

        originalOverridePath = actionRef.action.bindings[bindingIndex].overridePath;


        actionRef.action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(op =>
            {
                op.Dispose();
                waitingForKey = false;
                actionRef.action.Enable(); // 다시 활성화
                UpdateKeyText();
            })
            .OnCancel(op =>
            {
                op.Dispose();
                waitingForKey = false;
                actionRef.action.Enable(); // 다시 활성화
                UpdateKeyText();
            })
            .Start();
    }
    public void SaveBinding()
    {
        var path = actionRef.action.bindings[bindingIndex].effectivePath;
        var data = SaveLoadManager.LoadGame() ?? new SaveData();
        data.keyBindings[BindingSaveKey] = path;
        SaveLoadManager.SaveGame(data);
    }

    public void LoadBinding()
    {
        var data = SaveLoadManager.LoadGame();
        if (data != null && data.keyBindings.TryGetValue(BindingSaveKey, out string savedPath))
        {
            actionRef.action.ApplyBindingOverride(bindingIndex, savedPath);
        }
        UpdateKeyText();
    }


    public void RevertToOriginal()
    {
        if (!string.IsNullOrEmpty(originalOverridePath))
            actionRef.action.ApplyBindingOverride(bindingIndex, originalOverridePath);
        else
            actionRef.action.RemoveBindingOverride(bindingIndex);
        
        UpdateKeyText();
    }

    public void ResetToDefault()
    {
        actionRef.action.RemoveBindingOverride(bindingIndex);

        string defaultPath = actionRef.action.bindings[bindingIndex].path;
        if (string.IsNullOrEmpty(defaultPath))
        {
            Debug.LogError($"[KeyRebindButton] 기본 바인딩 경로가 비어 있음! action: {actionRef.action.name}, index: {bindingIndex}");
        }

        var data = SaveLoadManager.LoadGame() ?? new SaveData();
        data.keyBindings[BindingSaveKey] = defaultPath;
        SaveLoadManager.SaveGame(data);

        Debug.Log($"[KeyRebindButton] 기본값 저장됨: {BindingSaveKey} → {defaultPath}");

        UpdateKeyText();
    }


    public void UpdateKeyText()
    {
        string displayName = InputControlPath.ToHumanReadableString(
            actionRef.action.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice
        );
        keyText.text = displayName;
    }
    public string actionName => actionRef.action.name;

    public string GetCurrentBinding()
    {
        return actionRef.action.bindings[bindingIndex].effectivePath;
    }
}