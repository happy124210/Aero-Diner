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
        PlayerPrefs.SetString(BindingSaveKey, path);
    }

    public void LoadBinding()
    {
        if (PlayerPrefs.HasKey(BindingSaveKey))
        {
            string savedPath = PlayerPrefs.GetString(BindingSaveKey);
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
        PlayerPrefs.DeleteKey(BindingSaveKey);
        UpdateKeyText();
    }

    private void UpdateKeyText()
    {
        string displayName = InputControlPath.ToHumanReadableString(
            actionRef.action.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice
        );
        keyText.text = displayName;
    }
}