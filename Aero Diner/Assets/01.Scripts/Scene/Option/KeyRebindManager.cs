using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeyRebindManager : MonoBehaviour
{
    [SerializeField] private List<KeyRebindButton> rebindButtons;

    private bool isSaved = false;


    public bool HasUnsavedChanges()
    {
        foreach (var btn in rebindButtons)
        {
            var path = btn.GetCurrentPath();
            var saved = PlayerPrefs.GetString(btn.BindingSaveKey, null);
            if (path != saved) return true;
        }
        return false;
    }
    public void SaveAll()
    {
        foreach (var btn in rebindButtons)
        {
            btn.SaveBinding();
        }
        PlayerPrefs.Save();
        isSaved = true;
        Debug.Log("모든 키 바인딩 저장 완료");
    }

    public void CancelAll()
    {
        if (isSaved)
        {
            Debug.Log("저장됨 → 롤백 생략");
            isSaved = false; // 상태 초기화
            return;
        }

        foreach (var btn in rebindButtons)
        {
            btn.RevertToOriginal();
        }
        Debug.Log("모든 키 바인딩 롤백");
    }

    public void ResetAll()
    {
        foreach (var btn in rebindButtons)
        {
            btn.ResetToDefault();
            btn.SaveBinding();
        }
        isSaved = true;
        Debug.Log("모든 키 바인딩 기본값으로 리셋");
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