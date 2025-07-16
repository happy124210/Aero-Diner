using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeyRebindManager : MonoBehaviour
{
    [SerializeField] private List<KeyRebindButton> rebindButtons;

    private bool isSaved = false;

    private void Start()
    {
        var data = SaveLoadManager.LoadGame() ?? new SaveData();

        if (data.keyBindings == null || data.keyBindings.Count == 0)
        {
            ResetAll();
        }
        else
        {
            ApplyKeyBindings(data.keyBindings);
        }
    }
    public bool HasUnsavedChanges()
    {
        var data = SaveLoadManager.LoadGame();
        if (data == null)
        {
            Debug.LogWarning("[KeyRebindManager] 저장파일 없음 → 변경사항 있음으로 간주");
            return true;
        }

        bool hasChanges = false;

        foreach (var btn in rebindButtons)
        {
            var path = btn.GetCurrentPath();
            if (!data.keyBindings.TryGetValue(btn.BindingSaveKey, out string savedPath))
            {
                Debug.LogWarning($"[KeyRebindManager] '{btn.BindingSaveKey}'에 대한 저장값이 없음");
                hasChanges = true;
            }
            else if (path != savedPath)
            {
                Debug.LogWarning($"[KeyRebindManager] 키 바인딩 변경됨: '{btn.BindingSaveKey}' 현재='{path}' / 저장='{savedPath}'");
                hasChanges = true;
            }
        }

        return hasChanges;
    }
    public void SaveAll()
    {
        var data = SaveLoadManager.LoadGame() ?? new SaveData();

        foreach (var btn in rebindButtons)
        {
            var path = btn.GetCurrentPath();
            data.keyBindings[btn.BindingSaveKey] = path;
        }

        SaveLoadManager.SaveGame(data);
        isSaved = true;
    }
    public void CancelAll()
    {
        if (isSaved)
        {
            isSaved = false; // 상태 초기화
            return;
        }

        foreach (var btn in rebindButtons)
        {
            btn.RevertToOriginal();
        }
    }

    public void ResetAll()
    {
        var data = SaveLoadManager.LoadGame() ?? new SaveData();

        foreach (var btn in rebindButtons)
        {
            btn.actionRef.action.RemoveBindingOverride(btn.bindingIndex);
            string defaultPath = btn.GetCurrentPath(); // Override 제거 후 effectivePath == 기본값
            data.keyBindings[btn.BindingSaveKey] = defaultPath;
            btn.UpdateKeyText();
        }

        SaveLoadManager.SaveGame(data);
        isSaved = true;

    }
    public Dictionary<string, string> GetCurrentKeyBindings()
    {
        return rebindButtons.ToDictionary(b => b.actionName, b => b.GetCurrentBinding());
    }
    public void ApplyKeyBindings(Dictionary<string, string> bindings)
    {
        foreach (var button in rebindButtons)
        {
            if (bindings.TryGetValue(button.actionRef.action.name, out var path))
            {
                button.actionRef.action.ApplyBindingOverride(button.bindingIndex, path);
                button.UpdateKeyText(); // 키 텍스트 갱신 (선택)
            }
        }
    }
}