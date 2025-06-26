using System.Collections.Generic;
using UnityEngine;

public class KeyRebindManager : MonoBehaviour
{
    [SerializeField] private List<KeyRebindButton> rebindButtons;
    public static KeyRebindManager Instance { get; private set; }

    private bool isSaved = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
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
}